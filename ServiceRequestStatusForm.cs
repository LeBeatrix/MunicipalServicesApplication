using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalServicesApplication
{
    // Controls are built in ordinary code; existing generated Designer files remain untouched.
    public class ServiceRequestStatusForm : Form
    {
        private readonly TextBox reference = new TextBox { Width = 330, AccessibleName = "Request reference number" };
        private readonly Label counts = AppTheme.Label("");
        private readonly Label feedback = AppTheme.Label("All requests on this device are visible. Tabs do not provide authentication.");
        private readonly Label storage = AppTheme.Label("");
        private readonly DataGridView requester = CreateGrid();
        private readonly DataGridView reviewer = CreateGrid();
        private readonly ComboBox status = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
        private string activeReference;
        private readonly IssueStore store;

        public ServiceRequestStatusForm() : this(IssueRepository.Store) { }

        public ServiceRequestStatusForm(IssueStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            Text = "Municipal Services | Service Request Status";
            ClientSize = new Size(1180, 700);
            MinimumSize = new Size(900, 560);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var title = AppTheme.Label("Service Request Status");
            title.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            title.ForeColor = AppTheme.Teal;
            layout.Controls.Add(title);
            var search = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            search.Controls.Add(AppTheme.Label("Reference number:"));
            search.Controls.Add(reference);
            search.Controls.Add(AppTheme.Button("Find request", FindRequest));
            search.Controls.Add(AppTheme.Button("Show all", (s, e) => { activeReference = null; reference.Clear(); RefreshRequests(); feedback.Text = "Showing all requests."; }));
            layout.Controls.Add(search);
            layout.Controls.Add(counts);
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var requestTab = new TabPage("Requester - read only");
            requestTab.Controls.Add(requester);
            var reviewTab = new TabPage("Reviewer - update status");
            var reviewLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            reviewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            reviewLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            reviewLayout.Controls.Add(reviewer);
            var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            status.Items.AddRange(RequestStatuses.All);
            status.SelectedIndex = 0;
            actions.Controls.Add(AppTheme.Label("Selected request status:"));
            actions.Controls.Add(status);
            actions.Controls.Add(AppTheme.Button("Update status", UpdateStatus));
            reviewLayout.Controls.Add(actions);
            reviewTab.Controls.Add(reviewLayout);
            tabs.TabPages.Add(requestTab);
            tabs.TabPages.Add(reviewTab);
            var demoTab = new TabPage("Data structures - demonstration");
            var demoLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            demoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            demoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var demoText = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both, WordWrap = false, AccessibleName = "Data structure demonstration output" };
            demoText.Text = RequestStructureDemo.Render(store.GetAllIssues());
            demoLayout.Controls.Add(demoText);
            demoLayout.Controls.Add(AppTheme.Button("Refresh demonstration", (s, e) => demoText.Text = RequestStructureDemo.Render(store.GetAllIssues())));
            demoTab.Controls.Add(demoLayout);
            tabs.TabPages.Add(demoTab);
            layout.Controls.Add(tabs);
            var messages = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            feedback.MaximumSize = new Size(1050, 0);
            storage.MaximumSize = new Size(1050, 0);
            storage.ForeColor = Color.Firebrick;
            messages.Controls.Add(feedback);
            messages.Controls.Add(storage);
            messages.SizeChanged += (s, e) => {
                int width = Math.Max(200, messages.ClientSize.Width - 20);
                feedback.MaximumSize = new Size(width, 0);
                storage.MaximumSize = new Size(width, 0);
            };
            layout.Controls.Add(messages);
            var footer = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            footer.Controls.Add(AppTheme.Button("Close", (s, e) => Close()));
            layout.Controls.Add(footer);
            Controls.Add(layout);
            reference.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { FindRequest(s, e); e.SuppressKeyPress = true; } };
            reviewer.SelectionChanged += (s, e) => { var issue = reviewer.CurrentRow?.DataBoundItem as IssueReport; if (issue != null) status.SelectedItem = issue.Status; };
            store.Changed += RepositoryChanged;
            Disposed += (s, e) => store.Changed -= RepositoryChanged;
            AppTheme.Apply(this);
            RefreshRequests();
        }
        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, MultiSelect = false, RowHeadersVisible = false,
                AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells };
            string[] properties = { "ReferenceNumber", "Location", "Category", "Description", "DateReported", "Status", "LastUpdated", "AttachmentPath" };
            string[] titles = { "Reference number", "Location", "Category", "Description", "Reported", "Status", "Last updated", "Attachment path" };
            for (int i = 0; i < properties.Length; i++)
            {
                var column = new DataGridViewTextBoxColumn { DataPropertyName = properties[i], Name = properties[i], HeaderText = titles[i], MinimumWidth = 100 };
                if (properties[i] == "DateReported" || properties[i] == "LastUpdated") column.DefaultCellStyle.Format = "dd MMM yyyy HH:mm";
                grid.Columns.Add(column);
            }
            return grid;
        }
        private void FindRequest(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(reference.Text)) { feedback.Text = "Enter a reference number to find a request."; reference.Focus(); return; }
            var found = store.GetIssueByReference(reference.Text);
            activeReference = reference.Text.Trim();
            RefreshRequests();
            feedback.Text = found == null ? "No request found for reference: " + activeReference :
                "Found " + found.ReferenceNumber + " | " + found.Status + " | Updated " + found.LastUpdated.ToString("g");
        }
        private void UpdateStatus(object sender, EventArgs e)
        {
            var issue = reviewer.CurrentRow?.DataBoundItem as IssueReport;
            if (issue == null) { feedback.Text = "Select a request in the Reviewer table first."; return; }
            string next = status.SelectedItem as string;
            if (next == null) return;
            bool updated = store.UpdateStatus(issue.ReferenceNumber, next);
            feedback.Text = updated ? "Status updated: " + issue.ReferenceNumber + " is now " + next + ". Requester view refreshed." : "Request no longer available.";
        }
        private void RepositoryChanged(object sender, EventArgs e) { RefreshRequests(); }
        private void RefreshRequests()
        {
            string selectedRequester = (requester.CurrentRow?.DataBoundItem as IssueReport)?.ReferenceNumber;
            string selectedReviewer = (reviewer.CurrentRow?.DataBoundItem as IssueReport)?.ReferenceNumber;
            List<IssueReport> all = store.GetAllIssues();
            var visible = activeReference == null ? all : all.Where(i => string.Equals(i.ReferenceNumber, activeReference, StringComparison.OrdinalIgnoreCase)).ToList();
            requester.DataSource = visible.ToList();
            reviewer.DataSource = visible.ToList();
            RestoreSelection(requester, selectedRequester);
            RestoreSelection(reviewer, selectedReviewer);
            counts.Text = "All requests: " + all.Count + "     " + string.Join("     ", RequestStatuses.All.Select(s => s + ": " + all.Count(i => i.Status == s))) + "     Showing: " + visible.Count;
            storage.Text = store.StorageWarning ?? "Requests are saved locally on this device.";
            storage.ForeColor = store.StorageWarning == null ? AppTheme.Teal : Color.Firebrick;
        }

        private static void RestoreSelection(DataGridView grid, string referenceNumber)
        {
            if (referenceNumber == null) return;
            foreach (DataGridViewRow row in grid.Rows)
            {
                var issue = row.DataBoundItem as IssueReport;
                if (issue != null && string.Equals(issue.ReferenceNumber, referenceNumber, StringComparison.OrdinalIgnoreCase))
                {
                    grid.CurrentCell = row.Cells[0];
                    row.Selected = true;
                    return;
                }
            }
        }
    }
}
