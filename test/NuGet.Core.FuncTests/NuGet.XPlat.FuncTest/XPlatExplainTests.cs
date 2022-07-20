// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.CommandLine.XPlat;
using NuGet.Common;
using Xunit;

namespace NuGet.XPlat.FuncTest
{
    [Collection("NuGet XPlat Test Collection")]
    public class XPlatWhyTests
    {
        [Theory]
        [InlineData("-all")]
        [InlineData("-Signatures")]
        [InlineData("-certificate-fingerprint")]
        [InlineData("--h")]
        public void WhyCommandArgsParsing_UnrcognizedOption_Throws(string unrecognizedOption)
        {
            VerifyCommandArgs(
                (testApp, getLogLevel) =>
                {
                    //Arrange
                    string[] args = new string[] { "why", unrecognizedOption };

                    // Act & Assert
                    Assert.Throws<CommandParsingException>(() => testApp.Execute(args));
                });
        }

        [Fact]
        public void WhyCommandArgsParsing_MissingPackageName_Throws()
        {
            VerifyCommandArgs(
                (testApp, getLogLevel) =>
                {
                    // Arrange
                    var argList = new List<string>() { "why" };

                    // Act
                    var ex = Assert.Throws<ArgumentException>(() => testApp.Execute(argList.ToArray()));

                    // Assert
                    Assert.Equal("Unable to why package. Argument '<PACKAGE_NAME>' not provided.", ex.Message);
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
            WhyCommand.Register(testApp,
                () => logger,
                () => new WhyPackageCommandRunner());

            // Act & Assert
            verify(testApp, () => logLevel);
        }
    }
}
