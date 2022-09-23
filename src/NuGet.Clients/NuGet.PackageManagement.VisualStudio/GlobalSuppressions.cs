// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.MultiSourcePackageMetadataProvider.GetLocalPackageMetadataAsync(NuGet.Packaging.Core.PackageIdentity,System.Boolean,System.Threading.CancellationToken)~System.Threading.Tasks.Task{NuGet.Protocol.Core.Types.IPackageSearchMetadata}")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.ProjectRetargetingHandler.Microsoft#VisualStudio#Shell#Interop#IVsTrackBatchRetargetingEvents#OnBatchRetargetingEnd~System.Int32")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.ProjectRetargetingHandler.Microsoft#VisualStudio#Shell#Interop#IVsTrackProjectRetargetingEvents#OnRetargetingAfterChange(System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy,System.String,System.String)~System.Int32")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.RuntimeHelpers.QueueUnloadAndForget(System.AppDomain)")]
[assembly: SuppressMessage("Usage", "VSTHRD011:Use AsyncLazy<T>", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.PackageManagement.VisualStudio.VSSolutionManager.InitializeAsync~System.Threading.Tasks.Task")]
