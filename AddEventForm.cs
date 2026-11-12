using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalServicesApplication
{
    public class AddEventForm : Form
    {
        private readonly TextBox title = new TextBox { Dock = DockStyle.Fill, MaxLength = 150 };
        private readonly ComboBox category = new ComboBox { Dock = DockStyle.Fill };
        private readonly ComboBox kind = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker date = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        private readonly TextBox location = new TextBox { Dock = DockStyle.Fill, MaxLength = 250 };
        private readonly TextBox description = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 4000 };
        private readonly Label validation = AppTheme.Label("");
        public MunicipalEvent CreatedEvent { get; private set; }
        public AddEventForm(IEnumerable<string> categories)
        {
            Text = "Add Event / Announcement";
            ClientSize = new Size(620, 550);
            MinimumSize = new Size(550, 520);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 8 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            string[] labels = { "Title", "Type", "Category", "Date", "Location", "Description" };
            Control[] inputs = { title, kind, category, date, location, description };
            for (int i = 0; i < inputs.Length; i++)
            {
                layout.RowStyles.Add(new RowStyle(i == 5 ? SizeType.Percent : SizeType.AutoSize, i == 5 ? 100 : 0));
                layout.Controls.Add(AppTheme.Label(labels[i]), 0, i);
                inputs[i].Margin = new Padding(5, 8, 5, 8);
                inputs[i].AccessibleName = labels[i];
                layout.Controls.Add(inputs[i], 1, i);
            }
            kind.Items.AddRange(new object[] { "Event", "Announcement" });
            kind.SelectedIndex = 0;
            foreach (string item in categories) category.Items.Add(item);
            validation.ForeColor = Color.Firebrick;
            validation.MaximumSize = new Size(520, 0);
            layout.Controls.Add(validation, 0, 6);
            layout.SetColumnSpan(validation, 2);
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            var add = AppTheme.Button("Add", SaveEvent);
            var cancel = AppTheme.Button("Cancel", (s, e) => Close());
            cancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(add);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, 7);
            CancelButton = cancel;
            Controls.Add(layout);
            AppTheme.Apply(this);
        }
        private void SaveEvent(object sender, EventArgs e)
        {
            var item = new MunicipalEvent { EventName = title.Text.Trim(), Kind = kind.Text,
                Category = category.Text.Trim(), EventDate = date.Value.Date, Location = location.Text.Trim(), Description = description.Text.Trim() };
            string error = EventSearch.Validate(item);
            if (error != null) { validation.Text = error; return; }
            CreatedEvent = item;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
