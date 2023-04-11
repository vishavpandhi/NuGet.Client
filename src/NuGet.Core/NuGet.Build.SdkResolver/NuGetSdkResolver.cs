// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.NuGetSdkResolver;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Common.Migrations;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.DependencyResolver;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.RuntimeModel;
using NuGet.Versioning;

using ILogger = NuGet.Common.ILogger;
using SdkResolverBase = Microsoft.Build.Framework.SdkResolver;
using SdkResultBase = Microsoft.Build.Framework.SdkResult;
using Strings = Microsoft.Build.NuGetSdkResolver.Strings;

namespace NuGet.Build.SdkResolver
{
    /// <summary>
    /// Represents a NuGet-based MSBuild project SDK resolver.
    /// </summary>
    public sealed class NuGetSdkResolver : SdkResolverBase
    {
        /// <summary>
        /// Stores a cache that stores results for by a <see cref="LibraryIdentity" />.
        /// </summary>
        internal static readonly ConcurrentDictionary<LibraryIdentity, Lazy<SdkResultBase>> ResultCache = new ConcurrentDictionary<LibraryIdentity, Lazy<SdkResultBase>>();

        /// <summary>
        /// Respresents an environment variable a user can set to disable this SDK resolver.
        /// </summary>
        internal const string MSBuildDisableNuGetSdkResolver = nameof(MSBuildDisableNuGetSdkResolver);

        /// <summary>
        /// Respresents an environment variable a user can set to enable this SDK resolver.
        /// </summary>
        internal const string MSBuildEnableExperimentalNuGetSdkResolver = nameof(MSBuildEnableExperimentalNuGetSdkResolver);

        /// <summary>
        /// Stores a <see cref="LocalPackageFileCache" /> instance for cache package file look ups.
        /// </summary>
        private static readonly LocalPackageFileCache LocalPackageFileCache = new LocalPackageFileCache();

        /// <summary>
        /// Stores an <see cref="IMachineWideSettings" /> instance used for reading machine-wide settings.
        /// </summary>
        private static readonly Lazy<IMachineWideSettings> MachineWideSettingsLazy = new Lazy<IMachineWideSettings>(() => new XPlatMachineWideSetting());

        /// <summary>
        /// Stores a <see cref="SettingsLoadingContext" /> instance used to cache the loading of settings.
        /// </summary>
        private static readonly SettingsLoadingContext SettingsLoadContext = new SettingsLoadingContext();

        /// <summary>
        /// Stores a <see cref="SemaphoreSlim" /> instance used to ensure that this SDK resolver is only ever resolving one SDK at a time.
        /// </summary>
        private static readonly SemaphoreSlim SingleResolutionSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        /// <summary>
        /// Stores a value indicating whether or not this SDK resolver has been disabled.
        /// </summary>
        private readonly bool _disableNuGetSdkResolver = false;

        /// <summary>
        /// Stores a value indicating whether or not this SDK resolver has been enabled.
        /// </summary>
        private readonly bool _enableExperimentalNuGetSdkResolver = false;

        /// <summary>
        /// Stores a <see cref="IGlobalJsonReader" /> instance used to read a global.json.
        /// </summary>
        private readonly IGlobalJsonReader _globalJsonReader;

