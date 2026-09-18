using System;
using System.Collections.Generic;
using System.Text;

namespace DriveConnect.FollowUp
{
    using System;

    namespace DriveConnect.Models
    {
        public class FollowUp
        {
            public int FollowUpID { get; private set; }
            public DateTime FollowUpDate { get; private set; }
            public string Notes { get; private set; }
            public string Status { get; private set; }

            public FollowUp(
                int followUpID,
                DateTime followUpDate,
                string notes,
                string status)
            {
                FollowUpID = followUpID;
                FollowUpDate = followUpDate;
                Notes = notes;
                Status = status;
            }

            public void UpdateFollowUp(
                DateTime followUpDate,
                string notes,
                string status)
            {
                FollowUpDate = followUpDate;
                Notes = notes;
                Status = status;
            }
        }
    }
}
