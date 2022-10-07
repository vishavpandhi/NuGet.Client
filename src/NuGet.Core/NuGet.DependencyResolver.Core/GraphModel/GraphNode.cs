// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NuGet.LibraryModel;

namespace NuGet.DependencyResolver
{
    public class GraphNode<TItem>
    {
        public GraphNode(LibraryRange key, int dependenciesCount = -1, bool shouldCreateParentNodes = false)
        {
            Key = key;
            Disposition = Disposition.Acceptable;

            //Create nonEmpty ParentNodes only when it's nessecery (ParentNodes is nonEmpty only for certain nodes when Central Package Management is enabled).
            if (shouldCreateParentNodes)
            {
                ParentNodes = new List<GraphNode<TItem>>();
            }
            else
            {
                ParentNodes = EmptyList;
            }

            //Create nonEmpty InnerNodes only when it's nessecery (InnerNodes is empty when it has no dependencies, including runtime dependencies).
            if (dependenciesCount == -1)  //If dependenciesCount is not known before creating this node, create one with default capacity.
            {
                InnerNodes = new List<GraphNode<TItem>>();
            }
            else if (dependenciesCount == 0)
            {
                InnerNodes = EmptyList;
            }
            else
            {
                InnerNodes = new List<GraphNode<TItem>>(dependenciesCount);
            }
        }

        //All empty ParentNodes and InnerNodes point to this immutable EmptyList.
        //This is to reduce the memory allocation for empty ParentNodes and InnerNodes when graph is large and there are huge number of nodes.
        internal static readonly IList<GraphNode<TItem>> EmptyList = new List<GraphNode<TItem>>(0).AsReadOnly();
        public LibraryRange Key { get; set; }
        public GraphItem<TItem> Item { get; set; }
        public GraphNode<TItem> OuterNode { get; set; }
        public IList<GraphNode<TItem>> InnerNodes { get; set; }
        public Disposition Disposition { get; set; }

        /// <summary>
        /// Used in case that a node is removed from its outernode and needs to keep reference of its parents.
        /// </summary>
        public IList<GraphNode<TItem>> ParentNodes { get; }

        internal bool AreAllParentsRejected()
        {
            var pCount = ParentNodes.Count;
            if (pCount == 0)
            {
                return false;
            }

            for (int i = 0; i < pCount; i++)
            {
                if (ParentNodes[i].Disposition != Disposition.Rejected)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return (Item?.Key ?? Key) + " " + Disposition;
        }
    }
}
