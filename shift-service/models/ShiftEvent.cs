// Data model for incoming events

namespace Models 
{
    public class ShiftEvent
        {
            public string? Action { get; set; }    // "create", "delete", or "update"
            public string? ShiftId { get; set; }
            public string? EmployeeId { get; set; }
            public string? StartTime { get; set; }
            public string? EndTime { get; set; }
            public string? RoleID {get;set;}
        }

}
        