        static NuGetSdkResolver()
        {
            SettingsLoadContext.FileRead += (sender, path) => NuGetSdkResolverEventSource.Instance.SettingsFileRead(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetSdkResolver" /> class.
        /// </summary>
        public NuGetSdkResolver()
            : this(new GlobalJsonReader(), EnvironmentVariableWrapper.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetSdkResolver" /> class with the specified <see cref="IGlobalJsonReader" />.
        /// </summary>
        /// <param name="globalJsonReader">An <see cref="IGlobalJsonReader" /> to use when reading a global.json file.</param>
        /// <param name="environmentVariableReader">An <see cref="IEnvironmentVariableReader" /> to use when reading environment variables.</param>
        /// <param name="resetResultCache"><see langword="true" /> to reset the result cache, otherwise <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="globalJsonReader" /> is <see langword="null" />.</exception>
        internal NuGetSdkResolver(IGlobalJsonReader globalJsonReader, IEnvironmentVariableReader environmentVariableReader, bool resetResultCache = false)
        {
            _globalJsonReader = globalJsonReader ?? throw new ArgumentNullException(nameof(globalJsonReader));

            // Determine if the experimental NuGet-based MSBuild project SDK resolver has been enabled
            _enableExperimentalNuGetSdkResolver = IsFeatureFlagEnabled(environmentVariableReader, MSBuildEnableExperimentalNuGetSdkResolver);

            // Determine if the NuGet-based MSBuild project SDK resolver has been disabled
            _disableNuGetSdkResolver = IsFeatureFlagEnabled(environmentVariableReader, MSBuildDisableNuGetSdkResolver);

            if (!_disableNuGetSdkResolver && _enableExperimentalNuGetSdkResolver)
            {
                _globalJsonReader.FileReadStart += (sender, args) => NuGetSdkResolverEventSource.Instance.GlobalJsonReadStart(args.FullPath, args.ProjectFullPath, args.SolutionFullPath);
                _globalJsonReader.FileReadStop += (sender, args) => NuGetSdkResolverEventSource.Instance.GlobalJsonReadStop(args.FullPath, args.ProjectFullPath, args.SolutionFullPath);
            }

            if (resetResultCache)
            {
                ResultCache.Clear();
            }
        }

        /// <inheritdoc />
        public override string Name => "NuGet.Build.SdkResolver";

        /// <inheritdoc />
        public override int Priority => 5999;

        /// <inheritdoc />
        public override SdkResultBase Resolve(SdkReference sdkReference, SdkResolverContext resolverContext, SdkResultFactory resultFactory)
        {
            try
            {
                // If the experimental NuGet-based MSBuild project SDK resolver is not enabled, just return to MSBuild that nothing was resolved
                // so that the other resolvers can run.
                if (!_enableExperimentalNuGetSdkResolver)
                {
                    return resultFactory.IndicateFailure(errors: null, warnings: null);
                }

                // Feature flag to disable this resolver
                if (_disableNuGetSdkResolver)
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve the SDK "{0}" because the resolver is disabled by the {1} environment variable.
                    return resultFactory.IndicateFailure(new List<string>(1) { string.Format(CultureInfo.CurrentCulture, Strings.Error_DisabledExperimentalSdkResolver, sdkReference.Name, MSBuildDisableNuGetSdkResolver) });
                }

                NuGetSdkResolverEventSource.Instance.ResolveStart(sdkReference.Name, sdkReference.Version);

                // The main logger which logs messages back to MSBuild
                var sdkLogger = new NuGetSdkLogger(resolverContext.Logger);

                // A forwarding logger that logs messages to the main logger and the event source logger
                ILogger logger = new ForwardingLogger(sdkLogger, NuGetSdkResolverEventSource.Logger);

                // Try to see if a version is specified in the project or in a global.json.  The method will log a reason why a version wasn't found
                if (!TryGetLibraryIdentityFromSdkReference(sdkReference, resolverContext, logger, out LibraryIdentity libraryIdentity))
                {
                    return resultFactory.IndicateFailure(sdkLogger.Errors, sdkLogger.Warnings);
                }

                Lazy<SdkResultBase> resultLazy = ResultCache.GetOrAdd(
                    libraryIdentity,
                    (key) => new Lazy<SdkResultBase>(() => GetResult(sdkReference, resolverContext, resultFactory, key, sdkLogger, logger)));

                SdkResultBase result = resultLazy.Value;

                return result;
            }
            catch (Exception e)
            {
                return resultFactory.IndicateFailure(errors: new[] { Strings.UnhandledException, e.ToString() });
            }
            finally
            {
                NuGetSdkResolverEventSource.Instance.ResolveStop(sdkReference.Name, sdkReference.Version);
            }
        }

        /// <summary>
        /// Determines if a feature flag is enabled by reading the environment variable with the specified name.
        /// </summary>
        /// <param name="environmentVariableReader">The <see cref="IEnvironmentVariableReader" /> to use when reading environment variables.</param>
        /// <param name="name">The name of the environment variable to read.</param>
        /// <returns><see langword="true" /> if the specified feature flag has a value of &quot;1&quot; or &quot;true&quot;, otherwise <see langword="false" />.</returns>
        internal static bool IsFeatureFlagEnabled(IEnvironmentVariableReader environmentVariableReader, string name)
        {
            string value = environmentVariableReader.GetEnvironmentVariable(name);

            return string.Equals(value, "1", StringComparison.Ordinal) || string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        internal SdkResultBase GetResult(SdkReference sdkReference, SdkResolverContext resolverContext, SdkResultFactory resultFactory, LibraryIdentity libraryIdentity, NuGetSdkLogger sdkLogger, ILogger logger)
        {
            logger.LogVerbose(string.Format(CultureInfo.CurrentCulture, Strings.LocatingSdk, libraryIdentity.Name, libraryIdentity.Version.OriginalVersion));

            NuGetSdkResolverEventSource.Instance.GetResultStart(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

            SdkResultBase result = null;

            try
            {
                NuGetSdkResolverEventSource.Instance.MigrationRunnerStart();
                try
                {
                    MigrationRunner.Run();
                }
                finally
                {
                    NuGetSdkResolverEventSource.Instance.MigrationRunnerStop();
                }

                NuGetSdkResolverEventSource.Instance.LoadSettingsStart();
                ISettings settings;
                try
                {
                    settings = Settings.LoadDefaultSettings(resolverContext.ProjectFilePath, configFileName: null, MachineWideSettingsLazy.Value, SettingsLoadContext);
                }
                catch (Exception e)
                {
                    sdkLogger.LogError(string.Format(CultureInfo.CurrentCulture, Strings.Error_FailedToReadSettings, e.Message));

                    return resultFactory.IndicateFailure(sdkLogger.Errors, sdkLogger.Warnings);
                }
                finally
                {
                    NuGetSdkResolverEventSource.Instance.LoadSettingsStop();
                }

                var versionFolderPathResolver = new VersionFolderPathResolver(SettingsUtility.GetGlobalPackagesFolder(settings));

                string installPath = GetSdkPackageInstallPath(sdkReference.Name, libraryIdentity.Version, versionFolderPathResolver);

                if (!string.IsNullOrWhiteSpace(installPath))
                {
                    // The package is already on disk so return the path to it
                    result = resultFactory.IndicateSuccess(installPath, libraryIdentity.Version.ToNormalizedString(), sdkLogger.Warnings);
                }
                else
                {
                    // Restore the package from the configured feeds and return the path to the package on disk
                    result = RestorePackageAsync(libraryIdentity, resolverContext, resultFactory, settings, versionFolderPathResolver, logger, sdkLogger).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
                }
            }
            finally
            {
                NuGetSdkResolverEventSource.Instance.GetResultStop(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion, result?.Path, result == null ? false : result.Success);
            }

            return result;
        }

        /// <summary>
        /// Attempts to determine a version to use for the specified MSBuild project SDK.
        /// </summary>
        /// <param name="sdkReference">An <see cref="SdkReference" /> containing details about the MSBuild project SDK.</param>
        /// <param name="resolverContext">An <see cref="SdkResolverContext" /> representing the context under which the MSBuild project SDK is being resolved.</param>
        /// <param name="logger">An <see cref="ILogger" /> to use to log any messages.</param>
        /// <param name="libraryIdentity">Receives a <see cref="LibraryIdentity" /> for the specified MSBuild project SDK if one was found, otherwise <see langword="null" />.</param>
        /// <returns><see langword="true" /> if a version was found for the specified MSBuild project SDK, otherwise <see langword="false" />.</returns>
        internal bool TryGetLibraryIdentityFromSdkReference(SdkReference sdkReference, SdkResolverContext resolverContext, ILogger logger, out LibraryIdentity libraryIdentity)
        {
            // This resolver only works if the user specifies a version in a project or a global.json.
            string sdkVersion = sdkReference.Version;

            libraryIdentity = null;

            if (string.IsNullOrWhiteSpace(sdkVersion))
            {
                Dictionary<string, string> msbuildSdkVersions = _globalJsonReader.GetMSBuildSdkVersions(resolverContext, out string globalJsonFullPath);

                if (msbuildSdkVersions == null)
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve the SDK "{0}" because there was no version specified in the project or global.json.
                    logger.LogError(string.Format(CultureInfo.CurrentCulture, Strings.Error_NoSdkVersionSpecified, sdkReference.Name));

                    return false;
                }

                if (!msbuildSdkVersions.TryGetValue(sdkReference.Name, out sdkVersion))
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve the SDK "{0}" because there was no version specified the file "{1}".
                    logger.LogError(string.Format(CultureInfo.CurrentCulture, Strings.Error_NoSdkVersionSpecifiedInGlobalJson, sdkReference.Name, globalJsonFullPath));

                    return false;
                }
            }

            // Ignore invalid versions, there may be another resolver that can handle the version specified
            if (!NuGetVersion.TryParse(sdkVersion, out NuGetVersion nuGetVersion))
            {
                // The NuGet-based MSBuild project SDK resolver did not resolve SDK "{0}" because the version specified "{1}" is not a valid NuGet version.
                logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Strings.SdkVersionIsNotValidNuGetVersion, sdkReference.Name, sdkVersion));

                return false;
            }

            libraryIdentity = new LibraryIdentity(sdkReference.Name, nuGetVersion, LibraryType.Package);

            return true;
        }

