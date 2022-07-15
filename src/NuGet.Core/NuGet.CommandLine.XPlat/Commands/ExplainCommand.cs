// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Common;

namespace NuGet.CommandLine.XPlat.Commands
{
    internal class ExplainCommand
    {
        public static void Register(CommandLineApplication app, Func<ILogger> getLogger)
        {
            app.Command("explain", explain =>
            {
                explain.Description = Strings.Explain_Description;
                explain.HelpOption(XPlatUtility.HelpOption);

                CommandArgument path = explain.Argument(
                    "<PROJECT | SOLUTION>",
                    Strings.Explain_PathDescription,
                    multipleValues: false);

                CommandArgument packagePath = explain.Argument(
                    "<PACKAGE_NAME>",
                    Strings.ExplainCommandPackageDescription,
                    multipleValues: false);

                CommandOption framework = explain.Option(
                    "--framework",
                    Strings.ExplainFrameworkDescription,
                    CommandOptionType.MultipleValue);

                explain.OnExecute(() =>
                {
                    ValidatePackage(packagePath);

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
