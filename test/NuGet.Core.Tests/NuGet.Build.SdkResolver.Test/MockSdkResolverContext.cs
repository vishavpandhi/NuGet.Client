// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Build.NuGetSdkResolver.Test;

using SdkResolverContextBase = Microsoft.Build.Framework.SdkResolverContext;

namespace NuGet.Build.SdkResolver.Test
{
    /// <summary>
    /// A mock implementation of <see cref="SdkResolverContext" /> that uses a <see cref="MockSdkLogger" />.
    /// </summary>
    public sealed class MockSdkResolverContext : SdkResolverContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockSdkResolverContext" /> class.
        /// </summary>
        /// <param name="projectPath">The path to the project.</param>
        public MockSdkResolverContext(string projectPath = null, string solutionPath = null)
        {
            Logger = MockSdkLogger;

            ProjectFilePath = projectPath;
            SolutionFilePath = solutionPath;
        }

        /// <summary>
        /// Gets the <see cref="MockSdkLogger" /> being used by the context.
        /// </summary>
        public MockSdkLogger MockSdkLogger { get; } = new MockSdkLogger();
    }
}
