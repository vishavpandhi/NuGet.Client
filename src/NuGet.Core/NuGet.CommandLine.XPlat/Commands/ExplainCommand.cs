// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Common;

namespace NuGet.CommandLine.XPlat
{
    internal class ExplainCommand
    {
        public static void Register(CommandLineApplication app, Func<ILogger> getLogger, Func<IExplainPackageCommandRunner> getCommandRunner)
        {
            app.Command("explain", explain =>
            {
                explain.Description = Strings.Explain_Description;
                explain.HelpOption(XPlatUtility.HelpOption);

                CommandArgument path = explain.Argument(
                    "<PROJECT | SOLUTION>",
                    Strings.Explain_PathDescription,
                    multipleValues: false);

                CommandArgument package = explain.Argument(
                    "<PACKAGE_NAME>",
                    Strings.ExplainCommandPackageDescription,
                    multipleValues: false);

                CommandOption frameworks = explain.Option(
                    "--framework",
                    Strings.ExplainFrameworkDescription,
                    CommandOptionType.MultipleValue);

                explain.OnExecute(async () =>
                {
                    ValidatePackage(package);

                    var logger = getLogger();
                    var explainPackageArgs = new ExplainPackageArgs(
                        path.Value,
                        package.Value,
                        frameworks.Values,
                        logger);

                    var explainPackageCommandRunner = getCommandRunner();
                    await explainPackageCommandRunner.ExecuteCommandAsync(explainPackageArgs);
                    return 0;
                });
            });
        }

        private static void ValidatePackage(CommandArgument argument)
        {
            if (string.IsNullOrEmpty(argument.Value))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Error_PkgMissingArgument,
                    "explain",
                    argument.Name));
            }
        }
    }
}
