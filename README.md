# Municipal Services Application - AAPD7112/w Part 3

An integrated C# Windows Forms application for reporting municipal issues, browsing local events/announcements, and tracking the same reports as service requests. Existing namespace, project identity and .NET Framework 4.7.2 target are preserved. No third-party packages or external database are used.

## Build and run

Use Windows, Visual Studio with the .NET desktop development workload, and the .NET Framework 4.7.2 targeting pack. Open `MunicipalServicesApplication.slnx` in a Visual Studio version supporting that solution format; alternatively open the existing `.csproj` directly. Select the application as startup project, build, then press F5. No credentials or services need configuration.

From a Visual Studio Developer PowerShell, at the repository root:

```powershell
msbuild MunicipalServicesApplication.csproj /t:Rebuild /p:Configuration=Debug /v:minimal /nologo
msbuild MunicipalServicesApplication.csproj /t:Rebuild /p:Configuration=Release /v:minimal /nologo
& .\bin\Debug\MunicipalServicesApplication.exe
```

This is a classic .NET Framework project; the .NET SDK alone does not replace the targeting pack or desktop build tools. The application project can build independently of the console test harness.

## Features and workflows

- **Report Issues:** required location, category and description, optional image/document attachment path, progress indicator and confirmation with reference. A submission immediately enters the shared service-request repository as Pending. Attachment selection behavior is preserved; files are not uploaded, copied or opened by the status form.
- **Service Request Status:** enabled main-menu button opens a dedicated form. Both Requester and Reviewer grids display reference, location, category, description, reported date, status, last update and attachment path. Horizontal scrolling exposes wider fields. Enter a reference, in any case, and select Find request or press Enter. Missing/blank references receive explicit feedback. Show all restores the list. Counts describe the entire store and the number of visible matches.
- **Requester:** read-only table. **Reviewer:** selects a row and changes its status using the validated Pending / In Progress / Resolved control. Tables refresh through store change notifications, and an in-app message confirms the change. All three statuses may be selected, including reopening a resolved request; no restricted transition policy was requested.
- **Local Events and Announcements:** ten initial samples, including two announcements. Search title or description without case sensitivity, together with category and optional exact date. Search or Enter applies the filters. Clear Filters clears the keyword, category and optional date and restores upcoming events. Existing category-based recommendation history and custom priority queue remain in use.
- **Add Event / Announcement:** enter title, type, category, date, location and description. A new category may be typed. Required text is trimmed and validated; dates must be today or later. The list, category dictionary, date-sorted dictionary, upcoming queue, category selector, grid and recommendations refresh immediately. Filters reset so that the new item is visible. Added items survive closing/reopening the form during this process; events are deliberately session-only.
- **Coherent UI:** shared Segoe UI styling, teal actions/headers, alternating table rows and readable selection colors. The events form uses resizable layout panels and preserves the original controls and event handlers. The status form and Add dialog use layout panels. Existing generated Designer and resource files are unchanged. New controls/layouts are constructed in normal C# code; they are therefore not all editable through the Designer surface.

## Local request storage and scope

`IssueRepository` wraps one `IssueStore`, shared by Part 1 and Part 3. Production lookup uses `Dictionary<string, IssueReport>` with `OrdinalIgnoreCase`; the list supplies snapshots for display. Snapshot copies prevent consumers bypassing validation/persistence by mutating returned records.

New references use a full 32-character GUID. On insertion the store checks for an existing reference ignoring case and regenerates on a collision before accepting the record. Legacy/nonempty references are retained when unique. GUID generation alone is not treated as a uniqueness proof: the loaded store enforces uniqueness. This guarantees unique accepted references within that store, not a distributed global identifier service.

Requests save to `%LOCALAPPDATA%\MunicipalServicesApplication\requests.xml`. No file is created merely by browsing. Saves serialize to a sibling `.tmp` file, then move/replace the main file; replacement keeps the previous version as `.bak`. A failed save retains the accepted record/status in memory and visibly warns that disk persistence failed. A later successful save includes those session changes. Malformed XML, duplicate references or invalid records produce a warning and protect the original file from overwrite for the rest of that session. DTD processing is prohibited. All records are validated before any loaded record is indexed.

