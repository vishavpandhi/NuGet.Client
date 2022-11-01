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
using NuGet.Commands;
using NuGet.Common;
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

namespace Microsoft.Build.NuGetSdkResolver
{
    /// <summary>
    /// Represents a NuGet-based MSBuild project SDK resolver.
    /// </summary>
    public sealed class NuGetSdkResolver : SdkResolver
    {
        /// <summary>
        /// Stores a cache that stores results for by a <see cref="LibraryIdentity" />.
        /// </summary>
        internal static readonly ConcurrentDictionary<LibraryIdentity, Lazy<SdkResult>> ResultCache = new ConcurrentDictionary<LibraryIdentity, Lazy<SdkResult>>();

        /// <summary>
        /// Respresents an environment variable a user can set to disable this SDK resolver.
        /// </summary>
        private const string MSBuildDisableNuGetSdkResolver = nameof(MSBuildDisableNuGetSdkResolver);

        /// <summary>
        /// Stores a value indicating whether or not this SDK resolver has been disabled.
        /// </summary>
        private static readonly Lazy<bool> DisableNuGetSdkResolverLazy = new Lazy<bool>(IsDisabled);

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
            : this(new GlobalJsonReader())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetSdkResolver" /> class with the specified <see cref="IGlobalJsonReader" />.
        /// </summary>
        /// <param name="globalJsonReader">An <see cref="IGlobalJsonReader" /> to use when reading a global.json file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="globalJsonReader" /> is <c>null</c>.</exception>
        internal NuGetSdkResolver(IGlobalJsonReader globalJsonReader)
        {
            _globalJsonReader = globalJsonReader ?? throw new ArgumentNullException(nameof(globalJsonReader));
        }

        /// <inheritdoc />
        public override string Name => nameof(NuGetSdkResolver);

        /// <inheritdoc />
        public override int Priority => 6000;

        /// <inheritdoc />
        public override SdkResult Resolve(SdkReference sdkReference, SdkResolverContext resolverContext, SdkResultFactory factory)
        {
            NuGetSdkResolverEventSource.Instance.ResolveStart(sdkReference.Name, sdkReference.Version);

            try
            {
                // The main logger which logs messages back to MSBuild
                var sdkLogger = new NuGetSdkLogger(resolverContext.Logger);

                // A forwarding logger that logs messages to the main logger and the event source logger
                var logger = new ForwardingLogger(sdkLogger, NuGetSdkResolverEventSource.Logger);

                // Escape hatch to disable this resolver
                if (DisableNuGetSdkResolverLazy.Value)
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve SDK "{0}" because the resolver is disabled by the {1} environment variable.
                    logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Strings.SdkResolverIsDisabled, sdkReference.Name, MSBuildDisableNuGetSdkResolver));

                    return factory.IndicateFailure(errors: sdkLogger.Errors, warnings: sdkLogger.Warnings);
                }

                // Try to see if a version is specified in the project or in a global.json.  The TryGetNuGetVersionForSdk method will log a reason why a version wasn't found
                if (!TryGetNuGetVersionForSdk(sdkReference, resolverContext, logger, out NuGetVersion nuGetVersion))
                {
                    return factory.IndicateFailure(errors: sdkLogger.Errors, warnings: sdkLogger.Warnings);
                }

                NuGet.Common.Migrations.MigrationRunner.Run();

                var libraryIdentity = new LibraryIdentity(sdkReference.Name, nuGetVersion, LibraryType.Package);

                Lazy<SdkResult> resultLazy = ResultCache.GetOrAdd(
                    libraryIdentity,
                    (key) => new Lazy<SdkResult>(() => GetResult(sdkReference, resolverContext, factory, key, nuGetVersion, sdkLogger, logger)));

                SdkResult result = resultLazy.Value;

