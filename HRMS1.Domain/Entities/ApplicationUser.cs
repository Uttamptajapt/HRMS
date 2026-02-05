using Microsoft.AspNetCore.Identity;
using System;

namespace HRMS.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public Guid OrganizationId { get; set; }
    }
}
