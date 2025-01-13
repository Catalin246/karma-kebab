namespace employee_service_web.DTOs;

public class DeleteEmployeeRequest
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;

}
