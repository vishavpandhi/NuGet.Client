// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Commands;
using NuGet.Common;

namespace NuGet.CommandLine.XPlat
{
    internal static class ConfigCommand
    {
        internal static void Register(CommandLineApplication app,
            Func<ILogger> getLogger,
            Action<LogLevel> setLogLevel)
        {
            app.Command("config", configCmd =>
            {
                // sub-commands
                configCmd.Command("paths", pathsCommand =>
                {
                    pathsCommand.Description = "description";

                    CommandArgument workingDirectory = pathsCommand.Argument(
                    "<working-directory>",
                    "description",
                    multipleValues: false);

                    pathsCommand.HelpOption(XPlatUtility.HelpOption);

                    CommandOption verbosity = pathsCommand.VerbosityOption();

                    pathsCommand.OnExecute(async () =>
                    {
                        return await ExecuteCommand(ConfigSubCommands.Paths, workingDirectory, getLogger, setLogLevel);
                    });
                });

                // Main command
                configCmd.Description = "configCmd description";
                configCmd.HelpOption(XPlatUtility.HelpOption);

                // If no command specified then default to Help option.
                CommandOption mainVerbosity = configCmd.VerbosityOption();
            });
        }

        private static CommandOption VerbosityOption(this CommandLineApplication command)
        {
            return command.Option(
                "-v|--verbosity",
                Strings.Verbosity_Description,
                CommandOptionType.SingleValue);
        }

        private static async Task<int> ExecuteCommand(ConfigSubCommands subCommand,
            CommandArgument workingDirectory,
            Func<ILogger> getLogger)
        {
            ILogger logger = getLogger();

            var configArgs = new ConfigArgs()
            {
                Action = ConfigAction.Paths,
                WorkingDirectory = 
                Logger = logger
            };

            var runner = new ConfigCommandRunner();
            Task<int> configTask = runner.ExecuteCommandAsync(configArgs);
            return await configTask;
        }

        private static void ValidateWorkingDirectory(CommandArgument argument)
        {
            if (argument.Values.Count == 0 ||
                argument.Values.Any<string>(workingDirectory => string.IsNullOrEmpty(workingDirectory)))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Error_PkgMissingArgument,
                    CommandName,
                    argument.Name));
            }
        }
    }
}
