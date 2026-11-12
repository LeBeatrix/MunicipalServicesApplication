using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApplication.DataStructures
{
    public sealed class WeightedEdge
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public int Weight { get; private set; }
        public WeightedEdge(string from, string to, int weight) { From = from; To = to; Weight = weight; }
        public override string ToString() { return From + " -- " + To + " (illustrative cost " + Weight + ")"; }
    }

    // Undirected adjacency list. Insertion order makes BFS deterministic.
    public sealed class RequestGraph
    {
        private readonly Dictionary<string, List<WeightedEdge>> adjacent = new Dictionary<string, List<WeightedEdge>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<WeightedEdge> edges = new List<WeightedEdge>();
        public int VertexCount { get { return adjacent.Count; } }
        public IReadOnlyList<WeightedEdge> Edges { get { return edges.AsReadOnly(); } }
        public void AddVertex(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) throw new ArgumentException("A vertex reference is required.");
            if (!adjacent.ContainsKey(reference)) adjacent.Add(reference, new List<WeightedEdge>());
        }
        public void AddEdge(string from, string to, int illustrativeCost)
        {
            if (illustrativeCost < 0) throw new ArgumentOutOfRangeException(nameof(illustrativeCost));
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Self edges are not supported.");
            AddVertex(from); AddVertex(to);
            var edge = new WeightedEdge(from, to, illustrativeCost);
            edges.Add(edge);
            adjacent[from].Add(edge); adjacent[to].Add(edge);
        }
        public List<string> BreadthFirst(string start)
        {
            if (start == null || !adjacent.ContainsKey(start)) return new List<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { start };
            var queue = new Queue<string>();
            var result = new List<string>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                string current = queue.Dequeue(); result.Add(current);
                foreach (var edge in adjacent[current])
                {
                    string next = string.Equals(current, edge.From, StringComparison.OrdinalIgnoreCase) ? edge.To : edge.From;
                    if (visited.Add(next)) queue.Enqueue(next);
                }
            }
            return result;
        }
        // Kruskal with union by rank and path compression. Disconnected input yields a forest.
        public List<WeightedEdge> MinimumSpanningForest()
        {
            var parent = adjacent.Keys.ToDictionary(k => k, k => k, StringComparer.OrdinalIgnoreCase);
            var rank = adjacent.Keys.ToDictionary(k => k, k => 0, StringComparer.OrdinalIgnoreCase);
            var result = new List<WeightedEdge>();
            foreach (var edge in edges.OrderBy(e => e.Weight).ThenBy(e => e.From, StringComparer.Ordinal).ThenBy(e => e.To, StringComparer.Ordinal))
            {
                string a = Find(parent, edge.From), b = Find(parent, edge.To);
                if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) continue;
                if (rank[a] < rank[b]) parent[a] = b;
                else { parent[b] = a; if (rank[a] == rank[b]) rank[a]++; }
                result.Add(edge);
            }
            return result;
        }
        private static string Find(Dictionary<string, string> parent, string key)
        {
            if (!string.Equals(parent[key], key, StringComparison.OrdinalIgnoreCase)) parent[key] = Find(parent, parent[key]);
            return parent[key];
        }
    }
}
