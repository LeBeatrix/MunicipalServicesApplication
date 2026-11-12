 using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MunicipalServicesApplication
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            btnServiceRequestStatus.Enabled = true;
            btnServiceRequestStatus.Click += btnServiceStatus_Click;
            AppTheme.Apply(this);
            MinimumSize = Size;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnReportIssues_Click(object sender, EventArgs e)
        {
            using (var reportForm = new ReportIssuesForm()) reportForm.ShowDialog(this);
        }

        private void btnLocalEvents_Click(object sender, EventArgs e)
        {
            using (var eventsForm = new LocalEventsForm()) eventsForm.ShowDialog(this);
        }

        private void btnServiceStatus_Click(object sender, EventArgs e)
        {
            using (var statusForm = new ServiceRequestStatusForm()) statusForm.ShowDialog(this);
        }
    }
}
