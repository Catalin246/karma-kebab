using Azure;
using Azure.Data.Tables;

namespace Models
{
    public class Employee : ITableEntity
    {
        // Basic properties for the employee
        public Guid EmployeeId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? Address { get; set; }
        public decimal? Payrate { get; set; }
        public List<EmployeeRole> Roles { get; set; }
        public string? Email { get; set; }
        public List<Skill> Skills { get; set; } = new();

        // ITableEntity specific properties
        public string PartitionKey { get; set; } = default!; // Role as PartitionKey
        public string RowKey { get; set; } = default!; // EmployeeId as RowKey
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        ETag ITableEntity.ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Employee(){ }

        // Constructor to initialize the entity
        public Employee(Guid employeeId, EmployeeRole role)
        {
            EmployeeId = employeeId;
            RowKey = employeeId.ToString(); // EmployeeId as RowKey
            PartitionKey = role.ToString(); // Role as PartitionKey
        }
    }
}
