using System.Drawing;
using System.Windows.Forms;

namespace MunicipalServicesApplication
{
    internal static class AppTheme
    {
        internal static readonly Color Teal = Color.FromArgb(23, 75, 80);
        internal static void Apply(Form form)
        {
            form.BackColor = Color.FromArgb(246, 249, 250);
            form.StartPosition = FormStartPosition.CenterParent;
            Style(form.Controls);
        }
        private static void Style(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (!(control is Label)) control.Font = new Font("Segoe UI", 10F);
                var button = control as Button;
                if (button != null)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = Teal;
                    button.ForeColor = Color.White;
                    button.Cursor = Cursors.Hand;
                    button.AutoSize = true;
                    button.MinimumSize = new Size(110, 36);
                }
                var grid = control as DataGridView;
                if (grid != null)
                {
                    grid.BackgroundColor = Color.White;
                    grid.BorderStyle = BorderStyle.None;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Teal;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.ColumnHeadersHeight = 36;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 244, 244);
                    grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(49, 110, 116);
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.RowTemplate.Height = 32;
                }
                Style(control.Controls);
            }
        }
        internal static Button Button(string text, System.EventHandler click)
        {
            var button = new Button { Text = text, AutoSize = true, Margin = new Padding(5) };
            button.Click += click;
            return button;
        }
        internal static Label Label(string text)
        {
            return new Label { Text = text, AutoSize = true, Margin = new Padding(6, 12, 6, 6) };
        }
    }
}
