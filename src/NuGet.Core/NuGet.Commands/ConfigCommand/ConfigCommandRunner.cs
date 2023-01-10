// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Commands
{
    public class ConfigCommandRunner : IConfigCommandRunner
    {
        public async Task<int> ExecuteCommandAsync(ConfigArgs configArgs)
        {
            var logger = configArgs.Logger;
        }
    }
}
