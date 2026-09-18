using System;
using System.Collections.Generic;
using System.Text;

namespace DriveConnect.Report
{
    using System;

    namespace DriveConnect.Models
    {
        public class Report
        {
            public int ReportID { get; private set; }
            public string ReportType { get; private set; }
            public DateTime DateGenerated { get; private set; }
            public string ReportData { get; private set; }

            public Report(
                int reportID,
                string reportType,
                DateTime dateGenerated,
                string reportData)
            {
                ReportID = reportID;
                ReportType = reportType;
                DateGenerated = dateGenerated;
                ReportData = reportData;
            }

            public void UpdateReport(
                string reportType,
                string reportData)
            {
                ReportType = reportType;
                ReportData = reportData;
            }
        }
    }
}
