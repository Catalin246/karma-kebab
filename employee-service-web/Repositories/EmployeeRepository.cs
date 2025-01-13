using Azure;
using Azure.Data.Tables;
using Database;
using Interfaces;
using Models;
using Utility;

namespace Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ITableStorageService _tableStorageService;
        private const string TableName = "Employees"; // Table name constant

        public EmployeeRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            var employees = new List<Employee>();
            await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
            var tableClient = _tableStorageService.GetTableClient(TableName);

            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                // Use EmployeeMapper to map TableEntity to Employee
                var employee = EmployeeMapper.MapTableEntityToEmployee(entity);
                employees.Add(employee);
            }

            return employees;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
        {
            try
            {
                await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
                var entity = await _tableStorageService.GetEntityAsync(TableName, id.ToString(), id.ToString());

                // Use EmployeeMapper to map TableEntity to Employee
                return EmployeeMapper.MapTableEntityToEmployee(entity);
            }
            catch (RequestFailedException)
            {
                return null; // Handle "not found" gracefully
            }
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
        {
            var employees = new List<Employee>();
            await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
            var tableClient = _tableStorageService.GetTableClient(TableName);

            await foreach (var entity in tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{role}'"))
            {
                // Use EmployeeMapper to map TableEntity to Employee
                var employee = EmployeeMapper.MapTableEntityToEmployee(entity);
                employees.Add(employee);
            }

            return employees;
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
            var tableClient = _tableStorageService.GetTableClient(TableName);

            // Use EmployeeMapper to map Employee to TableEntity
            var entity = EmployeeMapper.MapEmployeeToTableEntity(employee);

            // Use UpsertEntityAsync for insert or update
            await tableClient.UpsertEntityAsync(entity); // Upsert the entity

            return employee; // Return the Employee object, which will be mapped to DTO in the service
        }
        

        public async Task<Employee?> UpdateEmployeeAsync(Employee updatedEmployee)
        {
            try
            {
                await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
                var tableClient = _tableStorageService.GetTableClient(TableName);

                // Use EmployeeMapper to map Employee to TableEntity
                var entity = EmployeeMapper.MapEmployeeToTableEntity(updatedEmployee);

                // Update the entity in the table
                await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);

                return updatedEmployee; // Return the updated Employee object
            }
            catch (RequestFailedException)
            {
                return null; // Handle failure appropriately
            }
        }


        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            try
            {
                await _tableStorageService.CreateTableIfNotExistsAsync(TableName); // Ensure the table exists
                var tableClient = _tableStorageService.GetTableClient(TableName);
                await tableClient.DeleteEntityAsync(id.ToString(), id.ToString());
                return true;
            }
            catch (RequestFailedException)
            {
                return false; // Handle deletion errors gracefully
            }
        }
    }
}
