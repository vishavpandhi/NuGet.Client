// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.NuGetSdkResolver;
using Microsoft.Build.NuGetSdkResolver.Test;
using NuGet.Common;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Test.Utility;
using Xunit;

using Strings = Microsoft.Build.NuGetSdkResolver.Strings;

namespace NuGet.Build.SdkResolver.Test
{
    public class NuGetSdkResolverTests
    {
        [Fact]
        public void Resolve_Disabled_ByDefault()
        {
            // Arrange
            var globalJsonReader = new MockGlobalJsonReader(new Dictionary<string, string>());

            var environmentVariableReader = new MockEnvironmentVariableReader();

            var resolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var sdkReference = new SdkReference("Sdk1", "1.0.0", minimumVersion: string.Empty);

            var resolverContext = new MockSdkResolverContext();

            var resultFactory = new MockSdkResultFactory();

            // Act
            MockSdkResult result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            // Assert
            result.Success.Should().BeFalse();

            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.Path.Should().BeNull();
            result.Version.Should().BeNull();
        }

        [Fact]
        public void Resolve_Disabled_BySameEnvironmentVariableAsSdkResolver()
        {
            // Arrange
            var globalJsonReader = new MockGlobalJsonReader(new Dictionary<string, string>());

            var environmentVariableReader = new MockEnvironmentVariableReader
            {
                [NuGetSdkResolver.MSBuildEnableExperimentalNuGetSdkResolver] = bool.TrueString,
                [NuGetSdkResolver.MSBuildDisableNuGetSdkResolver] = bool.TrueString
            };

            var resolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var sdkReference = new SdkReference("Sdk1", "1.0.0", minimumVersion: string.Empty);

            var resolverContext = new MockSdkResolverContext();

            var resultFactory = new MockSdkResultFactory();

            // Act
            MockSdkResult result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            // Assert
            result.Success.Should().BeFalse();

            result.Errors
                .Should().ContainSingle()
                .Which
                .Should().Be(string.Format(CultureInfo.CurrentCulture, Strings.Error_DisabledExperimentalSdkResolver, sdkReference.Name, NuGetSdkResolver.MSBuildDisableNuGetSdkResolver));
            result.Warnings.Should().BeEmpty();
            result.Path.Should().BeNull();
            result.Version.Should().BeNull();
        }

        [Fact]
        public void Resolve_LogsError_WhenPackageNotFound()
        {
            // Arrange
            using var pathContext = new SimpleTestPathContext();

            var sdkReference = new SdkReference("Sdk1", "1.0.0", minimumVersion: string.Empty);

            var globalJsonReader = new MockGlobalJsonReader(new Dictionary<string, string>());

            var environmentVariableReader = new MockEnvironmentVariableReader
            {
                [NuGetSdkResolver.MSBuildEnableExperimentalNuGetSdkResolver] = bool.TrueString
            };

            var resolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader, resetResultCache: true);

            var resolverContext = new MockSdkResolverContext(projectPath: pathContext.WorkingDirectory);

            var resultFactory = new MockSdkResultFactory();

            // Act
            MockSdkResult result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            // Assert
            result.Success.Should().BeFalse();

            result.Errors.Should().ContainSingle()
                .Which
                .Should().Be(string.Format(CultureInfo.CurrentCulture, NuGet.Commands.Strings.Error_NoPackageVersionsExist, sdkReference.Name, "source"));

            result.Warnings.Should().BeEmpty();
            result.Path.Should().BeNull();
            result.Version.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Resolve_Succeeds_WhenPackageFound(bool useFeed)
        {
            // Arrange
            using var pathContext = new SimpleTestPathContext();

            var sdkReference = new SdkReference("Sdk1", "1.0.0", minimumVersion: string.Empty);

            var package = new SimpleTestPackageContext(sdkReference.Name, sdkReference.Version);
            package.AddFile("Sdk/Sdk.props", "<Project />");
            package.AddFile("Sdk/Sdk.targets", "<Project />");

            await SimpleTestPackageUtility.CreateFolderFeedV3Async(
                    useFeed ? pathContext.PackageSource : pathContext.UserPackagesFolder, // Create the package in the feed or in the global package folder
                    PackageSaveMode.Defaultv3,
                    package);

            var globalJsonReader = new MockGlobalJsonReader(new Dictionary<string, string>());

            var environmentVariableReader = new MockEnvironmentVariableReader
            {
                [NuGetSdkResolver.MSBuildEnableExperimentalNuGetSdkResolver] = bool.TrueString
            };

            var resolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader, resetResultCache: true);

