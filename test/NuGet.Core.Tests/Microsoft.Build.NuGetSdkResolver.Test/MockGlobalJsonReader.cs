// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Microsoft.Build.NuGetSdkResolver.Test
{
    /// <summary>
    /// Represents a mock implementation of <see cref="IGlobalJsonReader" />.
    /// </summary>
    internal class MockGlobalJsonReader : IGlobalJsonReader
    {
        private readonly string _globalJsonFullPath;

        private readonly Dictionary<string, string> _sdkVersions;

        public MockGlobalJsonReader(Dictionary<string, string> sdkVersions, string globalJsonFullPath = null)
        {
            _sdkVersions = sdkVersions;
            _globalJsonFullPath = globalJsonFullPath;
        }

        public event EventHandler<GlobalJsonFileReadEventArgs> FileReadStart;

        public event EventHandler<GlobalJsonFileReadEventArgs> FileReadStop;

        public Dictionary<string, string> GetMSBuildSdkVersions(SdkResolverContext context, out string globalJsonPath, string fileName = "global.json")
        {
            GlobalJsonFileReadEventArgs eventArgs = new GlobalJsonFileReadEventArgs
            {
                FullPath = _globalJsonFullPath,
                ProjectFullPath = context.ProjectFilePath,
                SolutionFullPath = context.SolutionFilePath
            };

            FileReadStart?.Invoke(this, eventArgs);

            try
            {
                globalJsonPath = _globalJsonFullPath;

                return _sdkVersions;
            }
            finally
            {
                FileReadStop?.Invoke(this, eventArgs);
            }
        }
    }
}
