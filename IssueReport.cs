using System;

namespace MunicipalServicesApplication
{
    public class IssueReport
    {
        public string ReferenceNumber { get; set; }

        public string Location { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public string AttachmentPath { get; set; }

        public DateTime DateReported { get; set; }

        public string Status { get; set; } = RequestStatuses.Pending;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public IssueReport()
        {
            ReferenceNumber = Guid.NewGuid()
                .ToString("N")
                .ToUpperInvariant();

            DateReported = DateTime.Now;
        }
    }
}
