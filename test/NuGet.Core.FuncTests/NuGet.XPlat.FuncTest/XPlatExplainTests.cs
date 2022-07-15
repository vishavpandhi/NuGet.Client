// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.CommandLine.XPlat.Commands;
using NuGet.Common;
using Xunit;

namespace NuGet.XPlat.FuncTest
{
    [Collection("NuGet XPlat Test Collection")]
    public class XPlatExplainTests
    {
        [Theory]
        [InlineData("-all")]
        [InlineData("-Signatures")]
        [InlineData("-certificate-fingerprint")]
        [InlineData("--h")]
        public void ExplainCommandArgsParsing_UnrcognizedOption_Throws(string unrecognizedOption)
        {
            VerifyCommandArgs(
                (testApp, getLogLevel) =>
                {
                    //Arrange
                    string[] args = new string[] { "explain", unrecognizedOption };

                    // Act & Assert
                    Assert.Throws<CommandParsingException>(() => testApp.Execute(args));
                });
        }

        [Fact]
        public void ExplainCommandArgsParsing_MissingPackageName_Throws()
        {
            VerifyCommandArgs(
                (testApp, getLogLevel) =>
                {
                    // Arrange
                    var argList = new List<string>() { "explain" };

                    // Act
                    var ex = Assert.Throws<ArgumentException>(() => testApp.Execute(argList.ToArray()));

                    // Assert
                    Assert.Equal("Unable to explain package. Argument '<PACKAGE_NAME>' not provided.", ex.Message);
                });
        }

        private void VerifyCommandArgs(Action<CommandLineApplication, Func<LogLevel>> verify)
        {
            // Arrange
            var logLevel = LogLevel.Information;
            var logger = new TestCommandOutputLogger();
            var testApp = new CommandLineApplication
            {
                Name = "dotnet nuget_test"
            };
            ExplainCommand.Register(testApp,
                () => logger);

            // Act & Assert
            verify(testApp, () => logLevel);
        }
    }
}
