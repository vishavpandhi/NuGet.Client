// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.CommandLine.XPlat;
using NuGet.LibraryModel;
using NuGet.Test.Utility;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;
using Project = Microsoft.Build.Evaluation.Project;

namespace NuGet.CommandLine.Xplat.Tests
{
    public class MSBuildAPIUtilityTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        //private readonly ProjectCollection _projectCollection;

        //private readonly ProjectOptions _projectOptions;

        static MSBuildAPIUtilityTests()
        {
            try
            {
                MSBuildLocator.RegisterDefaults();
            }
            catch (Exception)
            {
            }
        }

        public MSBuildAPIUtilityTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            /*
            _projectCollection = new ProjectCollection(
                globalProperties: null,
                remoteLoggers: null,
                loggers: null,
                toolsetDefinitionLocations: ToolsetDefinitionLocations.Default,
                maxNodeCount: 1,
                onlyLogCriticalEvents: false,
                loadProjectsReadOnly: false);

            _projectOptions = new ProjectOptions
            {
                LoadSettings = ProjectLoadSettings.DoNotEvaluateElementsWithFalseCondition,
                ProjectCollection = _projectCollection
            };
            */
        }

        [Fact]
        public void ShowDebugInfoAboutDotnet()
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("dotnet", "--info")
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory
                });
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                _testOutputHelper.WriteLine(output);
                _testOutputHelper.WriteLine(error);

                process.WaitForExit();
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        }

        /*
        [Fact]
        public void GetDirectoryBuildPropsRootElementWhenItExists_Success()
        {
            var testDirectory = TestDirectory.Create();

            var propsFile =
@$"<Project>
    <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "Directory.Packages.props"), propsFile);

            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">    
	<PropertyGroup>                   
	<TargetFramework>net6.0</TargetFramework>
	</PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);

            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            var result = new MSBuildAPIUtility(logger: new TestLogger()).GetDirectoryBuildPropsRootElement(project);

            Assert.Equal(Path.Combine(testDirectory, "Directory.Packages.props"), result.FullPath);
        }

        [Fact]
        public void AddPackageReferenceIntoProjectFileWhenItemGroupDoesNotExist_Success()
        {
            // Set up
            var testDirectory = TestDirectory.Create();

            // Set up project file
            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">
<PropertyGroup>                   
<TargetFramework>net6.0</TargetFramework>
</PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);
            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            var msObject = new MSBuildAPIUtility(logger: new TestLogger());
            // Creating an item group in the project
            var itemGroup = msObject.CreateItemGroup(project, null);

            var libraryDependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                        name: "X",
                        versionRange: VersionRange.Parse("1.0.0"),
                        typeConstraint: LibraryDependencyTarget.Package)
            };

            // Act
            msObject.AddPackageReferenceIntoItemGroupCPM(project, itemGroup, libraryDependency);
            project.Save();

            // Assert
            Assert.Contains(@$"<PackageReference Include=""X"" />", File.ReadAllText(Path.Combine(testDirectory, "projectA.csproj")));
            Assert.DoesNotContain(@$"<Version = ""1.0.0"" />", File.ReadAllText(Path.Combine(testDirectory, "projectA.csproj")));
        }

        [Fact]
        public void AddPackageReferenceIntoProjectFileWhenItemGroupDoesExist_Success()
        {
            // Set up
            var testDirectory = TestDirectory.Create();
            
            // Set up project file
            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">
<PropertyGroup>                   
<TargetFramework>net6.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
<PackageReference Include=""Y"" />
</ItemGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);
            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            var msObject = new MSBuildAPIUtility(logger: new TestLogger());
            // Getting all the item groups in a given project
            var itemGroups = msObject.GetItemGroups(project);
            // Getting an existing item group that has package reference(s)
            var itemGroup = msObject.GetItemGroup(itemGroups, "PackageReference");

            var libraryDependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                        name: "X",
                        versionRange: VersionRange.Parse("1.0.0"),
                        typeConstraint: LibraryDependencyTarget.Package)
            };

            // Act
            msObject.AddPackageReferenceIntoItemGroupCPM(project, itemGroup, libraryDependency);
            project.Save();

            // Assert
            Assert.Contains(@$"<PackageReference Include=""X"" />", File.ReadAllText(Path.Combine(testDirectory, "projectA.csproj")));
            Assert.DoesNotContain(@$"<Version = ""1.0.0"" />", File.ReadAllText(Path.Combine(testDirectory, "projectA.csproj")));
        }

        [Fact]
        public void AddPackageVersionIntoPropsFileWhenItemGroupDoesNotExist_Success()
        {
            // Set up
            var testDirectory = TestDirectory.Create();

            // Set up Directory.Packages.props file
            var propsFile =
@$"<Project>
    <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "Directory.Packages.props"), propsFile);

            // Set up project file
            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">    
	<PropertyGroup>                   
	<TargetFramework>net6.0</TargetFramework>
	</PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);
            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            // Add item group to Directory.Packages.props
            var msObject = new MSBuildAPIUtility(logger: new TestLogger());
            var directoryBuildPropsRootElement = msObject.GetDirectoryBuildPropsRootElement(project);
            var propsItemGroup = directoryBuildPropsRootElement.AddItemGroup();

            var libraryDependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                        name: "X",
                        versionRange: VersionRange.Parse("1.0.0"),
                        typeConstraint: LibraryDependencyTarget.Package)
            };

            // Act
            msObject.AddPackageVersionIntoPropsItemGroup(propsItemGroup, libraryDependency);
            // Save the updated props file.
            directoryBuildPropsRootElement.Save();

            // Assert
            Assert.Contains(@$"<ItemGroup>
    <PackageVersion Include=""X"" Version=""1.0.0"" />
  </ItemGroup>", File.ReadAllText(Path.Combine(testDirectory, "Directory.Packages.props")));
        }

        [Fact]
        public void AddPackageVersionIntoPropsFileWhenItemGroupExists_Success()
        {
            // Set up
            var testDirectory = TestDirectory.Create();

            // Set up Directory.Packages.props file
            var propsFile =
@$"<Project>
    <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
    <ItemGroup>
    <PackageVersion Include=""X"" Version=""1.0.0"" />
    </ItemGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "Directory.Packages.props"), propsFile);

            // Set up project file
            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">    
	<PropertyGroup>                   
	<TargetFramework>net6.0</TargetFramework>
	</PropertyGroup>
    <ItemGroup>
    <PackageReference Include=""X"" />
    </ItemGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);
            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            // Get existing item group from Directory.Packages.props
            var msObject = new MSBuildAPIUtility(logger: new TestLogger());
            var directoryBuildPropsRootElement = msObject.GetDirectoryBuildPropsRootElement(project);
            var propsItemGroup = msObject.GetItemGroup(directoryBuildPropsRootElement.ItemGroups, "PackageVersion");

            var libraryDependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                        name: "Y",
                        versionRange: VersionRange.Parse("1.0.0"),
                        typeConstraint: LibraryDependencyTarget.Package)
            };

            // Act
            msObject.AddPackageVersionIntoPropsItemGroup(propsItemGroup, libraryDependency);
            // Save the updated props file
            directoryBuildPropsRootElement.Save();

            // Assert
            Assert.Contains(@$"<PackageVersion Include=""Y"" Version=""1.0.0"" />", File.ReadAllText(Path.Combine(testDirectory, "Directory.Packages.props")));
        }

        [Fact]
        public void UpdatePackageVersionInPropsFileWhenItExists_Success()
        {
            // Set up
            var testDirectory = TestDirectory.Create();

            // Set up Directory.Packages.props file
            var propsFile =
@$"<Project>
    <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>
    <ItemGroup>
    <PackageVersion Include=""X"" Version=""1.0.0"" />
    </ItemGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "Directory.Packages.props"), propsFile);

            // Set up project file
            string projectContent =
@$"<Project Sdk=""Microsoft.NET.Sdk"">    
	<PropertyGroup>                   
	<TargetFramework>net6.0</TargetFramework>
	</PropertyGroup>
    <ItemGroup>
    <PackageReference Include=""X"" />
    </ItemGroup>
</Project>";
            File.WriteAllText(Path.Combine(testDirectory, "projectA.csproj"), projectContent);
            var project = Project.FromFile(Path.Combine(testDirectory, "projectA.csproj"), _projectOptions);

            var msObject = new MSBuildAPIUtility(logger: new TestLogger());
            // Get package version if it already exists in the props file. Returns null if there is no matching package version.
            ProjectItem packageVersionInProps = project.Items.LastOrDefault(i => i.ItemType == "PackageVersion" && i.EvaluatedInclude.Equals("X"));

            var libraryDependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                        name: "X",
                        versionRange: VersionRange.Parse("2.0.0"),
                        typeConstraint: LibraryDependencyTarget.Package)
            };

            // Act
            msObject.UpdatePackageVersion(project, packageVersionInProps, "2.0.0");

            // Assert
            Assert.Equal(projectContent, File.ReadAllText(Path.Combine(testDirectory, "projectA.csproj")));
            Assert.Contains(@$"<PackageVersion Include=""X"" Version=""2.0.0"" />", File.ReadAllText(Path.Combine(testDirectory, "Directory.Packages.props")));
            Assert.DoesNotContain(@$"<PackageVersion Include=""X"" Version=""1.0.0"" />", File.ReadAllText(Path.Combine(testDirectory, "Directory.Packages.props")));
        }

        public void Dispose()
        {
            _projectCollection.Dispose();
        }
        */

        public void Dispose()
        {
        }
    }
}
