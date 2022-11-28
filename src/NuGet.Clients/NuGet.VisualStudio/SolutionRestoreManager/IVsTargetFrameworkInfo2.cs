// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace NuGet.SolutionRestoreManager
{
    /// <summary>
    /// Contains target framework metadata needed for restore operation. Compared to IVsTargetFrameworkInfo, this adds support for PackageDownload
    /// </summary>
    public interface IVsTargetFrameworkInfo2 : IVsTargetFrameworkInfo
    {
        /// <summary>
        /// Collection of package downloads.
        /// </summary>
        IVsReferenceItems PackageDownloads { get; }

        /// <summary>
        /// Collection of FrameworkReferences
        /// </summary>
        IVsReferenceItems FrameworkReferences { get; }
    }
}
