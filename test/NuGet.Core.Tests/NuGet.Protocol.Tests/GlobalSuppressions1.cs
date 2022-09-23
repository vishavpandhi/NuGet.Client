// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.MessageDispatcherTests.Close_DisposesAllActiveOutboundRequests~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.MessageDispatcherTests.OnMessageReceived_DoesNotThrowForCancelResponseAfterWaitForResponseIsCancelled~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.MessageDispatcherTests.OnMessageReceived_DoesNotThrowForResponseAfterWaitForResponseIsCancelled~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.MessageDispatcherTests.OnMessageReceived_WithDedicatedProcessingHandler_DoesNotThrowForResponseAfterWaitForResponseIsCancelled~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.OutboundRequestContextTests.HandleResponse_CompletesCompletionTask")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Plugins.Tests.OutboundRequestContextTests.HandleResponse_SecondResponseIsIgnored")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Tests.LocalPackageSearchResourceTests.LocalPackageSearch_SearchAsync_SlowLocalRepository_WithCancellationToken_ThrowsAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Protocol.Tests.RemotePackageArchiveDownloaderTests.Constructor_InitializesPropertiesAsync~System.Threading.Tasks.Task")]
