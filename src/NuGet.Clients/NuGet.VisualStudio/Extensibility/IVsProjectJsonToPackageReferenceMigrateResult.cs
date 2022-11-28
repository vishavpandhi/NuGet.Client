// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Contains the result of the migrate operation on a legacy project.json project
    /// </summary>
    public interface IVsProjectJsonToPackageReferenceMigrateResult
    {
        /// <summary>
        /// Returns the success value of the migration operation.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// If migrate operation was unsuccessful, stores the error message in the exception.
        /// </summary>
        string ErrorMessage { get; }
    }
}