If recovery is needed, close the application, copy the XML and backup somewhere safe, inspect them, and restore a valid backup only after preserving the originals. No automatic deletion or silent reset occurs. A `.bak` is one previous version, not a full backup history. Disk-full, permissions, and invalid-file paths are handled as storage errors; tests simulate representative failures rather than exhausting a real disk.

This is a **single-process local prototype**. Do not run simultaneous instances against the same XML file: cross-process merging/locking is not implemented. Requester/Reviewer are workflow tabs, without authentication or privacy separation. Everyone using this application/device can see local requests and use the Reviewer controls. There is no email, SMS, network notification, status audit history, or cloud service. Attachment paths may stop referring to a file after the user moves/deletes it.

## Advanced data structures: actual roles and complexity

Open Service Request Status > **Data structures - demonstration** and select Refresh demonstration after changing requests. The output is a snapshot of up to 40 real requests, supplemented to four with labeled DEMO records when needed. DEMO records never enter the repository. The cap keeps an intentionally skewed BST and its output manageable. Trees, heap and graph in this tab are educational comparisons; they do not secretly replace production dictionary lookup.

Here `n` is structure size, `h` tree height, `V` vertices, `E` edges, `k` distinct event priorities, and `m` search matches. Bounds below assume normal constant-time key comparisons; comparing variable-length strings also costs their compared length.

| Structure / source | Concrete operation and role | Time / storage |
| --- | --- | --- |
| Basic n-ary tree, `DataStructures/BasicTrees.cs` | Municipality root -> category children -> reference/status leaves. `Add`, recursive `PreOrder`. | Add amortized O(1); traverse O(n); O(n) storage and O(h) traversal stack. |
| Binary tree, same file | Decision root `Resolved?`: yes -> completed; no -> `In Progress?`: yes -> continue, no -> await review. Demonstrates two-child nodes and classifies snapshot statuses. | Follow decision O(h), pre-order O(n); O(n) storage. No ordering assumption. |
| BST, `DataStructures/SearchTrees.cs` | Insert references in sorted order, try lookup of an actual record, show in-order results, height and validation. Duplicate comparator keys are ignored. | Insert/search O(h), worst O(n), average O(log n) only for suitable insertion order; traversal/validation O(n), storage O(n). Sorted insertion intentionally shows skew. |
| AVL, same file | Same inputs/lookups; cached heights, single/double rotations; validates ordering, exact cached heights and balance factors. | Insert/search worst O(log n); each rotation O(1); traversal/validation O(n), storage O(n). |
| Red-black, same file | Explicit left-leaning red-black implementation; rotations, color flips, black root, no red-red or right-red links, equal black height. No reliance on framework containers as a claimed custom tree. | Insert/search worst O(log n); rotation/color flip O(1); traversal/validation O(n), storage O(n). |
| Min heap, `DataStructures/MinHeap.cs` | Array-backed illustrative open-work list: In Progress priority 0, Pending 1, excludes Resolved. Equal priorities dequeue FIFO, using sequence numbers. This is an example policy, not urgency scoring. | Enqueue/dequeue O(log n) amortized with array growth; peek O(1); O(n) storage. |
| Graph, `DataStructures/RequestGraph.cs` | Undirected adjacency list of four snapshot request references; vertices and weighted edges are displayed. | Add vertex/edge expected amortized O(1); O(V+E) storage. Parallel edges permitted, self-edges/negative costs rejected. |
| BFS, same file | Queue and visited set traverse the connected component from the first request, once per vertex. Missing start returns empty. | O(V+E) time; O(V) auxiliary space; disconnected components are not automatically traversed. |
| Kruskal MST, same file | Sort edges, skip cycles with union-find (path compression + union by rank), display chosen connections and total. Disconnected graphs return a minimum spanning forest. | O(E log E + V) time overall (union-find amortized near constant per operation); O(V+E) working storage including sorted edges. |
| Existing event priority queue, `LocalEventsForm.cs` | `SortedDictionary<int, Queue<MunicipalEvent>>`: lower score first, FIFO ties. Powers real recommendations based on category search frequency and event date. Preserved separately from the new binary heap. | Enqueue/dequeue O(log k), count/empty O(1); O(n+k) storage. This is not described as an array heap. |
| Existing Part 2 collections | List of events; hash dictionary by category; date-sorted dictionary; upcoming FIFO queue; LIFO search history; category hash set. | Category lookup expected O(1) plus bucket scan; rebuild O(n log n); FIFO/LIFO operations O(1) amortized. Combined search scans selected bucket and sorts matches: O(n + m log m) worst; result O(m) space. |

