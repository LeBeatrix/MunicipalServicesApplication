using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MunicipalServicesApplication.DataStructures;

namespace MunicipalServicesApplication.Tests
{
    internal static class Program
    {
        private static int passed, failed;
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Run("Request creation, defaults and validation", Creation);
            Run("10,000 unique references and forced collision", Uniqueness);
            Run("Case-insensitive lookup and isolated snapshots", Lookup);
            Run("Status updates, rejection and notifications", StatusUpdates);
            Run("XML restart, updates and backup", Persistence);
            Run("Corrupt XML preserved; session continues", CorruptStorage);
            Run("Save failure retains session records", FailedSave);
            Run("Basic tree and binary decision traversal", BasicTrees);
            Run("BST ordering, duplicate and missing keys", Bst);
            Run("AVL all four rotation patterns", AvlRotations);
            Run("AVL and red-black invariants after every insertion", BalancedTrees);
            Run("Heap ordering, FIFO ties, peek and empty behavior", Heap);
            Run("BFS cycles, isolated vertex and missing start", GraphTraversal);
            Run("Kruskal MST independently known weight", Mst);
            Run("Kruskal disconnected, duplicate, empty and single vertex", Forest);
            Run("MST agrees with exhaustive spanning-tree oracle", MstOracle);
            Run("Keyword title/description, category and date intersection", Search);
            Run("Event creation validation", EventValidation);
            Run("Add dialog validation and acceptance handler", AddDialog);
            Run("Existing Part 2 priority queue remains FIFO for ties", EventQueue);
            Run("Ten samples, add updates all collections, search and clear", EventIntegration);
            Run("Status form shared-store refresh and lookup feedback", StatusForm);
            Run("Forms construct and layout smoke checks", FormSmoke);
            if (args.Contains("--render")) Run("Rendered form layout checks", RenderForms);
            Run("Data structure demo uses snapshots and known MST", Demo);
            Console.WriteLine("RESULT: " + passed + " passed; " + failed + " failed.");
            return failed == 0 ? 0 : 1;
        }
        private static void Run(string name, Action test)
        {
            try { test(); passed++; Console.WriteLine("PASS " + name); }
            catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
        }
        private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); } catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
        private static IssueReport Request(string reference = null)
        {
            var issue = new IssueReport { Location = "12 Main Road", Category = "Water", Description = "Leaking pipe", AttachmentPath = "C:\\example\\photo.png" };
            if (reference != null) issue.ReferenceNumber = reference;
            return issue;
        }
        private static void Creation()
        {
            var store = new IssueStore(); var issue = Request(); store.AddIssue(issue);
            Check(issue.ReferenceNumber.Length == 32, "Full GUID expected");
            Check(store.GetAllIssues().Single().Status == RequestStatuses.Pending, "Default pending");
            Check(store.GetAllIssues().Single().AttachmentPath == issue.AttachmentPath, "Attachment lost");
            Throws<ArgumentNullException>(() => store.AddIssue(null));
            Throws<ArgumentException>(() => store.AddIssue(new IssueReport()));
        }
        private static void Uniqueness()
        {
            var store = new IssueStore();
            for (int i = 0; i < 10000; i++) store.AddIssue(Request());
            var all = store.GetAllIssues();
            Check(all.Select(r => r.ReferenceNumber).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 10000, "Duplicate generated reference");
            var duplicate = Request(all[0].ReferenceNumber.ToLowerInvariant()); store.AddIssue(duplicate);
            Check(!string.Equals(duplicate.ReferenceNumber, all[0].ReferenceNumber, StringComparison.OrdinalIgnoreCase), "Collision not repaired");
            Check(store.GetAllIssues().Count == 10001, "Collision dropped record");
        }
        private static void Lookup()
        {
            var store = new IssueStore(); store.AddIssue(Request("ABC-123"));
            Check(store.GetIssueByReference("  abc-123 ") != null, "Case/space lookup");
            Check(store.GetIssueByReference(null) == null && store.GetIssueByReference("missing") == null, "Missing lookup");
            store.GetAllIssues()[0].Status = "Changed outside store";
            store.GetIssueByReference("ABC-123").ReferenceNumber = "other";
            Check(store.GetIssueByReference("ABC-123").Status == RequestStatuses.Pending, "Snapshot mutation escaped");
        }
        private static void StatusUpdates()
        {
            var store = new IssueStore(); int changes = 0; store.Changed += (s, e) => changes++;
            store.AddIssue(Request("R1"));
            Check(store.UpdateStatus("r1", RequestStatuses.InProgress), "Update missing");
            Check(store.GetIssueByReference("R1").Status == RequestStatuses.InProgress, "Status did not update");
            Check(store.UpdateStatus("R1", RequestStatuses.Resolved), "Resolve failed");
            Check(changes == 3, "Notifications not sent");
            Check(!store.UpdateStatus("missing", RequestStatuses.Pending), "Missing request accepted");
            Throws<ArgumentException>(() => store.UpdateStatus("R1", "Unknown"));
        }
        private static string TempFolder()
        {
            string path = Path.Combine(Path.GetTempPath(), "MunicipalTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path); return path;
        }
        private static void Persistence()
        {
            string folder = TempFolder();
            try
            {
                string path = Path.Combine(folder, "requests.xml");
                var first = new IssueStore(path); first.AddIssue(Request("OLD8REF1"));
                first.UpdateStatus("old8ref1", RequestStatuses.Resolved);
                Check(first.StorageWarning == null && File.Exists(path + ".bak"), "Save/backup failed");
                var second = new IssueStore(path);
                Check(second.GetIssueByReference("OLD8REF1").Status == RequestStatuses.Resolved, "Restart lost status");
                Check(second.GetAllIssues()[0].AttachmentPath.Contains("photo.png"), "Restart lost attachment path");
            }
            finally { Directory.Delete(folder, true); }
        }
        private static void CorruptStorage()
        {
            string folder = TempFolder();
            try
            {
                string path = Path.Combine(folder, "requests.xml"); File.WriteAllText(path, "not xml");
                var store = new IssueStore(path); Check(store.StorageWarning != null, "Load error not surfaced");
                store.AddIssue(Request()); Check(store.GetAllIssues().Count == 1, "Session unusable");
                Check(File.ReadAllText(path) == "not xml", "Corrupt user file overwritten");
                // Duplicate persisted references must also protect the original file.
                var list = new List<IssueReport> { Request("DUP"), Request("dup") };
                using (var writer = new StreamWriter(path)) new System.Xml.Serialization.XmlSerializer(typeof(List<IssueReport>)).Serialize(writer, list);
                string before = File.ReadAllText(path); var duplicates = new IssueStore(path); duplicates.AddIssue(Request());
                Check(duplicates.StorageWarning != null && File.ReadAllText(path) == before, "Duplicate file was overwritten");
            }
            finally { Directory.Delete(folder, true); }
        }
        private static void FailedSave()
        {
            string folder = TempFolder();
            try
            {
                string path = Path.Combine(folder, "requests.xml"); var store = new IssueStore(path);
                Directory.CreateDirectory(path); // Destination becomes unwritable as a file after successful initial load.
                store.AddIssue(Request());
                Check(store.StorageWarning != null && store.GetAllIssues().Count == 1, "Save failure not graceful");
            }
            finally { Directory.Delete(folder, true); }
        }
        private static void BasicTrees()
        {
            var tree = new TreeNode<string>("Municipality"); tree.Add("Water").Add("R1"); tree.Add("Roads");
            Check(tree.PreOrder().SequenceEqual(new[] { "Municipality", "Water", "R1", "Roads" }), "N-ary pre-order");
            var binary = new BinaryNode<int>(1, new BinaryNode<int>(2), new BinaryNode<int>(3));
            Check(binary.PreOrder().SequenceEqual(new[] { 1, 2, 3 }), "Binary pre-order");
        }
        private static void Bst()
        {
            var tree = new BinarySearchTree<int>();
            foreach (int n in new[] { 4, 2, 6, 1, 3, 5, 7 }) tree.Insert(n);
            Check(!tree.Insert(4) && tree.Count == 7, "Duplicate handling");
            Check(tree.InOrder().SequenceEqual(Enumerable.Range(1, 7)) && tree.Validate(), "BST ordering");
            int result; Check(tree.TryGet(5, out result) && result == 5 && !tree.TryGet(9, out result), "BST lookup");
            var skew = new BinarySearchTree<int>(); foreach (int n in Enumerable.Range(1, 20)) skew.Insert(n);
            Check(skew.Height == 20, "Basic BST unexpectedly balanced");
        }
        private static void AvlRotations()
        {
            foreach (var order in new[] { new[] { 3, 2, 1 }, new[] { 1, 2, 3 }, new[] { 3, 1, 2 }, new[] { 1, 3, 2 } })
            {
                var tree = new AvlTree<int>(); foreach (int n in order) tree.Insert(n);
                Check(tree.Height == 2 && tree.Validate() && tree.InOrder().SequenceEqual(new[] { 1, 2, 3 }), "AVL rotation failed");
            }
        }
        private static void BalancedTrees()
        {
            var random = new Random(7112);
            var orders = new[] { Enumerable.Range(0, 600).ToArray(), Enumerable.Range(0, 600).Reverse().ToArray(), Enumerable.Range(0, 600).OrderBy(x => random.Next()).ToArray() };
            foreach (var order in orders)
            foreach (var tree in new BinarySearchTree<int>[] { new AvlTree<int>(), new RedBlackTree<int>() })
            {
                Check(tree.Validate(), "Empty invariants");
                foreach (int n in order) { Check(tree.Insert(n), "Insert rejected"); Check(tree.Validate(), "Balance/color invariant after " + n); }
                Check(tree.InOrder().SequenceEqual(Enumerable.Range(0, 600)), "Balanced ordering");
                Check(tree.Height <= 2 * Math.Ceiling(Math.Log(601, 2)), "Height exceeds red-black bound");
                Check(!tree.Insert(300) && tree.Validate(), "Duplicate broke balance");
                int value; Check(tree.TryGet(599, out value) && !tree.TryGet(700, out value), "Balanced lookup");
            }
        }
        private static void Heap()
        {
            var heap = new MinHeap<string>();
            heap.Enqueue("later", 8); heap.Enqueue("tie-first", -1); heap.Enqueue("middle", 3); heap.Enqueue("tie-second", -1);
            Check(heap.Peek() == "tie-first" && heap.Count == 4, "Peek altered heap");
            Check(new[] { heap.Dequeue(), heap.Dequeue(), heap.Dequeue(), heap.Dequeue() }.SequenceEqual(new[] { "tie-first", "tie-second", "middle", "later" }), "Heap order or FIFO");
            Throws<InvalidOperationException>(() => heap.Dequeue());
            var numbers = new MinHeap<int>(); var random = new Random(42); var expected = new List<int>();
            for (int i = 0; i < 500; i++) { int n = random.Next(-100, 100); expected.Add(n); numbers.Enqueue(n, n); }
            expected.Sort(); foreach (int n in expected) Check(numbers.Dequeue() == n, "Random heap ordering");
        }
        private static RequestGraph SampleGraph()
        {
            var graph = new RequestGraph();
            graph.AddEdge("A", "B", 4); graph.AddEdge("A", "C", 1); graph.AddEdge("B", "C", 2); graph.AddEdge("B", "D", 1); graph.AddEdge("C", "D", 5);
            return graph;
        }
        private static void GraphTraversal()
        {
            var graph = SampleGraph(); graph.AddVertex("isolated");
            Check(graph.BreadthFirst("A").SequenceEqual(new[] { "A", "B", "C", "D" }), "BFS order or cycle handling");
            Check(graph.BreadthFirst("isolated").Count == 1 && graph.BreadthFirst("missing").Count == 0, "BFS isolated/missing");
            Check(graph.BreadthFirst("a").Count == 4, "Graph case-insensitive IDs");
            Throws<ArgumentException>(() => graph.AddEdge("A", "a", 1));
            Throws<ArgumentOutOfRangeException>(() => graph.AddEdge("A", "E", -1));
        }
        private static void Mst()
        {
            var result = SampleGraph().MinimumSpanningForest();
            Check(result.Count == 3 && result.Sum(e => e.Weight) == 4, "Expected three edges with total 4");
            Check(result.Count(e => e.Weight == 1) == 2 && result.Count(e => e.Weight == 2) == 1, "Wrong MST edges");
        }
        private static void Forest()
        {
            var graph = new RequestGraph(); Check(graph.MinimumSpanningForest().Count == 0, "Empty graph");
            graph.AddVertex("solo"); Check(graph.MinimumSpanningForest().Count == 0, "Single vertex");
            graph.AddEdge("A", "B", 9); graph.AddEdge("A", "B", 2); graph.AddEdge("C", "D", 3);
            var forest = graph.MinimumSpanningForest();
            Check(forest.Count == 2 && forest.Sum(e => e.Weight) == 5, "Disconnected or parallel-edge forest");
        }
        // Independent brute-force oracle: enumerate all 3-edge subsets and use reachability,
        // rather than sorting or union-find, to calculate the optimum for four vertices.
        private static void MstOracle()
        {
            var random = new Random(7112);
            for (int trial = 0; trial < 30; trial++)
            {
                var graph = new RequestGraph();
                for (int a = 0; a < 4; a++) for (int b = a + 1; b < 4; b++) graph.AddEdge(a.ToString(), b.ToString(), random.Next(0, 20));
                int best = int.MaxValue;
                for (int mask = 0; mask < 64; mask++)
                {
                    var chosen = graph.Edges.Where((e, index) => (mask & (1 << index)) != 0).ToList();
                    if (chosen.Count != 3) continue;
                    var reached = new HashSet<string> { "0" };
                    for (int step = 0; step < 4; step++) foreach (var edge in chosen)
                    { if (reached.Contains(edge.From)) reached.Add(edge.To); if (reached.Contains(edge.To)) reached.Add(edge.From); }
                    if (reached.Count == 4) best = Math.Min(best, chosen.Sum(e => e.Weight));
                }
                Check(graph.MinimumSpanningForest().Sum(e => e.Weight) == best, "MST differs from exhaustive optimum");
            }
        }
        private static MunicipalEvent Event(string name = "Water workshop")
        { return new MunicipalEvent { EventName = name, Description = "Learn about recycling", Category = "Community", Location = "Town hall", EventDate = DateTime.Today.AddDays(3) }; }
        private static void Search()
        {
            var first = Event(); var second = Event("Sports day"); second.Category = "Sports"; second.Description = "Outdoor games";
            var events = new[] { first, second };
            Check(EventSearch.Filter(events, "WATER", "All Categories", null).Single() == first, "Title case search");
            Check(EventSearch.Filter(events, " ReCyClInG ", "community", first.EventDate).Single() == first, "Combined description/category/date");
            Check(EventSearch.Filter(events, "water", "Sports", null).Count == 0, "Category intersection");
            Check(EventSearch.Filter(events, "water", null, DateTime.Today).Count == 0, "Date intersection");
            Check(EventSearch.Filter(events, "", null, null).Count == 2 && EventSearch.Filter(events, "missing", null, null).Count == 0, "Empty/missing term");
        }
        private static void EventValidation()
        {
            Check(EventSearch.Validate(Event()) == null, "Valid event rejected");
            var item = Event(); item.Kind = "Announcement"; Check(EventSearch.Validate(item) == null, "Valid announcement rejected");
            item.EventName = " "; Check(EventSearch.Validate(item) != null, "Blank title accepted");
            item = Event(); item.EventDate = DateTime.Today.AddDays(-1); Check(EventSearch.Validate(item) != null, "Past event accepted");
            item = Event(); item.Kind = "Other"; Check(EventSearch.Validate(item) != null, "Invalid type accepted");
        }
        private static void AddDialog()
        {
            using (var dialog = new AddEventForm(new[] { "Water" }))
            {
                Invoke(dialog, "SaveEvent");
                Check(dialog.CreatedEvent == null && Field<Label>(dialog, "validation").Text.Contains("required"), "Blank dialog accepted");
                Field<TextBox>(dialog, "title").Text = "  New notice  ";
                Field<ComboBox>(dialog, "kind").SelectedItem = "Announcement";
                Field<ComboBox>(dialog, "category").Text = "Water";
                Field<TextBox>(dialog, "location").Text = "Library";
                Field<TextBox>(dialog, "description").Text = "Details of notice";
                Invoke(dialog, "SaveEvent");
                Check(dialog.DialogResult == DialogResult.OK && dialog.CreatedEvent.EventName == "New notice" && dialog.CreatedEvent.Kind == "Announcement", "Valid dialog not accepted/trimmed");
            }
        }
        private static void EventQueue()
        {
            var queue = new EventPriorityQueue(); var first = Event("First"); var second = Event("Second");
            queue.Enqueue(first, 1); queue.Enqueue(second, 1); queue.Enqueue(Event("Urgent"), -2);
            Check(queue.Dequeue().EventName == "Urgent" && queue.Dequeue() == first && queue.Dequeue() == second && queue.IsEmpty(), "Part 2 queue regression");
            Throws<InvalidOperationException>(() => queue.Dequeue());
        }
        private static T Field<T>(object target, string name)
        { return (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target); }
        private static void Invoke(object target, string name)
        {
            var method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(target, method.GetParameters().Length == 0 ? null : new object[] { target, EventArgs.Empty });
        }
        private static void EventIntegration()
        {
            using (var form = new LocalEventsForm())
            {
                var all = Field<List<MunicipalEvent>>(form, "allEvents"); Check(all.Count >= 10, "Fewer than ten samples");
                Check(all.Count(e => e.Kind == "Announcement") >= 2, "Sample announcements absent");
                int before = all.Count; var added = Event("Integration test addition"); added.Kind = "Announcement"; added.Category = "New category";
                form.AddMunicipalEvent(added);
                Check(all.Count == before + 1, "List not updated");
                Check(Field<Dictionary<string, List<MunicipalEvent>>>(form, "eventsByCategory")[added.Category].Contains(added), "Category index stale");
                Check(Field<SortedDictionary<DateTime, List<MunicipalEvent>>>(form, "eventsByDate")[added.EventDate].Contains(added), "Date index stale");
                Check(Field<Queue<MunicipalEvent>>(form, "upcomingEventsQueue").Contains(added), "Queue stale");
                Check(((List<MunicipalEvent>)Field<DataGridView>(form, "dgvEvents").DataSource).Contains(added), "Grid stale");
                Field<TextBox>(form, "txtKeyword").Text = "INTEGRATION TEST"; Invoke(form, "SearchEvents");
                Check(((List<MunicipalEvent>)Field<DataGridView>(form, "dgvEvents").DataSource).Single() == added, "Typed search integration");
                Field<CheckBox>(form, "chkFilterDate").Checked = true; Invoke(form, "ClearFilters");
                Check(Field<TextBox>(form, "txtKeyword").Text == "" && !Field<CheckBox>(form, "chkFilterDate").Checked && Field<ComboBox>(form, "cmbCategory").SelectedIndex == 0, "Clear filters incomplete");
                Check(Field<ListBox>(form, "lstRecommendations").Items.Count > 0, "Recommendations disappeared");
                using (var reopened = new LocalEventsForm()) Check(Field<List<MunicipalEvent>>(reopened, "allEvents").Contains(added), "Reopening lost added event");
            }
        }
        private static void StatusForm()
        {
            var store = new IssueStore();
            using (var form = new ServiceRequestStatusForm(store))
            {
                store.AddIssue(Request("REF-UI"));
                var requester = Field<DataGridView>(form, "requester"); var reviewer = Field<DataGridView>(form, "reviewer");
                Check(requester.ReadOnly && reviewer.ReadOnly, "Tables allow edits outside reviewer controls");
                Check(((List<IssueReport>)requester.DataSource).Count == 1, "New submission did not refresh");
                // Exercise the actual Reviewer event handler through its selected row.
                reviewer.BindingContext = new BindingContext();
                reviewer.CurrentCell = reviewer.Rows[0].Cells[0];
                Field<ComboBox>(form, "status").SelectedItem = RequestStatuses.Resolved;
                Invoke(form, "UpdateStatus");
                Check(store.GetIssueByReference("REF-UI").Status == RequestStatuses.Resolved, "Reviewer update handler failed");
                Check(((List<IssueReport>)requester.DataSource)[0].Status == RequestStatuses.Resolved, "Requester stale");
                Check(Field<Label>(form, "counts").Text.Contains("Resolved: 1"), "Counts stale");
                store.AddIssue(Request("REF-OTHER"));
                reviewer.CurrentCell = reviewer.Rows[1].Cells[0];
                Field<ComboBox>(form, "status").SelectedItem = RequestStatuses.InProgress;
                Invoke(form, "UpdateStatus");
                Check(store.GetIssueByReference("REF-OTHER").Status == RequestStatuses.InProgress &&
                    store.GetIssueByReference("REF-UI").Status == RequestStatuses.Resolved, "Wrong selected request updated");
                Check(((IssueReport)reviewer.CurrentRow.DataBoundItem).ReferenceNumber == "REF-OTHER", "Refresh lost reviewer selection");
                Field<TextBox>(form, "reference").Text = "ref-ui"; Invoke(form, "FindRequest");
                Check(Field<Label>(form, "feedback").Text.Contains("Found"), "Lookup failed");
                Field<TextBox>(form, "reference").Text = "missing"; Invoke(form, "FindRequest");
                Check(((List<IssueReport>)requester.DataSource).Count == 0 && Field<Label>(form, "feedback").Text.Contains("No request found"), "Not-found feedback");
            }
            store.AddIssue(Request()); // Disposed form must unsubscribe.
        }
        private static void FormSmoke()
        {
            using (var menu = new MainMenuForm()) Check(Field<Button>(menu, "btnServiceRequestStatus").Enabled, "Status menu disabled");
            using (var report = new ReportIssuesForm()) Check(Field<ComboBox>(report, "cmbCategory").Items.Count == 6, "Report categories changed");
            using (var add = new AddEventForm(new[] { "Water" })) Check(add.Controls.Count > 0, "Add form empty");
            using (var form = new LocalEventsForm())
            {
                form.PerformLayout(); var grid = Field<DataGridView>(form, "dgvEvents");
                Check(grid.Parent is TableLayoutPanel && grid.Height > 80, "Grid layout collapsed");
            }
        }
        private static void RenderForms()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "previews");
            Directory.CreateDirectory(folder);
            var store = new IssueStore(); store.AddIssue(Request("PREVIEW-REQUEST"));
            using (var menu = new MainMenuForm()) Render(menu, folder, "menu");
            using (var report = new ReportIssuesForm()) Render(report, folder, "report");
            using (var events = new LocalEventsForm()) Render(events, folder, "events");
            using (var add = new AddEventForm(new[] { "Community", "Water" })) Render(add, folder, "add-event");
            using (var status = new ServiceRequestStatusForm(store))
            {
                Render(status, folder, "status");
                var tabs = Descendants(status).OfType<TabControl>().Single();
                tabs.SelectedIndex = 1;
                Render(status, folder, "reviewer");
                tabs.SelectedIndex = 2;
                Render(status, folder, "structures");
            }
            using (var events = new LocalEventsForm())
            {
                events.Size = events.MinimumSize;
                Render(events, folder, "events-minimum");
            }
        }
        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }
        private static void Render(Form form, string folder, string name)
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-20000, -20000);
            form.Show();
            Application.DoEvents();
            form.PerformLayout();
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                bitmap.Save(Path.Combine(folder, name + ".png"), System.Drawing.Imaging.ImageFormat.Png);
            }
            form.Hide();
        }
        private static void Demo()
        {
            var empty = new List<IssueReport>(); string output = RequestStructureDemo.Render(empty);
            Check(output.Contains("Total illustrative cost: 4") && output.Contains("Supplementary DEMO records: 4") && output.Contains("invariants valid=True"), "Demo output incomplete");
            Check(empty.Count == 0, "Demo mutated input");
            var records = Enumerable.Range(1, 4).Select(i => Request("R" + i)).ToList();
            Check(RequestStructureDemo.BuildGraph(records).MinimumSpanningForest().Sum(e => e.Weight) == 4, "Demo graph edges wrong");
        }
    }
}
