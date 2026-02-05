using System;

namespace HRMS.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public Guid OrganizationId { get; set; }

        public string CreatedByUserId { get; set; }
    }
}
