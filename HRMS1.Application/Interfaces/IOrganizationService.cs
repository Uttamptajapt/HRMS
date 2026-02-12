using HRMS.Application.DTOs.Organization;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces.Services
{
    public interface IOrganizationService
    {
        Task<IEnumerable<Organization>> GetAllAsync();
        Task<Organization?> GetByIdAsync(Guid id);
        Task<Organization> CreateAsync(CreateOrganizationDto dto, string userId);
        Task<bool> DeleteAsync(Guid id);
    }
}
