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
    public partial class ReportIssuesForm : Form
    {
        private List<IssueReport> issueReports =
            new List<IssueReport>();

        private string selectedAttachmentPath = "";
        public ReportIssuesForm()
        {
            InitializeComponent();

            cmbCategory.Items.Add("Sanitation");
            cmbCategory.Items.Add("Roads");
            cmbCategory.Items.Add("Water");
            cmbCategory.Items.Add("Electricity");
            cmbCategory.Items.Add("Waste Management");
            cmbCategory.Items.Add("Other");

            cmbCategory.SelectedIndex = 0;
        }

        private void btnAttachFile_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Title = "Select an Image or Document";

                openFileDialog1.Filter =
                    "Supported Files|*.jpg;*.jpeg;*.png;*.pdf;*.doc;*.docx";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    selectedAttachmentPath = openFileDialog1.FileName;

                    lblAttachment.Text =
                        openFileDialog1.SafeFileName;


                    UpdateProgress();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "The file could not be attached. Please try again.",
                    "Attachment Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateProgress()
        {
            int progress = 0;

            if (!string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                progress += 25;
            }

            if (cmbCategory.SelectedIndex >= 0)
            {
                progress += 25;
            }

            if (!string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                progress += 25;
            }

            if (!string.IsNullOrEmpty(selectedAttachmentPath))
            {
                progress += 25;
            }

            progressBarReport.Value = progress;

            if (progress == 0)
            {
                lblEngagementMessage.Text =
                    "Start by telling us about the issue.";
            }
            else if (progress <= 50)
            {
                lblEngagementMessage.Text =
                    "Great! You are making progress.";
            }
            else if (progress < 100)
            {
                lblEngagementMessage.Text =
                    "Almost done. Your report can make a difference!";
            }
            else
            {
                lblEngagementMessage.Text =
                    "Excellent! Your report is ready to submit.";
            }
        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        private void rtbDescription_TextChanged(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show(
                    "Please enter the location of the issue.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtLocation.Focus();
                return;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select an issue category.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                MessageBox.Show(
                    "Please provide a description of the issue.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                rtbDescription.Focus();
                return;
            }

            IssueReport newReport = new IssueReport
            {
                Location = txtLocation.Text.Trim(),

                Category = cmbCategory.SelectedItem.ToString(),

                Description = rtbDescription.Text.Trim(),

                AttachmentPath = selectedAttachmentPath
            };

            issueReports.Add(newReport);

            MessageBox.Show(
                "Thank you for helping improve your community!"
                + Environment.NewLine
                + Environment.NewLine
                + "Your issue has been submitted successfully."
                + Environment.NewLine
                + "Reference Number: "
                + newReport.ReferenceNumber,
                "Report Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ClearForm();
        }

        private void ClearForm()
        {
            txtLocation.Clear();

            cmbCategory.SelectedIndex = 0;

            rtbDescription.Clear();

            selectedAttachmentPath = "";

            lblAttachment.Text = "No file selected";

            progressBarReport.Value = 0;

            lblEngagementMessage.Text =
                "Every report helps improve our community!";
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            MainMenuForm mainMenu = new MainMenuForm();

            mainMenu.Show();

            this.Close();
        }
    }
    }