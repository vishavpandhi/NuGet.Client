// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit;

namespace NuGet.VisualStudio.Common.Test
{
    public class NuGetFeedbackDiagnosticFileProviderTests
    {
        //private NuGetFeedbackDiagnosticFileProvider _target;
        //private Mock<ISolutionManager> _solutionManager;
        //private Mock<INuGetTelemetryProvider> _telemetryProvider;
        //private Mock<ISettings> _settings;

        public NuGetFeedbackDiagnosticFileProviderTests()
        {
            //_solutionManager = new Mock<ISolutionManager>();
            //_solutionManager.Setup(sm => sm.GetNuGetProjectsAsync())
            //    .Returns(Task.FromResult<IEnumerable<NuGetProject>>(Array.Empty<NuGetProject>())); // empty or no solution

            //_telemetryProvider = new Mock<INuGetTelemetryProvider>();

            //_settings = new Mock<ISettings>();

            //_target = new NuGetFeedbackDiagnosticFileProvider();
            //_target.SolutionManager = _solutionManager.Object;
            //_target.TelemetryProvider = _telemetryProvider.Object;
            //_target.Settings = _settings.Object;
        }

        [Fact]
        public void GetFilesTest()
        {
            // Arrange
            var provider = new NuGetFeedbackDiagnosticFileProvider();

            // Act
            var files = provider.GetFiles();

            // Assert
            foreach (var file in files)
            {
                Assert.True(Path.IsPathRooted(file));
            }
        }

        //        [Fact]
        //        public async Task WriteToZipAsync_WithMSSource_SourceRemainsStill()
        //        {
        //            // Arrange
        //            var projectName = "testproj";
        //            var dgSpecFileName = "dgspec.json";

        //            using (var solutionManager = new TestSolutionManager())
        //            {
        //                var projectFolder = new DirectoryInfo(Path.Combine(solutionManager.SolutionDirectory, projectName));
        //                projectFolder.Create();
        //                var projectConfig = new FileInfo(Path.Combine(projectFolder.FullName, "project.json"));
        //                var msbuildProjectPath = new FileInfo(Path.Combine(projectFolder.FullName, $"{projectName}.csproj"));
        //                string extractPath = Path.Combine(solutionManager.SolutionDirectory);

        //                BuildIntegrationTestUtility.CreateConfigJson(projectConfig.FullName);
        //                var json = JObject.Parse(File.ReadAllText(projectConfig.FullName));

        //                JsonConfigUtility.AddDependency(json, new PackageDependency("nuget.versioning", VersionRange.Parse("1.0.7")));

        //                using (var writer = new StreamWriter(projectConfig.FullName))
        //                {
        //                    writer.Write(json.ToString());
        //                }

        //                var sources = new List<SourceRepository> { };
        //                var testLogger = new TestLogger();
        //                var settings = Settings.LoadSpecificSettings(solutionManager.SolutionDirectory, "NuGet.Config");
        //                var project = new ProjectJsonNuGetProject(projectConfig.FullName, msbuildProjectPath.FullName);

        //                solutionManager.NuGetProjects.Add(project);

        //                var restoreContext = new DependencyGraphCacheContext(testLogger, settings);
        //                var providersCache = new RestoreCommandProvidersCache();
        //                DependencyGraphSpec dgSpec = await DependencyGraphRestoreUtility.GetSolutionRestoreSpec(solutionManager, restoreContext);

        //                using var stream = new MemoryStream();
        //                _target.Settings = settings;
        //                _target.SolutionManager = solutionManager;

        //                // Act
        //                await _target.WriteToZipAsync(stream);

        //                // Assert
        //                using (var zip = new ZipArchive(stream))
        //                {
        //                    IEnumerable<string> zipFiles = zip.Entries.Select(e => e.FullName);
        //                    var expectedFiles = new[] { dgSpecFileName };

        //                    Assert.Equal(zipFiles.OrderBy(f => f), expectedFiles);

        //                    foreach (ZipArchiveEntry entry in zip.Entries)
        //                    {
        //                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
        //                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
        //                            entry.ExtractToFile(destinationPath);
        //                    }
        //                }

