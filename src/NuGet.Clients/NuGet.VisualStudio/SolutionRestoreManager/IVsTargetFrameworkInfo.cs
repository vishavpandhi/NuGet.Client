// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.SolutionRestoreManager
{
    /// <summary>
    /// Contains target framework metadata needed for restore operation.
    /// </summary>
    public interface IVsTargetFrameworkInfo
    {
        /// <summary>
        /// Target framework name in full format.
        /// </summary>
        string TargetFrameworkMoniker { get; }

        /// <summary>
        /// Collection of project references.
        /// </summary>
        IVsReferenceItems ProjectReferences { get; }

        /// <summary>
        /// Collection of package references.
        /// </summary>
        IVsReferenceItems PackageReferences { get; }

        /// <summary>
        /// Collection of project level properties evaluated per each Target Framework,
        /// e.g. PackageTargetFallback.
        /// </summary>
        IVsProjectProperties Properties { get; }
    }
}
