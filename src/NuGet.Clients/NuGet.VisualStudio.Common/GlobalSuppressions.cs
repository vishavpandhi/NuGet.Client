// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Build", "CA1802:Field 'EventName' is declared as 'readonly' but is initialized with a constant value. Mark this field as 'const' instead.", Justification = "<Pending>", Scope = "member", Target = "~F:NuGet.VisualStudio.Telemetry.PackageSourceTelemetry.EventName")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'bool PathValidator.IsValidLocalPath(string path)', validate parameter 'path' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.PathValidator.IsValidLocalPath(System.String)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'bool PathValidator.IsValidUncPath(string path)', validate parameter 'path' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.PathValidator.IsValidUncPath(System.String)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'AssemblyBinding.AssemblyBinding(IAssembly assembly)', validate parameter 'assembly' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.AssemblyBinding.#ctor(NuGet.VisualStudio.IAssembly)")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'bool AssemblyBinding.Equals(AssemblyBinding other)', validate parameter 'other' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.AssemblyBinding.Equals(NuGet.VisualStudio.AssemblyBinding)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1507:Use nameof in place of string literal 'dependentAssembly'", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.AssemblyBinding.Parse(System.Xml.Linq.XContainer)~NuGet.VisualStudio.AssemblyBinding")]
[assembly: SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'ErrorListTableEntry.TryGetValue(string, out object)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.Common.ErrorListTableEntry.TryGetValue(System.String,System.Object@)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'void MessageHelper.ShowError(ErrorListProvider errorListProvider, TaskErrorCategory errorCategory, TaskPriority priority, string errorText, IVsHierarchy hierarchyItem)', validate parameter 'errorListProvider' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.MessageHelper.ShowError(Microsoft.VisualStudio.Shell.ErrorListProvider,Microsoft.VisualStudio.Shell.TaskErrorCategory,Microsoft.VisualStudio.Shell.TaskPriority,System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy)")]
[assembly: SuppressMessage("Build", "CA1507:Use nameof in place of string literal 'path'", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.PathHelper.EscapePSPath(System.String)~System.String")]
[assembly: SuppressMessage("Build", "CA1507:Use nameof in place of string literal 'maxWidth'", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.PathHelper.SmartTruncate(System.String,System.Int32)~System.String")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'Task ProjectHelper.DoWorkInWriterLockAsync(Project project, IVsHierarchy hierarchy, Action<Project> action)', validate parameter 'action' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.ProjectHelper.DoWorkInWriterLockAsync(EnvDTE.Project,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy,System.Action{Microsoft.Build.Evaluation.Project})~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Build", "CA1031:Modify 'GetInstanceSafe' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.ServiceLocator.GetInstanceSafe``1~``0")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'Task<TService> ServiceProviderExtensions.GetServiceAsync<TService>(IAsyncServiceProvider site)', validate parameter 'site' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.ServiceProviderExtensions.GetServiceAsync``1(Microsoft.VisualStudio.Shell.IAsyncServiceProvider)~System.Threading.Tasks.Task{``0}")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'void SolutionEventsListener.Advise(IVsSolution vsSolution)', validate parameter 'vsSolution' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.SolutionEventsListener.Advise(Microsoft.VisualStudio.Shell.Interop.IVsSolution)")]
[assembly: SuppressMessage("Build", "CA1303:Method 'string PackageSourceTelemetry.GetActionName(TelemetryAction action)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"Unknown value of TelemetryAction\".", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.Telemetry.PackageSourceTelemetry.GetActionName(NuGet.VisualStudio.Telemetry.PackageSourceTelemetry.TelemetryAction)~System.String")]
[assembly: SuppressMessage("Build", "CA1305:The behavior of 'int.ToString()' could vary based on the current user's locale settings. Replace this call in 'PackageSourceTelemetry.ToStatusCodeTelemetry(Dictionary<int, int>)' with a call to 'int.ToString(IFormatProvider)'.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.Telemetry.PackageSourceTelemetry.ToStatusCodeTelemetry(System.Collections.Generic.Dictionary{System.Int32,System.Int32})~NuGet.Common.TelemetryEvent")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'bool VsHierarchyItem.Equals(VsHierarchyItem other)', validate parameter 'other' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.VsHierarchyItem.Equals(NuGet.VisualStudio.VsHierarchyItem)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1031:Modify 'TryGetProperty' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.VsHierarchyItem.TryGetProperty(System.Int32,System.Object@)~System.Boolean")]
[assembly: SuppressMessage("Build", "CA1825:Avoid unnecessary zero-length array allocations.  Use Array.Empty<VsHierarchyItem>() instead.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.VsHierarchyUtility.GetExpandedProjectHierarchyItems(EnvDTE.Project)~System.Collections.Generic.ICollection{NuGet.VisualStudio.VsHierarchyItem}")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'Project VsHierarchyUtility.GetProjectFromHierarchy(IVsHierarchy pHierarchy)', validate parameter 'pHierarchy' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.VsHierarchyUtility.GetProjectFromHierarchy(Microsoft.VisualStudio.Shell.Interop.IVsHierarchy)~EnvDTE.Project")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'string VsHierarchyUtility.GetProjectPath(IVsHierarchy project)', validate parameter 'project' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.VsHierarchyUtility.GetProjectPath(Microsoft.VisualStudio.Shell.Interop.IVsHierarchy)~System.String")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'void WindowFrameHelper.AddF1HelpKeyword(IVsWindowFrame windowFrame, string keywordValue)', validate parameter 'windowFrame' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.WindowFrameHelper.AddF1HelpKeyword(Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame,System.String)")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'void WindowFrameHelper.DisableWindowAutoReopen(IVsWindowFrame windowFrame)', validate parameter 'windowFrame' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.WindowFrameHelper.DisableWindowAutoReopen(Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)")]
[assembly: SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, params object[])' could vary based on the current user's locale settings. Replace this call in 'PackageManagementFormat.PackageFormatSelectorLabel.get' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~P:NuGet.VisualStudio.PackageManagementFormat.PackageFormatSelectorLabel")]
[assembly: SuppressMessage("Build", "CA2227:Change 'ProjectNames' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:NuGet.VisualStudio.PackageManagementFormat.ProjectNames")]
[assembly: SuppressMessage("Build", "CA1822:Member OperationId does not access instance data and can be marked as static (Shared in VisualBasic)", Justification = "<Pending>", Scope = "member", Target = "~P:NuGet.VisualStudio.SolutionRestoreRequest.OperationId")]
[assembly: SuppressMessage("Build", "CA1816:Change OutputConsoleLogger.Dispose() to call GC.SuppressFinalize(object). This will prevent derived types that introduce a finalizer from needing to re-implement 'IDisposable' to call it.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.Common.OutputConsoleLogger.Dispose")]
[assembly: SuppressMessage("Build", "CA1062:In externally visible method 'void OutputConsoleLogger.Log(ILogMessage message)', validate parameter 'message' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.VisualStudio.Common.OutputConsoleLogger.Log(NuGet.Common.ILogMessage)")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Need to unify event names to be same as ones produced from telemetry.", Scope = "member", Target = "~M:NuGet.VisualStudio.Telemetry.EtwLogActivity.#ctor(System.String)")]
