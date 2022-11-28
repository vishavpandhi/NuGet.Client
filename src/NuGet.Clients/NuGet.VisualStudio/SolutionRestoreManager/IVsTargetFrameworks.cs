// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;

namespace NuGet.SolutionRestoreManager
{
    /// <summary>
    /// Represents a collection of target framework metadata items
    /// </summary>
    public interface IVsTargetFrameworks : IEnumerable
    {
        /// <summary>
        /// Total count of references in container.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Retrieves a reference by name or index.
        /// </summary>
        /// <param name="index">Reference name or index.</param>
        /// <returns>Reference item matching index.</returns>
        IVsTargetFrameworkInfo Item(object index);
    }
}
