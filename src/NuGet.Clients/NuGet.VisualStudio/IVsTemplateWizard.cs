// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#if IS_DESKTOP
using Microsoft.VisualStudio.TemplateWizard;

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Defines the logic for a template wizard extension.
    /// </summary>
    public interface IVsTemplateWizard : IWizard
    {
    }
}
#endif