            var resolverContext = new MockSdkResolverContext(projectPath: pathContext.WorkingDirectory);

            var resultFactory = new MockSdkResultFactory();

            // Act
            MockSdkResult result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            // Assert
            result.Success.Should().BeTrue();

            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.Path.Should().Be(Path.Combine(pathContext.UserPackagesFolder, sdkReference.Name.ToLowerInvariant(), sdkReference.Version, "Sdk"));
            result.Version.Should().Be(sdkReference.Version);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Resolve_UsesResultCache_WhenPackageFound(bool useFeed)
        {
            // Arrange
            using var pathContext = new SimpleTestPathContext();

            var sdkReference = new SdkReference("Sdk1", "1.0.0", minimumVersion: string.Empty);

            var package = new SimpleTestPackageContext(sdkReference.Name, sdkReference.Version);
            package.AddFile("Sdk/Sdk.props", "<Project />");
            package.AddFile("Sdk/Sdk.targets", "<Project />");

            await SimpleTestPackageUtility.CreateFolderFeedV3Async(
                    useFeed ? pathContext.PackageSource : pathContext.UserPackagesFolder, // Create the package in the feed or in the global package folder
                    PackageSaveMode.Defaultv3,
                    package);

            var globalJsonReader = new MockGlobalJsonReader(new Dictionary<string, string>());

            var environmentVariableReader = new MockEnvironmentVariableReader
            {
                [NuGetSdkResolver.MSBuildEnableExperimentalNuGetSdkResolver] = bool.TrueString
            };

            var resolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader, resetResultCache: true);

            var resolverContext = new MockSdkResolverContext(projectPath: pathContext.WorkingDirectory);

            var resultFactory = new MockSdkResultFactory();

            // Act
            MockSdkResult result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            string expected = Path.Combine(pathContext.UserPackagesFolder, sdkReference.Name.ToLowerInvariant(), sdkReference.Version, "Sdk");

            // Assert
            result.Success.Should().BeTrue();

            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.Path.Should().Be(expected);
            result.Version.Should().Be(sdkReference.Version);

            // Delete everything from disk and the second call to Resolve should just return a cached value
            pathContext.Dispose();

            result = resolver.Resolve(sdkReference, resolverContext, resultFactory) as MockSdkResult;

            result.Success.Should().BeTrue();

            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.Path.Should().Be(expected);
            result.Version.Should().Be(sdkReference.Version);
        }

        [Fact]
        public void TryGetLibraryIdentityFromSdkReference_LogsError_IfNoVersionSpecified()
        {
            // Arrange
            var globalJsonReader = new MockGlobalJsonReader(sdkVersions: null);

            var environmentVariableReader = new MockEnvironmentVariableReader();

            var sdkReference = new SdkReference("Sdk1", version: string.Empty, minimumVersion: string.Empty);

            var resolverContext = new MockSdkResolverContext();

            var sdkResolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var logger = new MockLogger();

            // Act
            bool result = sdkResolver.TryGetLibraryIdentityFromSdkReference(sdkReference, resolverContext, logger, out LibraryIdentity libraryIdentity);

            // Assert
            result.Should().BeFalse();

            logger.Errors.Should().ContainSingle()
                .Which
                .Should().Be(string.Format(CultureInfo.CurrentCulture, Strings.Error_NoSdkVersionSpecified, sdkReference.Name));

            logger.Warnings.Should().BeEmpty();

            libraryIdentity.Should().BeNull();
        }

