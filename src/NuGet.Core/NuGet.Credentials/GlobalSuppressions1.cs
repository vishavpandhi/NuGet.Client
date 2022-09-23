// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.PluginCredentialProvider.GetAsync(System.Uri,System.Net.IWebProxy,NuGet.Configuration.CredentialRequestType,System.String,System.Boolean,System.Boolean,System.Threading.CancellationToken)~System.Threading.Tasks.Task{NuGet.Credentials.CredentialResponse}")]
[assembly: SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.PluginCredentialProvider.GetPluginResponse(NuGet.Credentials.PluginCredentialRequest,System.Threading.CancellationToken)~NuGet.Credentials.PluginCredentialResponse")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.PluginCredentialProvider.PassVerbosityFlag(NuGet.Credentials.PluginCredentialRequest)~System.Boolean")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.PluginException.Create(System.String,System.Exception)~NuGet.Credentials.PluginException")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.PluginException.CreateInvalidResponseExceptionMessage(System.String,NuGet.Credentials.PluginCredentialResponseExitCode,NuGet.Credentials.PluginCredentialResponse)~NuGet.Credentials.PluginException")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.SecurePluginCredentialProvider.GetAsync(System.Uri,System.Net.IWebProxy,NuGet.Configuration.CredentialRequestType,System.String,System.Boolean,System.Boolean,System.Threading.CancellationToken)~System.Threading.Tasks.Task{NuGet.Credentials.CredentialResponse}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Credentials.SecurePluginCredentialProvider.SetPluginLogLevelAsync(NuGet.Protocol.Plugins.PluginCreationResult,NuGet.Common.ILogger,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "<Pending>", Scope = "member", Target = "~P:NuGet.Credentials.PluginCredentialRequest.Uri")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:NuGet.Credentials.PluginCredentialResponse.AuthTypes")]
