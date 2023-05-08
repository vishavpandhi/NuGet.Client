// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace NuGet.Test.Utility
{
    public class CommandRunner
    {
        // Item1 of the returned tuple is the exit code. Item2 is the standard output, and Item3
        // is the error output.
        public static CommandRunnerResult Run(
            string filename,
            string workingDirectory,
            string arguments,
            bool waitForExit,
            int timeOutInMilliseconds = 60000,
            Action<StreamWriter> inputAction = null,
            IDictionary<string, string> environmentVariables = null)
        {
            var processStartInfo = new ProcessStartInfo(Path.GetFullPath(filename), arguments)
            {
                WorkingDirectory = Path.GetFullPath(workingDirectory),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = waitForExit,
                RedirectStandardOutput = waitForExit,
                RedirectStandardInput = inputAction != null
            };

#if !IS_CORECLR
            processStartInfo.EnvironmentVariables["NuGetTestModeEnabled"] = "True";
#else
            processStartInfo.Environment["NuGetTestModeEnabled"] = "True";
#endif

            if (environmentVariables != null)
            {
                foreach (var pair in environmentVariables)
                {
#if !IS_CORECLR
                    processStartInfo.EnvironmentVariables[pair.Key] = pair.Value;
#else
                    processStartInfo.Environment[pair.Key] = pair.Value;
#endif
                }
            }

            using (var process = new Process()
            {
                EnableRaisingEvents = true,
            })
            {
                if (!waitForExit)
                {
                    process.Start();

                    return new CommandRunnerResult(process, 0, string.Empty, string.Empty);
                }

                var output = new StringBuilder();
                var error = new StringBuilder();

                using ManualResetEvent resetEvent = new ManualResetEvent(false);

                process.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data != null)
                    {
                        error.AppendLine(args.Data);
                    }
                };

                process.OutputDataReceived += (_, args) =>
                {
                    if (args.Data != null)
                    {
                        output.AppendLine(args.Data);
                    }
                };

                process.Exited += (sender, args) => { resetEvent.Set(); };

                process.StartInfo = processStartInfo;
                process.Start();

                inputAction?.Invoke(process.StandardInput);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool processExited = resetEvent.WaitOne(timeOutInMilliseconds);

                if (processExited)
                {
                    return new CommandRunnerResult(process, process.ExitCode, output.ToString(), error.ToString());
                }

                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                }

                throw new TimeoutException($"{processStartInfo.FileName} {processStartInfo.Arguments} timed out after {TimeSpan.FromMilliseconds(timeOutInMilliseconds).TotalSeconds:N0} seconds:{Environment.NewLine}Output:{output}{Environment.NewLine}Error:{error}");
            }
        }
    }
}
