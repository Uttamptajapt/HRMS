using HRMS.Application.DTOs.HR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces.Services
{
    public interface IHRService
    {
        Task<IEnumerable<Employee>> GetAllHRsAsync();
        Task<Employee?> GetHRByIdAsync(Guid id);
        Task<Employee> CreateHRAsync(CreateHRDto dto);
    }
}
