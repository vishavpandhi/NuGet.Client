// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FluentAssertions;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Xunit;

namespace NuGet.DependencyResolver.Core.Tests
{
    public class GraphNodeTests
    {
        [Theory]
        [InlineData(null, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void GraphNode_WithDifferentShouldCreateParentNodes_CreateParentNodesCorrectly(bool? shouldCreateParentNodes, bool IsParentNodesEmptyList)
        {
            LibraryRange libraryRangeA = GetLibraryRange("A", "1.0.0", "1.0.0");
            LibraryRange libraryRangeB = GetLibraryRange("B", "1.0.0", "1.0.0");

            GraphNode<RemoteResolveResult> nodeA, nodeB;

            if (!shouldCreateParentNodes.HasValue)
            {
                nodeA = new GraphNode<RemoteResolveResult>(libraryRangeA);
                nodeB = new GraphNode<RemoteResolveResult>(libraryRangeB);
            }
            else
            {
                nodeA = new GraphNode<RemoteResolveResult>(libraryRangeA, 0, shouldCreateParentNodes.Value);
                nodeB = new GraphNode<RemoteResolveResult>(libraryRangeB, 0, shouldCreateParentNodes.Value);
            }

            bool sameParentNodes = (nodeA.ParentNodes == nodeB.ParentNodes);
            if (IsParentNodesEmptyList)
            {
                //The ParentNodes should be pointing to the static EmptyList.
                sameParentNodes.Should().BeTrue(because: "nodeA.ParentNodes and nodeB.ParentNodes should both point to the static EmptyList");

                //EmptyList is immutable.
                var exception = Assert.ThrowsAny<NotSupportedException>(
                () => nodeA.ParentNodes.Add(nodeB));
                Assert.Equal("Collection is read-only.", exception.Message);
            }
            else
            {
                sameParentNodes.Should().BeFalse(because: "nodeA.ParentNodes and nodeB.ParentNodes should not point to the static EmptyList");
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(4, false)]
        public void GraphNode_WithDifferentDependenciesCount_CreateInnerNodesCorrectly(int? dependenciesCount, bool IsInnerNodesEmptyList)
        {
            LibraryRange libraryRangeA = GetLibraryRange("A", "1.0.0", "1.0.0");
            LibraryRange libraryRangeB = GetLibraryRange("B", "1.0.0", "1.0.0");

            GraphNode<RemoteResolveResult> nodeA, nodeB;

            if (!dependenciesCount.HasValue)
            {
                nodeA = new GraphNode<RemoteResolveResult>(libraryRangeA);
                nodeB = new GraphNode<RemoteResolveResult>(libraryRangeB);
            }
            else
            {
                nodeA = new GraphNode<RemoteResolveResult>(libraryRangeA, dependenciesCount.Value);
                nodeB = new GraphNode<RemoteResolveResult>(libraryRangeB, dependenciesCount.Value);
            }

            bool sameInnerNodes = (nodeA.InnerNodes == nodeB.InnerNodes);
            if (IsInnerNodesEmptyList)
            {
                //The InnerNodes should be pointing to the static EmptyList.
                sameInnerNodes.Should().BeTrue(because: "nodeA.InnerNodes and nodeB.InnerNodes should be the same as they both point to the static EmptyList");

                //EmptyList is immutable.
                var exception = Assert.ThrowsAny<NotSupportedException>(
                () => nodeA.InnerNodes.Add(nodeB));
                Assert.Equal("Collection is read-only.", exception.Message);
            }
            else
            {
                sameInnerNodes.Should().BeFalse(because: "nodeA.InnerNodes and nodeB.InnerNodes should not be the same");
            }
        }

        public LibraryRange GetLibraryRange(string id, string range, string version)
        {
            return new LibraryRange(id.ToUpperInvariant(), VersionRange.Parse(range), LibraryDependencyTarget.Package);
        }
    }
}
