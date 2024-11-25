using System;
public class EventDto
{
    public Guid EventId { get; set; } // Unique identifier for the event
    public DateTime StartTime { get; set; } // Start time of the event
    public DateTime EndTime { get; set; }   // End time of the event
}
