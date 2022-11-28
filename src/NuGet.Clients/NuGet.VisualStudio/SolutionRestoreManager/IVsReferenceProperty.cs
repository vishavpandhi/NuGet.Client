// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.SolutionRestoreManager
{
    /// <summary>
    /// Represents a property as a key-value pair
    /// </summary>
    public interface IVsReferenceProperty
    {
        /// <summary>
        /// Property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Property value.
        /// </summary>
        string Value { get; }
    }
}