                return result;
            }
            catch (Exception e)
            {
                return factory.IndicateFailure(errors: new[] { Strings.UnhandledException, e.ToString() });
            }
            finally
            {
                NuGetSdkResolverEventSource.Instance.ResolveStop(sdkReference.Name, sdkReference.Version);
            }
        }

        internal SdkResult GetResult(SdkReference sdkReference, SdkResolverContext resolverContext, SdkResultFactory factory, LibraryIdentity libraryIdentity, NuGetVersion nuGetVersion, NuGetSdkLogger sdkLogger, ILogger logger)
        {
            // Locating MSBuild project SDK "{0}" version "{1}"...
            logger.LogVerbose(string.Format(CultureInfo.CurrentCulture, Strings.LocatingSdk, libraryIdentity.Name, libraryIdentity.Version.OriginalVersion));

            NuGetSdkResolverEventSource.Instance.GetResultStart(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion);

            SdkResult result;

            NuGetSdkResolverEventSource.Instance.LoadSettingsStart();
            ISettings settings;
            try
            {
                settings = Settings.LoadDefaultSettings(resolverContext.ProjectFilePath, configFileName: null, MachineWideSettingsLazy.Value, SettingsLoadContext);
            }
            catch (Exception e)
            {
                sdkLogger.LogError(string.Format(CultureInfo.CurrentCulture, Strings.Error_FailedToReadSettings, e.Message));

                return factory.IndicateFailure(sdkLogger.Errors, sdkLogger.Warnings);
            }
            finally
            {
                NuGetSdkResolverEventSource.Instance.LoadSettingsStop();
            }

            var versionFolderPathResolver = new VersionFolderPathResolver(SettingsUtility.GetGlobalPackagesFolder(settings));

            string installPath = GetSdkPackageInstallPath(sdkReference.Name, nuGetVersion, versionFolderPathResolver);

            if (!string.IsNullOrWhiteSpace(installPath))
            {
                result = factory.IndicateSuccess(installPath, nuGetVersion.ToNormalizedString(), sdkLogger.Warnings);
            }
            else
            {
                result = RestorePackageAsync(libraryIdentity, resolverContext, factory, settings, versionFolderPathResolver, logger, sdkLogger).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
            }

            NuGetSdkResolverEventSource.Instance.GetResultStop(libraryIdentity.Name, libraryIdentity.Version.OriginalVersion, result.Path, result.Success);

            return result;
        }

        /// <summary>
        /// Attempts to determine a version to use for the specified MSBuild project SDK.
        /// </summary>
        /// <param name="sdkReference">An <see cref="SdkReference" /> containing details about the MSBuild project SDK.</param>
        /// <param name="resolverContext">An <see cref="SdkResolverContext" /> representing the context under which the MSBuild project SDK is being resolved.</param>
        /// <param name="logger">An <see cref="ILogger" /> to use to log any messages.</param>
        /// <param name="nuGetVersion">Receives a <see cref="NuGetVersion" /> for the specified MSBuild project SDK if one was found, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if a version was found for the specified MSBuild project SDK, otherwise <c>false</c>.</returns>
        internal bool TryGetNuGetVersionForSdk(SdkReference sdkReference, SdkResolverContext resolverContext, ILogger logger, out NuGetVersion nuGetVersion)
        {
            // This resolver only works if the user specifies a version in a project or a global.json.
            string sdkVersion = sdkReference.Version;

            nuGetVersion = null;

            if (string.IsNullOrWhiteSpace(sdkVersion))
            {
                Dictionary<string, string> msbuildSdkVersions = _globalJsonReader.GetMSBuildSdkVersions(resolverContext, out string globalJsonFullPath);

                if (msbuildSdkVersions == null)
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve the SDK "{0}" because there was no version specified in the project or global.json.
                    logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Strings.NoSdkVersionSpecified, sdkReference.Name));

                    return false;
                }

                if (!msbuildSdkVersions.TryGetValue(sdkReference.Name, out sdkVersion))
                {
                    // The NuGet-based MSBuild project SDK resolver did not resolve the SDK "{0}" because there was no version specified the file "{1}".
                    logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Strings.NoSdkVersionSpecifiedInGlobalJson, sdkReference.Name, globalJsonFullPath));

                    return false;
                }
            }

            // Ignore invalid versions, there may be another resolver that can handle the version specified
            if (!NuGetVersion.TryParse(sdkVersion, out nuGetVersion))
            {
                // The NuGet-based MSBuild project SDK resolver did not resolve SDK "{0}" because the version specified "{1}" is not a valid NuGet version.
                logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Strings.SdkVersionIsNotValidNuGetVersion, sdkReference.Name, sdkVersion));

                return false;
            }

            return true;
        }

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
        /// Determines if this SDK resolver has been disabled by the user.
        /// </summary>
        /// <returns><c>true</c> if the SDK resolver has been disabled, otherwise <c>false</c>.</returns>
        private static bool IsDisabled()
        {
            string value = Environment.GetEnvironmentVariable(MSBuildDisableNuGetSdkResolver);

            return string.Equals(value, "1", StringComparison.Ordinal) || string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
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
        /// <returns>An <see cref="Task{SdkResult}" /> representing the details of the package if it was found or errors if any occured.</returns>
        private async Task<SdkResult> RestorePackageAsync(LibraryIdentity libraryIdentity, SdkResolverContext context, SdkResultFactory factory, ISettings settings, VersionFolderPathResolver versionFolderPathResolver, ILogger logger, NuGetSdkLogger sdkLogger)
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
