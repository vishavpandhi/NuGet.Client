// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.UI.InfiniteScrollList.LoadNextPageAsync(NuGet.PackageManagement.UI.IPackageItemLoader,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Collections.Generic.IEnumerable{NuGet.PackageManagement.UI.PackageItemViewModel}}")]
[assembly: SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.UI.InfiniteScrollList.WaitForCompletionAsync(NuGet.PackageManagement.UI.IItemLoader{NuGet.PackageManagement.UI.PackageItemViewModel},System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.UI.PackageLoadContext.GetInstalledPackagesAsync~System.Threading.Tasks.Task{NuGet.PackageManagement.VisualStudio.PackageCollection}")]
