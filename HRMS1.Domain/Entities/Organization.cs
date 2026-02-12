using System;
using System.Collections.Generic;

namespace HRMS.Domain.Entities
{
    public class Organization
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
        public string CreatedByUserId { get; set; }


    }
}
