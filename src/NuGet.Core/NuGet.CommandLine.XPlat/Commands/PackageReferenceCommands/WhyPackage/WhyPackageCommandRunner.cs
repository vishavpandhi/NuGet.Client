// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using NuGet.ProjectModel;

namespace NuGet.CommandLine.XPlat
{
    internal class WhyPackageCommandRunner : IWhyPackageCommandRunner
    {
        private const string ProjectAssetsFile = "ProjectAssetsFile";
        private const string ProjectName = "MSBuildProjectName";

        public Task ExecuteCommandAsync(WhyPackageArgs whyPackageArgs)
        {
            var projectsPaths = Path.GetExtension(whyPackageArgs.Path).Equals(".sln") ?
                           MSBuildAPIUtility.GetProjectsFromSolution(whyPackageArgs.Path).Where(f => File.Exists(f)) :
                           new List<string>(new string[] { whyPackageArgs.Path });

            // the package you want to print the dependency paths for
            var package = whyPackageArgs.Package;

            var msBuild = new MSBuildAPIUtility(whyPackageArgs.Logger);

            foreach (var projectPath in projectsPaths)
            {
                //Open project to evaluate properties for the assets
                //file and the name of the project
                var project = MSBuildAPIUtility.GetProject(projectPath);

                if (!MSBuildAPIUtility.IsPackageReferenceProject(project))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        Strings.Error_NotPRProject,
                        projectPath));
                    Console.WriteLine();
                    continue;
                }

                var projectName = project.GetPropertyValue(ProjectName);

                var assetsPath = project.GetPropertyValue(ProjectAssetsFile);

                // If the file was not found, print an error message and continue to next project
                if (!File.Exists(assetsPath))
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture,
                        Strings.Error_AssetsFileNotFound,
                        projectPath));
                    Console.WriteLine();
                }
                else
                {
                    var lockFileFormat = new LockFileFormat();
                    var assetsFile = lockFileFormat.Read(assetsPath);

                    // Assets file validation
                    if (assetsFile.PackageSpec != null &&
                        assetsFile.Targets != null &&
                        assetsFile.Targets.Count != 0)
                    {

                        // Get all the packages that are referenced in a project
                        // TODO: for now, just passing in true for the last two args (hardcoded) so I need to change that
                        var packages = msBuild.GetResolvedVersions(project.FullPath, whyPackageArgs.Frameworks, assetsFile, true, true);

                        // If packages equals null, it means something wrong happened
                        // with reading the packages and it was handled and message printed
                        // in MSBuildAPIUtility function, but we need to move to the next project
                        if (packages != null)
                        {
                            // No packages means that no package references at all were found in the current framework
                            if (!packages.Any())
                            {
                                Console.WriteLine(string.Format(Strings.WhyPkg_NoPackagesFoundForFrameworks, projectName));
                            }
                            else
                            {
                                RunWhyCommand(packages, assetsFile.Targets, package);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(string.Format(Strings.ListPkg_ErrorReadingAssetsFile, assetsPath));
                    }

                    // Unload project
                    ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                }
            }

            return Task.CompletedTask;
        }

        public void RunWhyCommand(IEnumerable<FrameworkPackages> packages, IList<LockFileTarget> targetFrameworks, string package)
        {
            foreach (var frameworkPackages in packages)
            {
                // Get all the top level packages in the framework
                var frameworkTopLevelPackages = frameworkPackages.TopLevelPackages;
                // Get all the libraries in the framework
                var libraries = targetFrameworks.FirstOrDefault(i => i.Name == frameworkPackages.Framework).Libraries;

                FindPaths(frameworkPackages.Framework, frameworkTopLevelPackages, libraries, package);
            }
        }

        public void FindPaths(string frameworkName, IEnumerable<InstalledPackageReference> topLevelPackages, IList<LockFileTargetLibrary> libraries, string destination)
        {
            PrintFrameworkHeader(frameworkName);

            List<List<string>> listOfPaths = new List<List<string>>();
            HashSet<string> visited = new HashSet<string>();
            foreach (var package in topLevelPackages)
            {
                List<string> path = new List<string>
                {
                    // add the top level package to the path first
                    package.Name
                };
                var dependencyPathsInFramework = DfsTraversal(package.Name, libraries, visited, path, listOfPaths, destination);
                PrintDependencyPathsInFramework(dependencyPathsInFramework);
            }
        }

        public List<List<string>> DfsTraversal(string rootPackage, IList<LockFileTargetLibrary> libraries, HashSet<string> visited, List<string> path, List<List<string>> listOfPaths, string destination)
        {
            if (rootPackage == destination)
            {
                // copy what is stored in list variable over to list that you allocate memory for
                List<string> pathToAdd = new List<string>();
                foreach (var p in path)
                {
                    pathToAdd.Add(p);
                }
                listOfPaths.Add(pathToAdd);
                return listOfPaths;
            }

            // Find the library that matches the root package's ID and get all its dependencies
            LockFileTargetLibrary library = libraries.FirstOrDefault(i => i.Name == rootPackage);
            var listDependencies = library.Dependencies;

            if (listDependencies.Count != 0)
            {
                foreach (var dependency in listDependencies)
                {
                    if (!visited.Contains(dependency.Id))
                    {
                        visited.Add(dependency.Id);
                        path.Add(dependency.Id);

                        // recurse
                        DfsTraversal(dependency.Id, libraries, visited, path, listOfPaths, destination);

                        // backtrack
                        path.RemoveAt(path.Count - 1);
                        visited.Remove(dependency.Id);
                    }
                }
            }

            return listOfPaths;
        }

        public void PrintDependencyPathsInFramework(List<List<string>> listOfPaths)
        {
            if (listOfPaths.Count == 0)
            {
                Console.Write("No dependency paths found in this framework.");
            }

            foreach (var path in listOfPaths)
            {
                Console.Write("\t");
                int iteration = 0;
                foreach (var package in path)
                {
                    Console.Write(package);
                    // don't print arrows after the last package in the path
                    if (iteration < path.Count - 1)
                    {
                        Console.Write(" -> ");
                    }
                    iteration++;
                }
                Console.Write("\n");
            }
        }

        void PrintFrameworkHeader(string frameworkName)
        {
            Console.Write(frameworkName);
            Console.Write(":\n");
        }
    }
}
