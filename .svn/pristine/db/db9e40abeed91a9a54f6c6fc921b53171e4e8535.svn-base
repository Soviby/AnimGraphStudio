using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using GameCore.Animation;

namespace GameEditor.Animation
{
    public class AnimGraphLayout
    {
        public class Edge
        {
            public Edge(Vertex src, Vertex dest)
            {
                source = src;
                destination = dest;
            }

            public Vertex source { get; private set; }
            public Vertex destination { get; private set; }
        }

        public class Vertex
        {
            public Vector2 position { get; set; }

            public AnimNode node { get; private set; }

            public Vertex(AnimNode node)
            {
                this.node = node;
            }
        }

        private static readonly float s_DistanceBetweenNodes = 1.0f;
        private static readonly float s_WireLengthFactorForLargeSpanningTrees = 3.0f;
        private static readonly float s_MaxChildrenThreshold = 6.0f;

        readonly Dictionary<AnimNode, Vertex> nodeToVertex = new Dictionary<AnimNode, Vertex>();

        public IEnumerable<Vertex> GetVertices()
        {
            return nodeToVertex.Values;
        }

        public IEnumerable<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            foreach (var e in nodeToVertex)
            {
                var v = e.Value;
                var n = e.Key;
                var children = n.Children;
                if (children != null)
                {
                    foreach (var c in children)
                    {
                        edges.Add(new Edge(nodeToVertex[c], v));
                    }
                }
            }
            return edges;
        }

        public void Reset()
        {
            nodeToVertex.Clear();
        }

        public void CalcLayout(AnimGraph graph)
        {
            nodeToVertex.Clear();
            var root = graph.Root;
            GatherNodes(root);

            var horizontalPositions = ComputeHorizontalPositionForEachLevel();
            RecursiveLayout(root, 0, horizontalPositions);

            void GatherNodes(AnimNode node)
            {
                nodeToVertex.Add(node, new Vertex(node));
                var children = node.Children;
                if (children != null)
                {
                    foreach (var c in children)
                    {
                        GatherNodes(c);
                    }
                }
            }
        }

        private float[] ComputeHorizontalPositionForEachLevel()
        {
            var maxDepth = int.MinValue;
            var nodeDepths = new Dictionary<int, List<AnimNode>>();
            foreach (var node in nodeToVertex.Keys)
            {
                int d = node.Depth;
                if (!nodeDepths.TryGetValue(d, out var nodes))
                {
                    nodeDepths[d] = nodes = new List<AnimNode>();
                }
                nodes.Add(node);
                maxDepth = Mathf.Max(d, maxDepth);
            }

            var horizontalPositionForDepth = new float[maxDepth];
            horizontalPositionForDepth[0] = 0;
            for (int d = 1; d < maxDepth; ++d)
            {
                var nodesOnThisLevel = nodeDepths[d + 1];
                int maxChildren = 0;
                foreach (var node in nodesOnThisLevel)
                {
                    var children = node.Children;
                    if (children != null)
                    {
                        maxChildren = Mathf.Max(children.Count, maxChildren);
                    }
                }

                float wireLengthHeuristic = Mathf.Lerp(1, s_WireLengthFactorForLargeSpanningTrees, Mathf.Min(1, maxChildren / s_MaxChildrenThreshold));
                horizontalPositionForDepth[d] = horizontalPositionForDepth[d - 1] + s_DistanceBetweenNodes * wireLengthHeuristic;
            }

            return horizontalPositionForDepth;
        }

        private void GetSubtreeNodes(List<AnimNode> nodes, AnimNode root)
        {
            nodes.Add(root);
            var children = root.Children;
            if (children != null)
            {
                foreach (var child in children)
                {
                    GetSubtreeNodes(nodes, child);
                }
            }
        }

