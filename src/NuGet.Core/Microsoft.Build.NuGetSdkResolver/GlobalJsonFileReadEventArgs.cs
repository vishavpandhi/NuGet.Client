// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Build.NuGetSdkResolver
{
    /// <summary>
    /// Represents the arguments for an event that fires when a global.json file is read.
    /// </summary>
    internal class GlobalJsonFileReadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the full path to the file that was read.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the full path to the project that requested the file to be read.
        /// </summary>
        public string ProjectFullPath { get; set; }

        /// <summary>
        /// Gets or sets the full path to the solution that requested the file to be read.
        /// </summary>
        public string SolutionFullPath { get; set; }
    }
}