Ordered trees implement insertion, search, in-order traversal and validation; no deletion operation is claimed. A sorted four-reference BST has height 4; AVL/red-black heights remain smaller. Reference comparisons use `OrdinalIgnoreCase` in the demonstration.

### Deterministic graph example

Let A, B, C and D be the first four displayed references. Edges are A-B=4, A-C=1, B-C=2, B-D=1, C-D=5. Weights are **illustrative coordination cost units**, never measured geographic distances. BFS from A visits A, B, C, D in insertion order. Kruskal selects A-C (1), B-D (1), B-C (2), total **4**, connecting four vertices with three edges. This is an educational minimum-cost connection example, not routing/dispatch optimization or a shortest-path algorithm.

## Automated tests

A dependency-free .NET Framework console harness is provided in `Tests/`. It exits nonzero on failure; no NuGet restore or external test framework is needed. Run from Developer PowerShell:

```powershell
msbuild Tests\MunicipalServicesApplication.Tests.csproj /t:Rebuild /p:Configuration=Debug /v:minimal /nologo
& .\Tests\bin\Debug\MunicipalServicesApplication.Tests.exe
msbuild Tests\MunicipalServicesApplication.Tests.csproj /t:Rebuild /p:Configuration=Release /v:minimal /nologo
& .\Tests\bin\Release\MunicipalServicesApplication.Tests.exe
```

There are 24 functional test groups, including 10,000 generated references and a forced case-insensitive collision, request validation/retrieval/snapshots, valid/invalid status changes and notifications, persistence/restart/backups, malformed/duplicate XML preservation, save-failure retention, basic/binary tree traversal, BST operations, four AVL rotations, 600 ascending/descending/shuffled insertions with balance/color checks after every insertion, heap tie/random ordering, cyclic/disconnected BFS, MST fixtures and 30 exhaustive spanning-tree comparisons, combined event search, event validation, existing event queue behavior, all event collection updates, Requester/Reviewer integration, form construction and demo output.

Store tests use isolated temporary paths and clean up only the directories they create. Status form tests inject an isolated store. Event additions during tests affect only the test process. Tests never submit to or update the resident's saved request XML. Some UI integration checks use reflection to exercise existing private event handlers without showing modal dialogs; this is a test-only technique.

Optional layout previews on a Windows interactive desktop:

```powershell
& .\Tests\bin\Debug\MunicipalServicesApplication.Tests.exe --render
```

This runs the same checks plus a rendering group which briefly creates offscreen forms and writes PNGs under `Tests\bin\Debug\previews`. Previews are layout inspection aids, not proof of interactive accessibility or end-to-end testing. Run the manual checklist before submission.

## Submission documents

- `lecturer-feedback-changes.md`: feedback mapped to actual changes and limitations.
- `Part3_ProjectCompletionReport_DRAFT.md`: draft completion report, tradeoffs and reflection prompts needing student review.
- `manual-testing-checklist.md`: manual checks and recommended screenshots for the final Word submission.
- `verification-results.md`: executed commands and observed evidence, with manual gaps distinguished.
