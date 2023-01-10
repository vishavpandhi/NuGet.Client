// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGet.Commands
{
    public class ConfigArgs
    {
        public enum ConfigAction
        {
            Paths
        }

        /// <summary>
        /// Action to be performed by the config command.
        /// </summary>
        public ConfigAction Action { get; set; }
        /// <summary>
        /// The working directory to be passed to the config command.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Logger to be used to display the logs during the execution of config command.
        /// </summary>
        public ILogger Logger { get; set; }
    }
}
