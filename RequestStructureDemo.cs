using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MunicipalServicesApplication.DataStructures;

namespace MunicipalServicesApplication
{
    public static class RequestStructureDemo
    {
        public static RequestGraph BuildGraph(IList<IssueReport> fourRequests)
        {
            if (fourRequests.Count < 4) throw new ArgumentException("Four demonstration records are required.");
            string a = fourRequests[0].ReferenceNumber, b = fourRequests[1].ReferenceNumber;
            string c = fourRequests[2].ReferenceNumber, d = fourRequests[3].ReferenceNumber;
            var graph = new RequestGraph();
            graph.AddEdge(a, b, 4); graph.AddEdge(a, c, 1); graph.AddEdge(b, c, 2);
            graph.AddEdge(b, d, 1); graph.AddEdge(c, d, 5);
            return graph;
        }

        public static string Render(IEnumerable<IssueReport> requests)
        {
            var snapshot = requests.OrderBy(r => r.ReferenceNumber, StringComparer.OrdinalIgnoreCase).ToList();
            int realCount = snapshot.Count;
            // Bound the intentionally skewed BST demonstration to keep recursion and output usable.
            var records = snapshot.Take(40).ToList();
            int displayedRealCount = records.Count;
            for (int i = 1; records.Count < 4; i++)
            {
                string reference = "DEMO-" + i;
                if (records.Any(r => string.Equals(r.ReferenceNumber, reference, StringComparison.OrdinalIgnoreCase))) continue;
                records.Add(new IssueReport { ReferenceNumber = reference, Category = i % 2 == 0 ? "Roads" : "Water",
                    Location = "Illustrative location", Description = "Demonstration only", Status = RequestStatuses.All[(i - 1) % 3] });
            }
            var output = new StringBuilder();
            output.AppendLine("EDUCATIONAL DEMONSTRATION - snapshot, refreshed using the button below.");
            output.AppendLine("Real requests: " + realCount + ". Displayed (maximum 40): " + displayedRealCount + ". Supplementary DEMO records: " + (records.Count - displayedRealCount) + ". Demo records are never saved to the repository.");
            output.AppendLine("Production lookup: case-insensitive dictionary, expected O(1). Trees below are demonstrations, not production indexes.");
            output.AppendLine();
            var hierarchy = new TreeNode<string>("Municipal requests");
            foreach (var group in records.GroupBy(r => r.Category).OrderBy(g => g.Key))
            {
                var category = hierarchy.Add(group.Key);
                foreach (var record in group) category.Add(record.ReferenceNumber + " | " + record.Status);
            }
            output.AppendLine("1. BASIC TREE: municipality -> categories -> request leaves. Pre-order visit O(n), add child amortized O(1).");
            AppendHierarchy(output, hierarchy, 0);
            var decision = new BinaryNode<string>("Resolved?", new BinaryNode<string>("Archive / completed"),
                new BinaryNode<string>("In Progress?", new BinaryNode<string>("Continue work"), new BinaryNode<string>("Await review")));
            output.AppendLine("\r\n2. BINARY TREE: left = yes, right = no. Pre-order traversal O(n); decision follows O(h) path.");
            output.AppendLine(string.Join(" -> ", decision.PreOrder()));
            foreach (var record in records.Take(4))
            {
                var node = record.Status == RequestStatuses.Resolved ? decision.Left : decision.Right;
                if (node == decision.Right) node = record.Status == RequestStatuses.InProgress ? node.Left : node.Right;
                output.AppendLine(record.ReferenceNumber + " => " + node.Value);
            }
            var comparer = Comparer<IssueReport>.Create((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.ReferenceNumber, b.ReferenceNumber));
            var bst = new BinarySearchTree<IssueReport>(comparer);
            var avl = new AvlTree<IssueReport>(comparer);
            var rb = new RedBlackTree<IssueReport>(comparer);
            foreach (var record in records.OrderBy(r => r.ReferenceNumber, StringComparer.OrdinalIgnoreCase))
            { bst.Insert(record); avl.Insert(record); rb.Insert(record); }
            output.AppendLine("\r\n3. ORDERED TREES: insert the same references in sorted order; duplicates are ignored.");
            AppendTree(output, "BST", bst, records[0]);
            AppendTree(output, "AVL", avl, records[0]);
            AppendTree(output, "Red-black (left-leaning)", rb, records[0]);
            output.AppendLine("BST insert/search O(h), worst O(n). AVL and red-black insert/search worst O(log n). In-order traversal O(n). No deletion implemented.");
            var heap = new MinHeap<IssueReport>();
            foreach (var record in records.OrderBy(r => r.DateReported).ThenBy(r => r.ReferenceNumber))
                if (record.Status != RequestStatuses.Resolved) heap.Enqueue(record, record.Status == RequestStatuses.InProgress ? 0 : 1);
            output.AppendLine("\r\n4. MIN HEAP: illustrative work list, In Progress priority 0, Pending 1; Resolved excluded. FIFO ties by enqueue order.");
            while (heap.Count > 0) { var next = heap.Dequeue(); output.AppendLine(next.ReferenceNumber + " | " + next.Status); }
            output.AppendLine("Enqueue/dequeue O(log n), peek O(1); this policy is illustrative, not an urgency assessment.");
            var graphRecords = records.Take(4).ToList();
            var graph = BuildGraph(graphRecords);
            output.AppendLine("\r\n5. UNDIRECTED GRAPH: first four snapshot records; deterministic illustrative coordination costs, NOT geographic distances.");
            output.AppendLine("Adjacency list space O(V+E). Add vertex/edge expected amortized O(1).");
            foreach (var edge in graph.Edges) output.AppendLine(edge.ToString());
            output.AppendLine("BFS from " + graphRecords[0].ReferenceNumber + ": " + string.Join(" -> ", graph.BreadthFirst(graphRecords[0].ReferenceNumber)));
            output.AppendLine("BFS uses a queue and visited set: O(V+E), reachable component only.");
            var mst = graph.MinimumSpanningForest();
            output.AppendLine("\r\n6. KRUSKAL MINIMUM SPANNING TREE (connected four-vertex demo):");
            foreach (var edge in mst) output.AppendLine(edge.ToString());
            output.AppendLine("Total illustrative cost: " + mst.Sum(e => e.Weight) + " (expected 4); 3 edges connect 4 requests without a cycle.");
            output.AppendLine("Sort edges + union-find: O(E log E + V); disconnected graphs return a minimum spanning forest.");
            return output.ToString();
        }
        private static void AppendHierarchy(StringBuilder text, TreeNode<string> node, int depth)
        {
            text.AppendLine(new string(' ', depth * 2) + node.Value);
            foreach (var child in node.Children) AppendHierarchy(text, child, depth + 1);
        }
        private static void AppendTree(StringBuilder text, string name, BinarySearchTree<IssueReport> tree, IssueReport key)
        {
            IssueReport found;
            text.AppendLine(name + ": count=" + tree.Count + ", height=" + tree.Height + ", invariants valid=" + tree.Validate());
            text.AppendLine("  In-order: " + string.Join(", ", tree.InOrder().Select(r => r.ReferenceNumber)));
            text.AppendLine("  Search " + key.ReferenceNumber + ": " + (tree.TryGet(key, out found) ? found.Status : "not found"));
        }
    }
}
