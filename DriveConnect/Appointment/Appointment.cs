using System;
using System.Collections.Generic;
using System.Text;

namespace DriveConnect.Appointment
{
    using System;

namespace DriveConnect.Models
{
    public class Appointment
    {
        public int AppointmentID { get; private set; }
        public DateTime AppointmentDate { get; private set; }
        public TimeSpan AppointmentTime { get; private set; }
        public string Purpose { get; private set; }
        public string Status { get; private set; }

        public Appointment(
            int appointmentID,
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            string purpose,
            string status)
        {
            AppointmentID = appointmentID;
            AppointmentDate = appointmentDate;
            AppointmentTime = appointmentTime;
            Purpose = purpose;
            Status = status;
        }

        public void UpdateAppointment(
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            string purpose,
            string status)
        {
            AppointmentDate = appointmentDate;
            AppointmentTime = appointmentTime;
            Purpose = purpose;
            Status = status;
        }

        public void Cancel()
        {
            Status = "Cancelled";
        }
    }
}
}
