// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.ProjectModel;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Commands.Test
{
    public class DependencyResolutionExampleTests
    {

        /// <summary>
        /// Project -> A 1.0.0 -> B 1.0.0 -> C 1.0.0 -> D 2.0.0
        ///            A 1.0.0 -> C 2.0.0
        ///            A 1.0.0 -> D 1.0.0
        /// </summary>
        [Fact]
        public async Task RestoreCommand_PrunesFurthestNodes()
        {
            // Arrange
            using var pathContext = new SimpleTestPathContext();
            var logger = new TestLogger();
            var projectName = "TestProject";
            var projectPath = Path.Combine(pathContext.SolutionRoot, projectName);
            var sources = new List<PackageSource> { new PackageSource(pathContext.PackageSource) };

            var A = new SimpleTestPackageContext("A", "1.0.0");
            var B = new SimpleTestPackageContext("B", "1.0.0");
            var C100 = new SimpleTestPackageContext("C", "1.0.0");
            var C200 = new SimpleTestPackageContext("C", "2.0.0");
            var D100 = new SimpleTestPackageContext("D", "1.0.0");
            var D200 = new SimpleTestPackageContext("D", "2.0.0");

            B.Dependencies.Add(C100);
            A.Dependencies.Add(B);
            A.Dependencies.Add(C200);
            A.Dependencies.Add(D100);
            C100.Dependencies.Add(D200);

            await SimpleTestPackageUtility.CreateFolderFeedV3Async(
                pathContext.PackageSource,
                PackageSaveMode.Defaultv3,
                A,
                B,
                C100,
                C200,
                D100,
                D200
                );

            var spec = ProjectTestHelpers.GetPackageSpec(projectName, rootPath: projectPath, framework: "net472", dependencyName: "a");

            var request = new TestRestoreRequest(spec, sources, pathContext.UserPackagesFolder, logger)
            {
                LockFilePath = Path.Combine(projectPath, "project.assets.json"),
                ProjectStyle = ProjectStyle.PackageReference
            };

            var command = new RestoreCommand(request);

            // Act
            var result = await command.ExecuteAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.LogMessages.Should().HaveCount(0);
            result.LockFile.Libraries.Should().HaveCount(4);
            string[] files = Directory.GetFiles(pathContext.UserPackagesFolder, "*.nupkg", SearchOption.AllDirectories);
            files.Should().HaveCount(4);
        }

        [Fact]
        public async Task RestoreCommand_Custom()
        {
            // Arrange
            using (var pathContext = new SimpleTestPathContext())
            {
                var logger = new TestLogger();
                var projectName = "TestProject";
                var projectPath = Path.Combine(pathContext.SolutionRoot, projectName);
                var sources = new List<PackageSource> { new PackageSource(pathContext.PackageSource) };

                var project1Json = @"
                {
                  ""version"": ""1.0.0"",
                  ""frameworks"": {
                    ""net472"": {
                        ""dependencies"": {
                            ""A"": ""1.0.0"",
                            ""B"": ""1.0.0""
                        }
                    }
                  }
                }";

                var A = new SimpleTestPackageContext("A", "1.0.0");
                var B = new SimpleTestPackageContext("B", "1.0.0");
                var C100 = new SimpleTestPackageContext("C", "1.0.0");
                var C200 = new SimpleTestPackageContext("C", "2.0.0");
                //var D = new SimpleTestPackageContext("D", "1.0.0");
                var E = new SimpleTestPackageContext("E", "1.0.0");
                var F = new SimpleTestPackageContext("F", "1.0.0");

                B.Dependencies.Add(C200);
                A.Dependencies.Add(C100);
                A.Dependencies.Add(E);
                //D.Dependencies.Add(E);
                E.Dependencies.Add(F);
                F.Dependencies.Add(C200);


                await SimpleTestPackageUtility.CreateFolderFeedV3Async(
                    pathContext.PackageSource,
                    PackageSaveMode.Defaultv3,
                    A,
                    B,
                    C100,
                    C200,
                    //D,
                    E,
                    F
                    );
                // set up the project

                var spec = JsonPackageSpecReader.GetPackageSpec(project1Json, projectName, Path.Combine(projectPath, $"{projectName}.json")).WithTestRestoreMetadata();

                var request = new TestRestoreRequest(spec, sources, pathContext.UserPackagesFolder, logger)
                {
                    LockFilePath = Path.Combine(projectPath, "project.assets.json"),
                    ProjectStyle = ProjectStyle.PackageReference
                };

                var command = new RestoreCommand(request);

                // Act
                var result = await command.ExecuteAsync();

                // Assert
                result.Success.Should().BeTrue();
                result.LogMessages.Should().HaveCount(1);
                var message = result.LogMessages.Single();
                message.AsRestoreLogMessage().Code.Should().Be(NuGetLogCode.NU1605);
                message.AsRestoreLogMessage().LibraryId.Should().Be("packageA");
                result.LockFile.Libraries.Count.Should().Be(1);
            }
        }

        [Fact]
        public async Task RestoreCommand_Custom2()
        {
            // Arrange
            using (var pathContext = new SimpleTestPathContext())
            {
                var logger = new TestLogger();
                var projectName = "TestProject";
                var projectPath = Path.Combine(pathContext.SolutionRoot, projectName);
                var sources = new List<PackageSource> { new PackageSource(pathContext.PackageSource) };

                var project1Json = @"
                {
                  ""version"": ""1.0.0"",
                  ""frameworks"": {
                    ""net472"": {
                        ""dependencies"": {
                            ""C"": ""1.0.0"",
                            ""E"": ""1.0.0""
                        }
                    }
                  }
                }";

                var A = new SimpleTestPackageContext("A", "1.0.0");
                var B = new SimpleTestPackageContext("B", "1.0.0");
                var C100 = new SimpleTestPackageContext("C", "1.0.0");
                var C200 = new SimpleTestPackageContext("C", "2.0.0");
                var D = new SimpleTestPackageContext("D", "1.0.0");
                var E = new SimpleTestPackageContext("E", "1.0.0");
                var F = new SimpleTestPackageContext("F", "1.0.0");

                B.Dependencies.Add(C200);
                A.Dependencies.Add(D);
                D.Dependencies.Add(C100);
                D.Dependencies.Add(E);
                E.Dependencies.Add(F);
                F.Dependencies.Add(C200);


                await SimpleTestPackageUtility.CreateFolderFeedV3Async(
                    pathContext.PackageSource,
                    PackageSaveMode.Defaultv3,
                    A,
                    B,
                    C100,
                    C200,
                    D,
                    E,
                    F
                    );
                // set up the project

                var spec = JsonPackageSpecReader.GetPackageSpec(project1Json, projectName, Path.Combine(projectPath, $"{projectName}.json")).WithTestRestoreMetadata();

                var request = new TestRestoreRequest(spec, sources, pathContext.UserPackagesFolder, logger)
                {
                    LockFilePath = Path.Combine(projectPath, "project.assets.json"),
                    ProjectStyle = ProjectStyle.PackageReference
                };

                var command = new RestoreCommand(request);

                // Act
                var result = await command.ExecuteAsync();

                // Assert
                result.Success.Should().BeTrue();
                result.LogMessages.Should().HaveCount(1);
                var message = result.LogMessages.Single();
                message.AsRestoreLogMessage().Code.Should().Be(NuGetLogCode.NU1605);
                message.AsRestoreLogMessage().LibraryId.Should().Be("packageA");
                result.LockFile.Libraries.Count.Should().Be(1);
            }
        }
    }
}
