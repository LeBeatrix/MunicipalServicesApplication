# Part 3 Project Completion Report - DRAFT

**AAPD7112/w final Portfolio of Evidence - student review required**

This draft describes the implementation in this repository. It is not a declaration that a lecturer has approved it. Student name, number, institution-specific format, final dates, personal reflections and screenshot captions require the student's review. Do not submit unreviewed placeholders or claims of personal experience.

## Project completion and scope

The project remains one C# Windows Forms application targeting .NET Framework 4.7.2. Part 1 issue reporting, Part 2 local events/announcements and Part 3 service-request tracking share the existing application menu. The same IssueReport submitted in Part 1 is stored and displayed in Part 3, with a unique reference, status and last-updated time. There is no duplicate service-request model or external database.

Residents can inspect records and find a reference without case sensitivity. Reviewers select a request and update Pending, In Progress or Resolved. A shared store notifies both table views and updates counts. These tabs demonstrate two workflows without authentication or access-control guarantees. In-app updates meet the requested notification scope.

Part 2 now supports free-text title/description search combined with category/date filters; clearing resets the filters. A validated creation dialog adds events and announcements to all relevant collections. There are ten samples, including two announcements. Recommendations continue using the pre-existing custom priority queue and category search history.

## Design and implementation

### Model and repository integration

The implementation extends the existing IssueReport and IssueRepository seam. IssueStore separates storage behavior from the static application facade so tests can create isolated stores. The list provides display snapshots and the dictionary provides case-insensitive reference retrieval. Copies prevent a table consumer changing the repository without validation. Full GUID generation is combined with an insertion collision check; the collision case is explicitly tested.

### Persistence and error recovery

XML persistence is local to the Windows user's LocalApplicationData directory. Each successful replacement keeps the prior XML as a backup. Writes use a temporary file before replacing the destination. File errors leave the accepted changes in memory and display a warning instead of silently claiming durable success. Invalid/corrupt XML and duplicate references disable overwriting that original file for the session. Recovery remains a deliberate user action after preserving the originals.

This is single-process storage. Concurrent instances, multi-device merging, database transactions, authentication and durable audit histories are outside the implemented requirements. Attachments are existing local paths, not managed copies.

### Advanced structures and honest separation of roles

A labeled educational tab takes request snapshots and supplements small datasets with DEMO records that are never persisted. A maximum of 40 real records bounds the intentionally unbalanced BST example.

- The ordinary tree groups municipality -> category -> request leaves; pre-order traversal visits every node.
- The binary decision tree routes statuses through yes/no branches to completed, continue work or await review.
- The BST orders references and demonstrates how sorted insertion creates skew.
- The AVL tree caches heights and uses single/double rotations to maintain balance.
- The left-leaning red-black tree uses rotations and color flips, and validates black-root, red-link and black-height rules.
- The array min heap creates an illustrative work sequence, with FIFO ties. It is separate from the existing sorted-dictionary event priority queue.
- The undirected adjacency list connects four request references with fixed illustrative coordination costs. BFS shows reachability and order using a queue/visited set.
- Kruskal sorts edges and uses union-find to reject cycles. The example MST has three edges and total cost four; disconnected inputs return a forest.

These trees/heap/graph are demonstrations. Production reference lookup actually uses the dictionary. Graph weights are educational costs with no geographical measurement. An MST connects vertices at minimum total edge weight; it does not calculate shortest travel routes or dispatch instructions. README documents concrete operations, complexity bounds and storage costs. No tree-deletion API is claimed.

### Interface and compatibility

Shared styling gives the forms consistent typography, actions, grid headers and selection colors. The events screen reparents existing controls into layout panels, retaining handlers. The new status and creation forms also use layout panels. Original Designer and resource files are preserved. Runtime-created controls can be maintained in code but are not all available as drag-and-drop Designer controls. Main-menu navigation disposes owned modal forms instead of accumulating hidden windows.

## Challenges and solutions

| Challenge observed in the code or tests | Solution and limitation |
| --- | --- |
| Part 1 reports needed a shared lifecycle | Extend the user's repository work; connect submission and status to one store. |
| Truncated references could collide | Full GUID and collision re-generation before indexing; preserve unique legacy references. |
| UI callers could mutate returned objects | Return snapshot copies; explicit validated update method sends notifications and saves. |
| XML failure must not destroy existing data | Validate all loaded records; protect invalid input from overwrite; retain failed saves in session with warnings. |
| Initial persistence test exposed uncaught InvalidDataException | Add it explicitly to handled storage errors; rerun corruption/duplicate-file regression checks. |
| Balanced-tree correctness is more than sorted traversal | Validate AVL cached heights/balance and red-black link/color/black-height invariants after each insertion. |
| MST total could look plausible despite wrong algorithm | Compare deterministic fixtures and 30 complete four-vertex graphs against an exhaustive edge-subset/reachability oracle independent of union-find. |
| Part 2 indexes can become stale after additions | Centralize acceptance through AddMunicipalEvent, rebuild all collections and immediately refresh filters/grid/recommendations. |
| Hidden form rendering gave blank initial previews | Treat that output as invalid visual evidence; use an optional offscreen-window rendering mode and retain manual display/DPI checks. |

### Personal reflection on understanding and implementing advanced data structures

