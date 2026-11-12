# Lecturer feedback changes - Parts 2 and 3

## Part 2 corrections

| Feedback / requirement | Actual change | Evidence and scope |
| --- | --- | --- |
| Free-text event searching | `txtKeyword` added to `LocalEventsForm`; title and description compared using OrdinalIgnoreCase; surrounding whitespace trimmed. Search button and Enter apply the query. | `EventSearch.Filter` unit checks plus form integration test. |
| Preserve existing filters | Keyword predicate intersects category and optional exact calendar date. Category dictionary still narrows the candidates. | Automated combined-filter checks. |
| Reset filters | Clear Filters clears keyword, selects All Categories, disables date filtering, resets date to today and restores upcoming queue results. | Automated form integration check. Recommendation history is intentionally retained. |
| Create events/announcements | `AddEventForm` offers title, Event/Announcement type, editable category, date, location and description. Required text and non-past date are validated. | Validation tests and shared add-path integration; actual modal typing remains on the manual checklist. |
| Update collections immediately | Accepted items update list, category dictionary, date-sorted dictionary, upcoming queue, category selector, grid and recommendations. New categories appear; existing categories are normalized ignoring case. Filters reset to show the addition. | Integration checks inspect all indexes/queue and visible grid data. Session-local additions survive form reopening. |
| At least ten samples | Seven original records retained; Water Maintenance Notice, Library Reading Circle and Road Works Notice added. Two items are marked Announcement. | Automated count/type checks. Notices explicitly say sample/illustrative; no real maintenance claim is made. |
| Design/readability | Original controls reparented into TableLayoutPanel/FlowLayoutPanel; resizable grid, coherent teal actions/headers, Segoe UI, alternating rows, clearer title/type labels and labeled keyword. | Build and form layout smoke checks; see verification report for visual evidence and remaining DPI checks. |
| Preserve recommendations | Existing category history, frequency/date scoring and `EventPriorityQueue` preserved. Keyword text is recorded after the category field without changing category parsing. | Original queue FIFO tests plus nonempty recommendations integration check. |

## Integrated Part 3 changes

- The existing disabled status menu action is enabled and wired to `ServiceRequestStatusForm`.
- The existing `IssueReport` and user-started `IssueRepository` integration are extended, not replaced with another application.
- A Part 1 report is the Part 3 service request: same stored reference, location, category, description, attachment path and reported time. Status and last-updated fields are added.
- Requester tables are read-only. Reviewer uses explicit validated controls. There is no login, and the tabs make no security claim.
- Full GUID references plus collision checking replace truncated identifiers. Lookup ignores case and surrounding whitespace. Missing references have explicit in-app feedback.
- Counts, change notifications and a confirmation message expose status changes. Local XML saves requests across restarts; storage warnings preserve usable session state and protect invalid existing files.
- Basic/n-ary and binary trees, BST, AVL, explicit red-black tree, binary min heap, weighted adjacency-list graph, BFS and Kruskal MST have separate testable implementations and a clearly labeled demonstration tab using request data. Production lookup remains a dictionary. Illustrative graph weights are not real distances.
- Main-menu navigation uses owned modal forms and disposal. Back closes the Report form instead of creating another menu and leaving the original hidden process alive.

## Compatibility and limits

The application remains one .NET Framework 4.7.2 WinForms application with the original namespace and project identity. Generated Designer and resource files remain unchanged. New runtime layouts are maintained in regular C# files. The console test project is a verification utility, not a second municipal application. No external database, dependencies, paid services, login, email, SMS or cloud hosting are added.

Request persistence supports one process on one device. Events and recommendation history are session-based; event history remains per form as before. There is no identity-based filtering, audit trail, dispatch system or real geographic routing. Review the manual checklist and replace personal-reflection placeholders before the final Word submission.
