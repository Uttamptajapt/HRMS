using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(Guid id);
        Task<Employee> CreateAsync(CreateEmployeeDto dto);
        Task<Employee?> UpdateAsync(Guid id, UpdateEmployeeDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