One of the most challenging aspects of developing the Municipal Services Application was understanding and implementing the advanced data structures required for Part 3. Although I had previously worked with simpler collections, such as lists, dictionaries, queues and stacks, the introduction of binary search trees, AVL trees, red-black trees, heaps and graph algorithms required a deeper understanding of how data can be organised, searched and processed efficiently.

A particular challenge was understanding the differences between ordinary binary search trees and self-balancing trees. I needed to understand how AVL trees use rotations to maintain balance and how red-black trees use node colours, rotations and specific rules to prevent excessive tree height. Understanding graph traversal and minimum spanning tree algorithms also required additional attention, particularly the difference between visiting connected nodes using breadth-first search and identifying a minimum-cost set of connections using Kruskal's algorithm.

To address these challenges, I used AI-assisted development tools, examined the implementation and reviewed the results generated by the application. The Data Structures Demonstration tab helped me compare the behaviour of different algorithms using the same service request records. For example, the previously reviewed demonstration showed a binary search tree with a height of four, while the AVL and red-black trees had heights of three. This provided a practical illustration of how balancing can improve the structure of a search tree for that input; it was not a performance benchmark or a guarantee of those heights for every dataset.

The application also demonstrated how a min heap can prioritise service requests in an illustrative work sequence, how breadth-first search traverses a connected component, and how Kruskal's algorithm identifies a minimum spanning tree. These examples helped connect the theoretical concepts to practical software development. I also learned to distinguish these educational demonstrations from the dictionary used for production reference lookup and the existing event recommendation priority queue.

Through this process, I developed a better appreciation of the importance of selecting suitable data structures for specific problems. I also learned that successful implementation requires more than compiling code; algorithms must be tested, their results verified, and their efficiency understood. This experience strengthened my analytical thinking and highlighted areas where I would like to continue developing my programming knowledge.

## Verification and evidence

See verification-results.md for exact executed commands and current results. The console harness runs without third-party packages and returns failure exit codes. It covers request creation, 10,000 references and collision repair, retrieval, status updates, persistence and failures, tree operations/invariants, heap ordering, graph traversal, MST correctness, search intersections and form integration. The test harness uses its own data; it does not alter saved resident requests.

Automated form construction, bindings and handler checks do not prove mouse/keyboard usability, native file-dialog behavior, every DPI setting or Visual Studio Designer operation. Those checks are listed in manual-testing-checklist.md. Only screenshots captured from the final running application should be used as final submission evidence. Record actual outcomes and dates rather than checking boxes speculatively.

## Lessons learned - student review required

The following are technical conclusions supported by the implementation; rewrite them in your own words if they match your experience:

- A shared model/repository avoids divergent copies of an issue and its service request.
- A unique-looking reference still needs a collision check at the store boundary.
- Sorted output alone does not establish AVL or red-black correctness; invariants need testing.
- A graph's edge weights must have an explicit meaning, and an MST must not be confused with a route.
- A failed disk write needs a visible persistence distinction even when the in-memory update succeeded.
- Preserving event filters/recommendations requires updating every derived collection after creation.

### Personal and professional development

Completing this project helped me recognise the importance of combining theoretical knowledge with practical implementation. The advanced data structures were initially the most difficult part of the assessment because they required me to understand not only how the algorithms work, but also why one structure may be more appropriate than another.

Working through the implementation and reviewing the test results encouraged me to approach complex programming problems more systematically. I gained experience interpreting algorithm outputs, identifying differences in performance characteristics and evaluating whether the application behaved as expected.

Another important lesson was the value of responding constructively to feedback. After receiving feedback about the event search functionality and interface design in Part 2, I improved the application by introducing free-text searching, additional event management functionality and a more consistent interface.

Overall, this project contributed to my technical development and reinforced the importance of continuous learning, testing and improving software based on user requirements and feedback. My remaining learning goals include developing a deeper understanding of balanced-tree rotations and graph algorithms, and becoming more confident in explaining why particular data structures are appropriate for a given problem.

## Justified technology recommendations

These are design recommendations based on this repository's requirements, not an assertion that a migration has been completed:

1. Retain C# WinForms and .NET Framework 4.7.2 for this submission: it preserves a working Windows desktop solution, existing controls and assessment continuity. A platform migration would add unrelated compatibility work.
2. Keep the local list/dictionary plus XML for the required local prototype: they support a small dataset without external infrastructure and allow explicit demonstration of structures. Full-file XML saves cost O(n) and suit a modest single-process workload; they are not a multi-user transaction system.
3. Keep the testable store and dependency-free harness: isolated paths permit repeatable persistence-failure tests without resident data. A larger maintained product could consider a conventional test runner once dependency management is authorized.
4. If future requirements demand multiple simultaneous users or private requester data, first define identity, authorization, privacy, audit and concurrency requirements. Then evaluate transactional storage and an authenticated service boundary. Workflow tabs alone cannot satisfy those requirements.
5. If real dispatch is later needed, obtain valid geographic/operational data and define routing constraints before selecting a graph algorithm. The current illustrative MST should remain labeled educational until such a requirement exists.

**[STUDENT REVIEW]** Confirm these recommendations match the lecturer's rubric and add any institution-required references using sources you have checked. No external technology adoption, performance benchmarks, deployment or lecturer acceptance is claimed.

## Submission readiness

Implementation/build/test evidence is documented separately. Complete the manual checklist, capture recommended screenshots, review the personal reflections, format the final Word submission to the institution's requirements, and include only outcomes you have verified. No Git commit or push is part of this task.