        //                DependencyGraphSpec vsFeedbackDgSpec = DependencyGraphSpec.Load(Path.Combine(extractPath, dgSpecFileName));
        //                Assert.Equal(dgSpec.Projects.Count, vsFeedbackDgSpec.Projects.Count);
        //                Assert.Equal(dgSpec.Projects[0].RestoreMetadata.Sources.Count, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources.Count);
        //                Assert.Equal(dgSpec.Projects[0].RestoreMetadata.Sources[0].Source, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources[0].Source);
        //                // dgSpec.Save replaces source name with source.
        //                Assert.Equal(dgSpec.Projects[0].RestoreMetadata.Sources[0].Source, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources[0].Name);
        //            }
        //        }

        //        [Fact]
        //        public async Task WriteToZipAsync_WithNonMSSource_SourceHashed()
        //        {
        //            // Arrange
        //            var projectName = "testproj";
        //            var dgSpecFileName = "dgspec.json";

        //            using (var solutionManager = new TestSolutionManager())
        //            {
        //                var privateRepositoryPath = Path.Combine(solutionManager.TestDirectory, "SharedRepository");
        //                Directory.CreateDirectory(privateRepositoryPath);

        //                var configPath = Path.Combine(solutionManager.TestDirectory, "nuget.config");
        //                SettingsTestUtils.CreateConfigurationFile(configPath, $@"<?xml version=""1.0"" encoding=""utf-8""?>
        //<configuration>
        //    <packageSources>
        //    <!--To inherit the global NuGet package sources remove the <clear/> line below -->
        //    <clear />
        //    <add key=""PrivateRepository"" value=""{privateRepositoryPath}"" />
        //    </packageSources>
        //</configuration>");

        //                var projectFolder = new DirectoryInfo(Path.Combine(solutionManager.SolutionDirectory, projectName));
        //                projectFolder.Create();
        //                var projectConfig = new FileInfo(Path.Combine(projectFolder.FullName, "project.json"));
        //                var msbuildProjectPath = new FileInfo(Path.Combine(projectFolder.FullName, $"{projectName}.csproj"));
        //                string extractPath = Path.Combine(solutionManager.SolutionDirectory);

        //                BuildIntegrationTestUtility.CreateConfigJson(projectConfig.FullName);

        //                var sources = new List<SourceRepository> { };
        //                var testLogger = new TestLogger();
        //                var settings = Settings.LoadSpecificSettings(solutionManager.SolutionDirectory, "NuGet.Config");
        //                var project = new ProjectJsonNuGetProject(projectConfig.FullName, msbuildProjectPath.FullName);

        //                solutionManager.NuGetProjects.Add(project);

        //                var restoreContext = new DependencyGraphCacheContext(testLogger, settings);
        //                var providersCache = new RestoreCommandProvidersCache();
        //                DependencyGraphSpec dgSpec = await DependencyGraphRestoreUtility.GetSolutionRestoreSpec(solutionManager, restoreContext);

        //                using var stream = new MemoryStream();
        //                _target.Settings = settings;
        //                _target.SolutionManager = solutionManager;

        //                // Act
        //                await _target.WriteToZipAsync(stream);

        //                // Assert
        //                using (var zip = new ZipArchive(stream))
        //                {
        //                    IEnumerable<string> zipFiles = zip.Entries.Select(e => e.FullName);
        //                    var expectedFiles = new[] { dgSpecFileName };

        //                    Assert.Equal(zipFiles.OrderBy(f => f), expectedFiles);

        //                    foreach (ZipArchiveEntry entry in zip.Entries)
        //                    {
        //                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
        //                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
        //                            entry.ExtractToFile(destinationPath);
        //                    }
        //                }

        //                DependencyGraphSpec vsFeedbackDgSpec = DependencyGraphSpec.Load(Path.Combine(extractPath, dgSpecFileName));
        //                Assert.Equal(dgSpec.Projects.Count, vsFeedbackDgSpec.Projects.Count);
        //                Assert.Equal(dgSpec.Projects[0].RestoreMetadata.Sources.Count, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources.Count);
        //                string hashedSource = CryptoHashUtility.GenerateUniqueToken(privateRepositoryPath);
        //                Assert.Equal(hashedSource, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources[0].Source);
        //                // dgSpec.Save replaces source name with source.
        //                Assert.Equal(hashedSource, vsFeedbackDgSpec.Projects[0].RestoreMetadata.Sources[0].Name);
        //            }
        //        }
    }
}
