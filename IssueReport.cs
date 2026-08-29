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

        public IssueReport()
        {
            ReferenceNumber = Guid.NewGuid()
                .ToString()
                .Substring(0, 8)
                .ToUpper();

            DateReported = DateTime.Now;
        }
    }
}