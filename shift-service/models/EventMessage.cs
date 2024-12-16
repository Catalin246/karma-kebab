namespace Models 
{
    public class EventMessage //this gon have a list of roles - length is number of shifts.
    {
        public required string EventID { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
        public int ShiftsNumber { get; set; }
    }
}