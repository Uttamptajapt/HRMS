
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Employee> CreateAsync(CreateEmployeeDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<Employee?> UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
