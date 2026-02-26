//using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS1.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IGenericRepository<Employee> _repository;

        public EmployeeService(IGenericRepository<Employee> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }


        public async Task<Employee> CreateAsync(CreateEmployeeDto dto)
        {
            try
            {
                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address
                };

                await _repository.AddAsync(employee);
                await _repository.SaveChangesAsync();

                return employee;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is PostgresException pgEx &&
                    pgEx.SqlState == "23505") // Unique violation
                {
                    throw new ApplicationException("Employee with this email already exists.");
                }

                throw;
            }
        }

        public async Task<Employee?> UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
                return null;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Address = dto.Address;

            _repository.Update(employee);
            await _repository.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
                return false;

            _repository.Delete(employee);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}