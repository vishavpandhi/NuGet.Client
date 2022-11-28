// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Contains batch events which are raised when packages are installed or uninstalled from projects with packages.config
    /// and the current solution.
    /// </summary>
    public interface IVsPackageInstallerProjectEvents
    {
        /// <summary>
        /// Raised before any IVsPackageInstallerEvents events are raised for a project.
        /// </summary>
        event VsPackageProjectEventHandler BatchStart;

        /// <summary>
        /// Raised after all IVsPackageInstallerEvents events are raised for a project.
        /// </summary>
        event VsPackageProjectEventHandler BatchEnd;

    }
}
