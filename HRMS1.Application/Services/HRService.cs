using HRMS.Application.DTOs.HR;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services
{
    public class HRService : IHRService
    {
        public async Task<IEnumerable<Employee>> GetAllHRsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Employee?> GetHRByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Employee> CreateHRAsync(CreateHRDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