        /// <summary>
        /// Gets the full path to the MSBuild project SDK for
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="versionFolderPathResolver"></param>
        /// <returns></returns>
        private static string GetSdkPackageInstallPath(string id, NuGetVersion version, VersionFolderPathResolver versionFolderPathResolver)
        {
            string installPath = versionFolderPathResolver.GetInstallPath(id, version);

            if (string.IsNullOrWhiteSpace(installPath))
            {
                return null;
            }

            string sdkPath = Path.Combine(installPath, "Sdk");

            if (Directory.Exists(sdkPath))
            {
                return sdkPath;
            }

            sdkPath = Path.Combine(installPath, "sdk");

            if (Directory.Exists(sdkPath))
            {
                return sdkPath;
            }

            return null;
        }

        /// <summary>
        /// Restores an MSBuild project SDK NuGet package.
        /// </summary>
        /// <param name="libraryIdentity">The <see cref="LibraryIdentity" /> of the NuGet package.</param>
        /// <param name="context">The <see cref="SdkResolverContext" /> under which the MSBuild project SDK is being resolved.</param>
        /// <param name="factory">An <see cref="SdkResultFactory" /> to use when creating a result</param>
        /// <param name="settings">The <see cref="ISettings" /> to use when locating the package.</param>
        /// <param name="versionFolderPathResolver">A <see cref="VersionFolderPathResolver" /> to use when locating the package.</param>
        /// <param name="logger">A <see cref="ILogger" /> to use when logging messages.</param>
        /// <param name="sdkLogger">A <see cref="NuGetSdkLogger" /> to use when logging errors or warnings.</param>
        /// <returns>An <see cref="Task{SdkResultBase}" /> representing the details of the package if it was found or errors if any occured.</returns>
        private async Task<SdkResultBase> RestorePackageAsync(LibraryIdentity libraryIdentity, SdkResolverContext context, SdkResultFactory factory, ISettings settings, VersionFolderPathResolver versionFolderPathResolver, ILogger logger, NuGetSdkLogger sdkLogger)
        {
            NuGetSdkResolverEventSource.Instance.WaitForRestoreSemaphoreStart(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

            // Only ever resolve one package at a time to reduce the possibilty of thread starvation
            await SingleResolutionSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                NuGetSdkResolverEventSource.Instance.WaitForRestoreSemaphoreStop(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

                NuGetSdkResolverEventSource.Instance.RestorePackageStart(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

                // Downloading SDK package "{0}" version "{1}"...
                logger.LogVerbose(string.Format(CultureInfo.CurrentCulture, Strings.DownloadingPackage, libraryIdentity.Name, libraryIdentity.Version.OriginalVersion));

                DefaultCredentialServiceUtility.SetupDefaultCredentialService(logger, nonInteractive: !context.Interactive);

#if !NETFRAMEWORK
                X509TrustStore.InitializeForDotNetSdk(logger);
#endif

                using (var sourceCacheContext = new SourceCacheContext
                {
                    IgnoreFailedSources = true,
                })
                {
                    var packageSourceProvider = new PackageSourceProvider(settings);

                    var cachingSourceProvider = new CachingSourceProvider(packageSourceProvider);

                    var remoteWalkContext = new RemoteWalkContext(cacheContext: sourceCacheContext, packageSourceMapping: PackageSourceMapping.GetPackageSourceMapping(settings), logger);

                    foreach (SourceRepository source in SettingsUtility.GetEnabledSources(settings).Select(i => cachingSourceProvider.CreateRepository(i)))
                    {
                        SourceRepositoryDependencyProvider remoteProvider = new SourceRepositoryDependencyProvider(
                            source,
                            logger,
                            sourceCacheContext,
                            sourceCacheContext.IgnoreFailedSources,
                            ignoreWarning: false,
                            fileCache: LocalPackageFileCache,
                            isFallbackFolderSource: false);

                        remoteWalkContext.RemoteLibraryProviders.Add(remoteProvider);
                    }

                    var walker = new RemoteDependencyWalker(remoteWalkContext);

                    GraphNode<RemoteResolveResult> result = await walker.WalkAsync(libraryIdentity, FrameworkConstants.CommonFrameworks.Net45, null, RuntimeGraph.Empty, recursive: false).ConfigureAwait(continueOnCapturedContext: false);

                    RemoteMatch match = result.Item.Data.Match;

                    if (match == null || match.Library.Type == LibraryType.Unresolved)
                    {
                        RestoreLogMessage message = await UnresolvedMessages.GetMessageAsync(
                            "any/any",
                            libraryIdentity,
                            remoteWalkContext.FilterDependencyProvidersForLibrary(match.Library),
                            remoteWalkContext.PackageSourceMapping.IsEnabled,
                            remoteWalkContext.RemoteLibraryProviders,
                            remoteWalkContext.CacheContext,
                            remoteWalkContext.Logger,
                            CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);

                        logger.Log(message);

                        return factory.IndicateFailure(sdkLogger.Errors, sdkLogger.Warnings);
                    }

                    var packageIdentity = new PackageIdentity(match.Library.Name, match.Library.Version);

                    ClientPolicyContext clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);

                    var packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, PackageExtractionBehavior.XmlDocFileSaveMode, clientPolicyContext, logger);

                    using (IPackageDownloader downloader = await match.Provider.GetPackageDownloaderAsync(packageIdentity, sourceCacheContext, logger, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        bool installed = await PackageExtractor.InstallFromSourceAsync(
                            packageIdentity,
                            downloader,
                            versionFolderPathResolver,
                            packageExtractionContext,
                            CancellationToken.None,
                            parentId: default).ConfigureAwait(continueOnCapturedContext: false);

                        if (installed)
                        {
                            string installPath = GetSdkPackageInstallPath(packageIdentity.Id, packageIdentity.Version, versionFolderPathResolver);

                            if (!string.IsNullOrWhiteSpace(installPath))
                            {
                                // Successfully downloaded SDK package "{0}" version "{1}" to "{2}".
                                logger.LogVerbose(string.Format(CultureInfo.CurrentCulture, Strings.SuccessfullyDownloadedPackage, libraryIdentity.Name, libraryIdentity.Version.OriginalVersion, installPath));

                                return factory.IndicateSuccess(installPath, packageIdentity.Version.ToNormalizedString(), sdkLogger.Warnings);
                            }
                        }
                    }
                }

                return factory.IndicateFailure(sdkLogger.Errors, sdkLogger.Warnings);
            }
            finally
            {
                DefaultCredentialServiceUtility.UpdateCredentialServiceDelegatingLogger(NullLogger.Instance);

                NuGetSdkResolverEventSource.Instance.RestorePackageStop(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

                SingleResolutionSemaphore.Release();
            }
        }
    }
}
