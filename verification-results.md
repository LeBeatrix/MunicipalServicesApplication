# Verification results

Evidence recorded on 12 November 2026 in the supplied Windows workspace. This records executed checks, not an assertion of lecturer acceptance or complete manual testing.

## Build and test commands

Commands run from the repository root using the installed Visual Studio MSBuild and .NET Framework tooling:

| Command | Observed result |
| --- | --- |
| `msbuild MunicipalServicesApplication.csproj /t:Build /p:Configuration=Debug /v:minimal /nologo` | Baseline passed before implementation; Debug builds also passed after request/status integration, event corrections and structure/demo integration. |
| `msbuild MunicipalServicesApplication.slnx /t:Rebuild /p:Configuration=Debug /v:minimal /nologo` | Final integrated solution Debug rebuild passed, exit 0. |
| `msbuild MunicipalServicesApplication.slnx /t:Rebuild /p:Configuration=Release /v:minimal /nologo` | Final integrated solution Release rebuild passed, exit 0. |
| `msbuild Tests/MunicipalServicesApplication.Tests.csproj /t:Rebuild /p:Configuration=Debug /v:minimal /nologo` | Application and test harness built, exit 0. |
| `.\Tests\bin\Debug\MunicipalServicesApplication.Tests.exe` | 24 functional groups passed, 0 failed, exit 0. |
| `msbuild Tests/MunicipalServicesApplication.Tests.csproj /t:Rebuild /p:Configuration=Release /v:minimal /nologo` | Application and test harness built, exit 0. |
| `.\Tests\bin\Release\MunicipalServicesApplication.Tests.exe` | 24 functional groups passed, 0 failed, exit 0. |
| `.\Tests\bin\Debug\MunicipalServicesApplication.Tests.exe --render` | 24 functional groups plus rendering group passed: 25 passed, 0 failed, exit 0. |
| `git diff --check` | Passed, exit 0; no whitespace errors. |

No automated test was skipped because of an environment blocker. The harness is a standalone console executable, not a `dotnet test` / Test Explorer framework. The original solution contains the one application project; the test project is built explicitly with the commands above.

## Requirement-to-evidence map

| Requirement | Changed seam | Executed evidence | Practical status |
| --- | --- | --- | --- |
| A Part 1 report becomes a Part 3 request | ReportIssuesForm -> IssueRepository -> IssueStore; status form uses same default store | Source wiring inspected; store creation/default/attachment tests; status form refresh tests with injected store | Data integration confirmed; native submission/file-dialog workflow remains a manual check. |
| Unique references | Full GUID in IssueReport; checked at IssueStore insertion | 10,000 generated references and forced duplicate ignoring case; unique legacy-style reference persisted/reloaded | Confirmed within one loaded store. |
| Retrieval and not-found feedback | Dictionary lookup; status form Find handler | Case/space lookup, null/missing reference, actual Find handler/result grid and feedback checks | Confirmed. |
| Reviewer update, requester refresh, counts | UpdateStatus; store Changed event; both grid data sources | Actual Reviewer handler, two-row targeting and selection retention, Requester binding and counts inspected | Confirmed at form/store integration level. |
| Safe local persistence | XML load/save/replace/backup | Real isolated temporary files; save/restart/status/attachment preservation, corrupt and duplicate XML protected, failed save retains memory | Confirmed for tested scenarios; no multi-process behavior claimed. |
| Typed event search plus existing filters | EventSearch.Filter and SearchEvents | Title/description case checks, category/date intersection, missing/empty terms, form typed-search path | Confirmed. |
| Creation, ten samples and collection updates | AddEventForm, AddMunicipalEvent, sample initialization | Creation handler trims/validates and accepts Announcement; ten samples/two announcements; list/category/date/queue/grid updated; reopened form retains item | Confirmed at form integration level; physical modal typing remains manual. |
| Preserve recommendations and event priority queue | Existing category history/scoring and EventPriorityQueue | FIFO equal-priority and lower-priority-first checks; recommendations remain populated after add/search/clear | Confirmed for exercised cases; broader preference UX remains manual. |
| Basic and binary trees | BasicTrees.cs | Pre-order expected values and executable status-decision demonstration | Confirmed. |
| BST / AVL / red-black | SearchTrees.cs | Ordering, duplicates, missing keys; four AVL rotations; 600 ascending/descending/seeded-shuffled inserts per balanced type, validating after each insertion; height bounds | Confirmed for implemented insert/search/traversal operations. No deletion implemented. |
| Heap | MinHeap.cs | Peek/count, negative priorities, FIFO ties, empty exception, 500 seeded priorities compared to independent sorted results | Confirmed. |
| Graph / BFS | RequestGraph.cs | Cyclic connected graph, isolated/missing vertices, case-insensitive IDs, deterministic BFS and invalid edges | Confirmed. |
| Kruskal MST | MinimumSpanningForest | Known cost 4/three edges, disconnected/parallel/empty/single cases, 30 four-vertex complete graphs compared to exhaustive connected-subset oracle | Confirmed for tested cases. |
| Demonstration separation and labeling | RequestStructureDemo; status tab | Empty input supplemented without mutating source, known demo MST and invariant output; source confirms dictionary production lookup and illustrative weights | Confirmed. |
| UI layout/styling | AppTheme; events/status/add layout panels | Form construction/layout smoke checks; offscreen rendered PNGs visually inspected for menu, report, events, add dialog, requester, reviewer, demonstration and minimum-size events | Partial visual evidence; DPI/accessibility/designer/native-dialog manual checks outstanding. |
| Documentation | README, lecturer feedback file, completion draft, checklist, this file | Content inspected against implemented behavior; personal reflections marked for review | Delivered; student must review and format final submission. |

## Defects found and corrected during verification

1. Duplicate persisted records initially raised an uncaught InvalidDataException. It is now explicitly handled, the original XML stays unchanged and session storage remains usable. The regression passes.
2. Reviewer refresh initially reset the selected row. Selection now follows the same reference after rebinding; a two-row update regression verifies that another request is not changed.
3. Initial hidden-form DrawToBitmap previews were blank. Those images were not treated as UI evidence. The optional `--render` mode creates actual offscreen windows, pumps events and captures populated controls. Revised previews were inspected. Preview artifacts are ignored build outputs under Tests/bin, not resident data or final submission screenshots.

## Remaining manual verification

No automated check in this harness failed at handoff. Native attachment selection/cancel behavior, the real confirmation dialogs/submission workflow, user mouse/keyboard navigation, Visual Studio Designer loading, Windows scaling at 100/125/150%, and real-application restart through the UI were not fully executed end to end. Follow manual-testing-checklist.md and record actual results. XML restart behavior was tested independently with isolated stores.

## Scope and repository safety

Existing uncommitted repository work was preserved and extended. The original target framework, namespace, project identity, generated Designer files, resources and solution structure are retained. No new third-party dependency, external database, commit or push was introduced. Tests use isolated stores and never write the resident's saved request XML. Request persistence is single-process; event additions are session-only; workflow tabs provide no authentication. The tree/heap/graph tab is labeled educational and uses illustrative weights.
