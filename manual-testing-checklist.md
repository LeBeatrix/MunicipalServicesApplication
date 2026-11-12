# Manual testing checklist and submission screenshots

Record actual results, date, Windows display scale, build configuration and any defects. Boxes are deliberately unchecked: automated evidence is in verification-results.md. Use one running instance. Do not alter existing resident XML to simulate errors; the automated tests use isolated files for that purpose.

## Visual Studio and application lifecycle

- [ ] Open the existing `.slnx` (or `.csproj` if the installed Visual Studio does not support `.slnx`), select the municipal application as startup project, and build Debug and Release.
- [ ] Open the original MainMenuForm, ReportIssuesForm and LocalEventsForm in Designer; verify no Designer error. Runtime additions appear when running, and are maintained in C# code.
- [ ] Run with F5. Open and close each feature using both its buttons and window X. Confirm the same menu remains and closing the main menu ends the process.
- [ ] Check default and minimum window sizes, maximize, and 100%, 125% and 150% Windows scaling where available. Verify fields, actions, grid scrollbars, recommendations and storage warnings remain readable.
- [ ] Tab through inputs and actions, press Enter in search boxes, and verify focus/selection contrast. Test keyboard use of category/status controls.

## Part 1 report -> Part 3 status

- [ ] Attempt an empty submission and a missing description/location; confirm validation and focused field.
- [ ] Enter a location, choose each available category as appropriate and type a description. Check progress and engagement messages update.
- [ ] Attach a supported image/document using the native file dialog. Check the filename label; cancel a subsequent selection and verify the previous choice remains. Confirm submission also works without an attachment.
- [ ] Submit a valid report. Copy the full reference from confirmation; check form clears. Submit another report and verify a different reference.
- [ ] Open Service Request Status. Confirm both records appear with original fields, attachment path if selected, Pending status, reported date and last-update time. Scroll horizontally to inspect long fields.
- [ ] Find the reference in lower case with surrounding spaces. Confirm exactly the same request appears.
- [ ] Enter an unknown reference; confirm explicit not-found feedback and empty results. Try blank lookup; confirm guidance. Select Show all to restore the table and counts.
- [ ] Try editing Requester cells; they must remain read-only. Confirm no update control appears on that tab.
- [ ] On Reviewer, select the intended row and change Pending -> In Progress -> Resolved. After each change, inspect confirmation, timestamp, total/status counts and Requester tab. Confirm another row was not updated.
- [ ] Reopen a resolved request by selecting Pending; confirm counts and status reflect it (all three transitions are allowed).
- [ ] Close and reopen the status form; confirm current status persists. Exit and restart the application; confirm references, descriptions and statuses reload from XML.
- [ ] Observe that both workflow tabs are accessible without login; do not describe them as access control in the report.

## Part 2 events and announcements

- [ ] On a fresh application process, count at least ten sample rows, including two announcements. Confirm samples are labeled and dates are relative to startup.
- [ ] Search `CLEAN` and confirm Community Clean-up. Search `responsible waste` and confirm description matching. Verify case does not affect results.
- [ ] Combine keyword, category and checked date (use the row's actual displayed date). Confirm intersection rather than independent matches.
- [ ] Search a term with no results; acknowledge the no-match message. Clear Filters; confirm keyword empty, All Categories selected, date disabled/reset and upcoming list restored.
- [ ] Search one category several times and check recommendations favor it while excluding past items. Clear Filters and verify recommendations/history remain available.
- [ ] Open Add Event / Announcement. Try blank required fields, whitespace and a past date; verify readable validation and no accepted item.
- [ ] Cancel creation and confirm no row is added.
- [ ] Create an Event with title, category, today/future date, location and description. Confirm grid refresh, new category if typed, and details/recommendations where applicable.
- [ ] Create an Announcement. Confirm Type column and details distinguish it. Search its title/description and combine with category/date.
- [ ] Close/reopen the events form; new items remain for that process. Restart the app and confirm added events reset (documented session-only behavior).

## Structure demonstration

- [ ] Open Data structures - demonstration. With fewer than four real requests, confirm supplementary records have DEMO references and never appear in Requester/Reviewer grids.
- [ ] Read the n-ary hierarchy and binary status decisions. Identify the request/category leaves and yes/no classification path.
- [ ] Compare BST, AVL and red-black in-order output, height, lookup result and invariant labels. Explain why sorted insertion skews the BST.
- [ ] Check the heap list prioritizes In Progress before Pending and excludes Resolved. Treat this as an illustrative policy, not urgency.
- [ ] Check five displayed graph edges, illustrative-cost labels, BFS order and MST's three edges with total 4. Explain that the MST is not a geographical route.
- [ ] Change a real request status, return to the demo and select Refresh demonstration. Confirm snapshot updates without changing stored data.

## Persistence failures and test harness

- [ ] Run the documented console tests in Debug and Release; record counts and exit status. XML corruption/duplicate/save-failure scenarios use temporary files, preserving resident data.
- [ ] If manually inspecting a warning, use a disposable Windows profile or a safe copy of data. Verify the message distinguishes session acceptance from disk save. Never delete the actual XML to test error handling.

## Recommended screenshots for the final Word submission

Capture from the running final app, use fictitious report content, and crop personal filesystem paths where appropriate. Number each image and explain its demonstrated requirement. Generated test previews are aids rather than final user-workflow evidence.

1. Main menu showing all three integrated feature buttons enabled.
2. Completed Report Issues form with attachment label and progress (before submission).
3. Submission confirmation with the unique reference, using the same reference in the next screenshots.
4. Requester grid showing the submitted record as Pending and status counts.
5. Successful lower-case reference lookup and a separate clear not-found result.
6. Reviewer selection/status control and update confirmation; Requester view showing that same record In Progress or Resolved.
7. Restarted application showing the same persisted record/status.
8. Local events grid showing ten or more samples, Event/Announcement types and recommendations.
9. Keyword plus category/date search with visible query and matching row; a description-keyword example if space allows.
10. Add Event/Announcement dialog with meaningful fictitious data, one validation example, and the newly added row in the grid.
11. Demonstration output for basic/binary trees and BST/AVL/red-black operations/heights.
12. Demonstration output for heap ordering, graph edges/BFS and Kruskal MST total 4 with illustrative weight labeling.
13. Developer PowerShell showing successful final builds and console test totals.

Before submission, review personal-reflection placeholders in Part3_ProjectCompletionReport_DRAFT.md, apply institution-specific Word formatting, add captions/cross-references, and ensure every claimed manual result was actually performed.
