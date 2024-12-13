namespace Models 
{
    public class EventMessage
    {
        public required string EventID { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
        public int ShiftsNumber { get; set; }
    }
}