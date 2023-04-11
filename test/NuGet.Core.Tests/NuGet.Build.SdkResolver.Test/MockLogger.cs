// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGet.Build.SdkResolver.Test
{
    internal class MockLogger : ILogger
    {
        private readonly ConcurrentQueue<string> _errors = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _warnings = new ConcurrentQueue<string>();

        public IReadOnlyCollection<string> Errors => _errors;

        public IReadOnlyCollection<string> Messages => _messages;

        public IReadOnlyCollection<string> Warnings => _warnings;

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Verbose:
                case LogLevel.Information:
                case LogLevel.Minimal:
                    _messages.Enqueue(data);
                    break;

                case LogLevel.Error:
                    _errors.Enqueue(data);
                    break;

                case LogLevel.Warning:
                    _warnings.Enqueue(data);
                    break;
            }
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        public Task LogAsync(LogLevel level, string data)
        {
            Log(level, data);

            return Task.CompletedTask;
        }

        public Task LogAsync(ILogMessage message)
        {
            Log(message);

            return Task.CompletedTask;
        }

        public void LogDebug(string data) => Log(LogLevel.Debug, data);

        public void LogError(string data) => Log(LogLevel.Error, data);

        public void LogInformation(string data) => Log(LogLevel.Information, data);

        public void LogInformationSummary(string data)
        {
        }

        public void LogMinimal(string data) => Log(LogLevel.Minimal, data);

        public void LogVerbose(string data) => Log(LogLevel.Verbose, data);

        public void LogWarning(string data) => Log(LogLevel.Warning, data);
    }
}
