// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.PackageResolver.Resolve(NuGet.Resolver.PackageResolverContext,System.Threading.CancellationToken)~System.Collections.Generic.IEnumerable{NuGet.Packaging.Core.PackageIdentity}")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.PackageResolver.Resolve(NuGet.Resolver.PackageResolverContext,System.Threading.CancellationToken)~System.Collections.Generic.IEnumerable{NuGet.Packaging.Core.PackageIdentity}")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.ResolverComparer.Compare(NuGet.Resolver.ResolverPackage,NuGet.Resolver.ResolverPackage)~System.Int32")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.ResolverInputSort.TreeFlatten(System.Collections.Generic.List{System.Collections.Generic.List{NuGet.Resolver.ResolverPackage}},NuGet.Resolver.PackageResolverContext)~System.Collections.Generic.List{System.Collections.Generic.List{NuGet.Resolver.ResolverPackage}}")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.ResolverPackage.#ctor(NuGet.Packaging.Core.PackageDependencyInfo,System.Boolean,System.Boolean)")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.ResolverUtility.FindFirstCircularDependency(System.Collections.Generic.IEnumerable{NuGet.Resolver.ResolverPackage})~System.Collections.Generic.IEnumerable{NuGet.Resolver.ResolverPackage}")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:NuGet.Resolver.ResolverUtility.IsDependencySatisfied(NuGet.Packaging.Core.PackageDependency,NuGet.Packaging.Core.PackageIdentity)~System.Boolean")]
