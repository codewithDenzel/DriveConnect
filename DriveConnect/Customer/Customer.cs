using System;
using System.Collections.Generic;
using System.Text;

namespace DriveConnect.Customer
{
    using System;

    namespace DriveConnect.Models
    {
        public class Customer
        {
            public int CustomerID { get; private set; }
            public string Name { get; private set; }
            public string ContactNumber { get; private set; }
            public string Email { get; private set; }
            public string Address { get; private set; }

            public Customer(
                int customerID,
                string name,
                string contactNumber,
                string email,
                string address)
            {
                CustomerID = customerID;
                Name = name;
                ContactNumber = contactNumber;
                Email = email;
                Address = address;
            }

            public void UpdateCustomer(
                string name,
                string contactNumber,
                string email,
                string address)
            {
                Name = name;
                ContactNumber = contactNumber;
                Email = email;
                Address = address;
            }
        }
    }
}
