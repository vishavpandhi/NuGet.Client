// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.VisualStudio
{
    /// <summary>
    /// NuGet path information specific to the current context (e.g. project context) or solution context
    /// Represents captured snapshot associated with current project/solution settings.
    /// Should be discarded immediately after all queries are done.
    /// </summary>
    public interface IVsPathContext2 : IVsPathContext
    {
        /// <summary>
        /// Solution packages folder directory. This will always be set irrespective if folder actually exists or not.
        /// The path returned is an absolute path.
        /// </summary>
        string SolutionPackageFolder { get; }
    }
}
