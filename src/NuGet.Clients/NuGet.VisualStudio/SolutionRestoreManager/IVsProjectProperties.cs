// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;

namespace NuGet.SolutionRestoreManager
{
    /// <summary>
    /// Represents a collection of project properties.
    /// </summary>
    public interface IVsProjectProperties : IEnumerable
    {
        /// <summary>
        /// Total count of properties in container.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Retrieves a property by name or index.
        /// </summary>
        /// <param name="index">Property name or index.</param>
        /// <returns>Property matching index.</returns>
        IVsProjectProperty Item(object index);
    }
}