        [Fact]
        public void TryGetLibraryIdentityFromSdkReference_LogsError_IfNoVersionSpecifiedInGlobalJson()
        {
            // Arrange
            string globalJsonFullPath = Path.Combine("folder", "global.json");

            var globalJsonReader = new MockGlobalJsonReader(sdkVersions: new Dictionary<string, string>(), globalJsonFullPath);

            var environmentVariableReader = new MockEnvironmentVariableReader();

            var sdkReference = new SdkReference("Sdk1", version: string.Empty, minimumVersion: string.Empty);

            var resolverContext = new MockSdkResolverContext();

            var sdkResolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var logger = new MockLogger();

            // Act
            bool result = sdkResolver.TryGetLibraryIdentityFromSdkReference(sdkReference, resolverContext, logger, out LibraryIdentity libraryIdentity);

            // Assert
            result.Should().BeFalse();

            logger.Errors.Should().ContainSingle()
                .Which
                .Should().Be(string.Format(CultureInfo.CurrentCulture, Strings.Error_NoSdkVersionSpecifiedInGlobalJson, sdkReference.Name, globalJsonFullPath));

            logger.Warnings.Should().BeEmpty();

            libraryIdentity.Should().BeNull();
        }

        [Fact]
        public void TryGetLibraryIdentityFromSdkReference_LogsWarning_WithInvalidNuGetVersion()
        {
            // Arrange
            var globalJsonReader = new MockGlobalJsonReader(sdkVersions: null);

            var environmentVariableReader = new MockEnvironmentVariableReader();

            var sdkReference = new SdkReference("Sdk1", version: "Invalid", minimumVersion: string.Empty);

            var resolverContext = new MockSdkResolverContext();

            var sdkResolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var logger = new MockLogger();

            // ACt
            bool result = sdkResolver.TryGetLibraryIdentityFromSdkReference(sdkReference, resolverContext, logger, out LibraryIdentity libraryIdentity);

            // Assert
            result.Should().BeFalse();

            logger.Errors.Should().BeEmpty();

            logger.Warnings.Should().ContainSingle()
                .Which
                .Should().Be(string.Format(CultureInfo.CurrentCulture, Strings.SdkVersionIsNotValidNuGetVersion, sdkReference.Name, sdkReference.Version));

            libraryIdentity.Should().BeNull();
        }

        [Theory]
        [InlineData(true, "1.02.03", "1.2.3")]
        [InlineData(false, "1.02.03", "1.2.3")]
        public void TryGetLibraryIdentityFromSdkReference_ReturnsTrue_WithValidNuGetPackageIdAndVersion(bool versionFromGlobalJson, string version, string expectedVersion)
        {
            // Arrange
            var sdkReference = new SdkReference("Sdk1", version: versionFromGlobalJson ? string.Empty : version, minimumVersion: string.Empty);

            var sdkVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (versionFromGlobalJson)
            {
                sdkVersions.Add(sdkReference.Name, version);
            }

            var globalJsonReader = new MockGlobalJsonReader(sdkVersions);

            var environmentVariableReader = new MockEnvironmentVariableReader();

            var resolverContext = new MockSdkResolverContext();

            var sdkResolver = new NuGetSdkResolver(globalJsonReader, environmentVariableReader);

            var logger = new MockLogger();

            // Act
            bool result = sdkResolver.TryGetLibraryIdentityFromSdkReference(sdkReference, resolverContext, logger, out LibraryIdentity libraryIdentity);

            // Assert
            result.Should().BeTrue();

            logger.Errors.Should().BeEmpty();

            logger.Warnings.Should().BeEmpty();

            libraryIdentity.Should().NotBeNull();
            libraryIdentity.Name.Should().Be(sdkReference.Name);
            libraryIdentity.Version.ToNormalizedString().Should().Be(expectedVersion);
            libraryIdentity.Version.OriginalVersion.Should().Be(version);
            libraryIdentity.Type.Should().Be(LibraryType.Package);
        }
    }
}
