// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Contains information about project with packages.config where we're executing nuget actions.
    /// </summary>
    public interface IVsPackageProjectMetadata
    {
        /// <summary>
        /// Unique batch id for batch start/end events of the project.
        /// </summary>
        string BatchId { get; }

        /// <summary>
        /// Name of the project.
        /// </summary>
        string ProjectName { get; }
    }
}
