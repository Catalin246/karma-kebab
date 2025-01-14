namespace Models 
{
    public class EventMessage
    {
        public required string RowKey { get; set; } // EventID
        public required string PartitionKey { get; set; } 
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
        public required List<int> RoleIds { get; set; }
    }
}