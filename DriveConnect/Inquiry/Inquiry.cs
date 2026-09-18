using System;
using System.Collections.Generic;
using System.Text;

namespace DriveConnect.Inquiry
{
    using System;

    namespace DriveConnect.Models
    {
        public class Inquiry
        {
            public int InquiryID { get; private set; }
            public DateTime InquiryDate { get; private set; }
            public string Details { get; private set; }
            public string VehicleInterest { get; private set; }

            public Inquiry(
                int inquiryID,
                DateTime inquiryDate,
                string details,
                string vehicleInterest)
            {
                InquiryID = inquiryID;
                InquiryDate = inquiryDate;
                Details = details;
                VehicleInterest = vehicleInterest;
            }

            public void UpdateInquiry(
                DateTime inquiryDate,
                string details,
                string vehicleInterest)
            {
                InquiryDate = inquiryDate;
                Details = details;
                VehicleInterest = vehicleInterest;
            }
        }
    }
}
