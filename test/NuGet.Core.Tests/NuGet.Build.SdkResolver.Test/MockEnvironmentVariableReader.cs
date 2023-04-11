// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Common;

namespace NuGet.Build.SdkResolver.Test
{
    public sealed class MockEnvironmentVariableReader : Dictionary<string, string>, IEnvironmentVariableReader
    {
        public MockEnvironmentVariableReader()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public string GetEnvironmentVariable(string variable) => TryGetValue(variable, out var value) ? value : null;
    }
}
