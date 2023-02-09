// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Test.Apex.VisualStudio;
using Microsoft.Test.Apex.VisualStudio.Solution;
using NuGet.Common;
using NuGet.ProjectModel;
using NuGet.Test.Utility;
using NuGet.Versioning;
using NuGet.VisualStudio;
using Thread = System.Threading.Thread;

namespace NuGet.Tests.Apex
{
    public class CommonUtility
    {
        public static async Task CreatePackageInSourceAsync(string packageSource, string packageName, string packageVersion)
        {
            var package = CreatePackage(packageName, packageVersion);
            await SimpleTestPackageUtility.CreatePackagesAsync(packageSource, package);
        }

        public static SimpleTestPackageContext CreatePackage(string packageName, string packageVersion, string requestAdditionalContent = null)
        {
            var package = new SimpleTestPackageContext(packageName, packageVersion);
            package.Files.Clear();
            package.AddFile("lib/net45/_._");
            package.AddFile("lib/netstandard1.0/_._");

            if (!string.IsNullOrWhiteSpace(requestAdditionalContent))
            {
                package.AddFile("lib/net45/" + requestAdditionalContent);
            }

            return package;
        }

        public static void AssertPackageInAssetsFile(VisualStudioHost visualStudio, ProjectTestExtension project, string packageName, string packageVersion, ILogger logger)
        {
            logger.LogInformation($"Checking assets file for {packageName}");

            var testService = visualStudio.Get<NuGetApexTestService>();
            testService.WaitForAutoRestore();

            var assetsFilePath = GetAssetsFilePath(project.FullPath);

            // Project has an assets file, let's look there to assert
            var inAssetsFile = IsPackageInstalledInAssetsFile(assetsFilePath, packageName, packageVersion, true);

            logger.LogInformation($"Exists: {inAssetsFile}");

            inAssetsFile.Should().BeTrue(AppendErrors($"{packageName}/{packageVersion} should be installed in {project.Name}", visualStudio));
        }

        public static void AssertPackageInPackagesConfig(VisualStudioHost visualStudio, ProjectTestExtension project, string packageName, string packageVersion, ILogger logger)
        {
            logger.LogInformation($"Checking project {packageName}");
            var testService = visualStudio.Get<NuGetApexTestService>();

            // Check using the IVs APIs
            var exists = testService.IsPackageInstalled(project.UniqueName, packageName, packageVersion);

            logger.LogInformation($"Exists: {exists}");

            exists.Should().BeTrue(AppendErrors($"{packageName}/{packageVersion} should be installed in {project.Name}", visualStudio));
        }

        private static bool IsPackageInstalledInAssetsFile(string assetsFilePath, string packageName, string packageVersion, bool expected)
        {
            return PackageExistsInLockFile(assetsFilePath, packageName, packageVersion, expected);
        }

        // return true if package exists, but retry logic is based on what value is expected so there is enough time for assets file to be updated.
        private static bool PackageExistsInLockFile(string pathToAssetsFile, string packageName, string packageVersion, bool expected)
        {
            var numAttempts = 0;
            LockFileLibrary lockFileLibrary = null;
            while (numAttempts++ < 3)
            {
                var version = NuGetVersion.Parse(packageVersion);
                var lockFile = GetAssetsFileWithRetry(pathToAssetsFile);
                lockFileLibrary = lockFile.Libraries
                    .SingleOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, packageName)
                                        && p.Version.Equals(version));
                if (expected && lockFileLibrary != null)
                {
                    return true;
                }
                if (!expected && lockFileLibrary == null)
                {
                    return false;
                }

                Thread.Sleep(2000);
            }

            return lockFileLibrary != null;
        }

        private static LockFile GetAssetsFileWithRetry(string path)
        {
            var timeout = TimeSpan.FromSeconds(20);
            var timer = Stopwatch.StartNew();
            string content = null;

            do
            {
                Thread.Sleep(100);
                if (File.Exists(path))
                {
                    try
                    {
                        content = File.ReadAllText(path);
                        var format = new LockFileFormat();
                        return format.Parse(content, path);
                    }
                    catch
                    {
                        // Ignore errors from conflicting writes.
                    }
                }
            }
            while (timer.Elapsed < timeout);

            // File cannot be read
            if (File.Exists(path))
            {
                throw new InvalidOperationException("Unable to read: " + path);
            }
            else
            {
                throw new FileNotFoundException("Not found: " + path);
            }
        }

        private static string GetAssetsFilePath(string projectPath)
        {
            var projectDirectory = Path.GetDirectoryName(projectPath);
            return Path.Combine(projectDirectory, "obj", "project.assets.json");
        }

        public static void UIInvoke(Action action)
        {
            var jtf = NuGetUIThreadHelper.JoinableTaskFactory;

            if (jtf != null)
            {
                jtf.Run(async () =>
                {
                    await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    action();
                });
            }
            else
            {
                // Run directly
                action();
            }
        }

        internal static ProjectTestExtension CreateAndInitProject(ProjectTemplate projectTemplate, SimpleTestPathContext pathContext, SolutionService solutionService, ILogger logger)
        {
            logger.LogInformation("Creating solution");
            solutionService.CreateEmptySolution("TestSolution", pathContext.SolutionRoot);

            logger.LogInformation("Adding project");
            var project = solutionService.AddProject(ProjectLanguage.CSharp, projectTemplate, ProjectTargetFramework.V46, "TestProject");

            logger.LogInformation("Saving solution");
            solutionService.Save();

            logger.LogInformation("Building solution");
            project.Build();

            return project;
        }

        public static string AppendErrors(string s, VisualStudioHost visualStudio)
        {
            var errors = visualStudio.GetErrorsInOutputWindows();

            if (errors.Any())
            {
                s += Environment.NewLine + string.Join("\n\t error: ", errors);
            }

            return s;
        }
    }
}