        private Dictionary<int, Vector2> GetBoundaryPositions(AnimNode subTreeRoot)
        {
            var extremePositions = new Dictionary<int, Vector2>();
            var descendants = new List<AnimNode>();
            GetSubtreeNodes(descendants, subTreeRoot);

            foreach (var node in descendants)
            {
                int depth = node.Depth;
                float pos = nodeToVertex[node].position.y;
                if (extremePositions.ContainsKey(depth))
                    extremePositions[depth] = new Vector2(Mathf.Min(extremePositions[depth].x, pos), Mathf.Max(extremePositions[depth].y, pos));
                else
                    extremePositions[depth] = new Vector2(pos, pos);
            }

            return extremePositions;
        }

        private void RecursiveMoveSubtree(AnimNode subtreeRoot, float yDelta)
        {
            Vector2 pos = nodeToVertex[subtreeRoot].position;
            nodeToVertex[subtreeRoot].position = new Vector2(pos.x, pos.y + yDelta);

            var children = subtreeRoot.Children;
            if (children != null)
            {
                foreach (var child in children)
                {
                    RecursiveMoveSubtree(child, yDelta);
                }
            }
        }

        private Dictionary<int, Vector2> CombineBoundaryPositions(Dictionary<int, Vector2> upperTree, Dictionary<int, Vector2> lowerTree)
        {
            var combined = new Dictionary<int, Vector2>();
            int minDepth = upperTree.Keys.Min();
            int maxDepth = System.Math.Max(upperTree.Keys.Max(), lowerTree.Keys.Max());

            for (int d = minDepth; d <= maxDepth; d++)
            {
                float upperBoundary = upperTree.ContainsKey(d) ? upperTree[d].x : lowerTree[d].x;
                float lowerBoundary = lowerTree.ContainsKey(d) ? lowerTree[d].y : upperTree[d].y;
                combined[d] = new Vector2(upperBoundary, lowerBoundary);
            }
            return combined;
        }

        private Vector2 GetAveragePosition(List<AnimNode> children)
        {
            Vector2 centroid = new Vector2();

            centroid = children.Aggregate(centroid, (current, n) => current + nodeToVertex[n].position);

            if (children.Count > 0)
                centroid /= children.Count;

            return centroid;
        }

        private void SeparateSubtrees(List<AnimNode> subroots)
        {
            if (subroots.Count < 2)
                return;

            var upperNode = subroots[0];

            var upperTreeBoundaries = GetBoundaryPositions(upperNode);
            for (int s = 0; s < subroots.Count - 1; s++)
            {
                var lowerNode = subroots[s + 1];
                Dictionary<int, Vector2> lowerTreeBoundaries = GetBoundaryPositions(lowerNode);

                int minDepth = upperTreeBoundaries.Keys.Min();
                if (minDepth != lowerTreeBoundaries.Keys.Min())
                    Debug.LogError("Cannot separate subtrees which do not start at the same root depth");

                int lowerMaxDepth = lowerTreeBoundaries.Keys.Max();
                int upperMaxDepth = upperTreeBoundaries.Keys.Max();
                int maxDepth = System.Math.Min(upperMaxDepth, lowerMaxDepth);

                for (int depth = minDepth; depth <= maxDepth; depth++)
                {
                    float delta = s_DistanceBetweenNodes - (lowerTreeBoundaries[depth].x - upperTreeBoundaries[depth].y);
                    delta = System.Math.Max(delta, 0);
                    RecursiveMoveSubtree(lowerNode, delta);
                    for (int i = minDepth; i <= lowerMaxDepth; i++)
                        lowerTreeBoundaries[i] += new Vector2(delta, delta);
                }
                upperTreeBoundaries = CombineBoundaryPositions(upperTreeBoundaries, lowerTreeBoundaries);
            }
        }

        private void RecursiveLayout(AnimNode node, int depth, float[] horizontalPositions)
        {
            var yPos = 0.0f;
            var children = node.Children;
            if (children != null)
            {
                foreach (var child in children)
                {
                    RecursiveLayout(child, depth + 1, horizontalPositions);
                }

                if (children.Count > 0)
                {
                    SeparateSubtrees(children);
                    yPos = GetAveragePosition(children).y;
                }
            }

            var pos = new Vector2(horizontalPositions[depth], yPos);
            nodeToVertex[node].position = pos;
        }
    }
}