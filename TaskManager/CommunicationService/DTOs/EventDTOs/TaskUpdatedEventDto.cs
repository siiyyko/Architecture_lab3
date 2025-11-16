namespace CommunicationService.EventDTOs
{
    public class TaskUpdatedEventDto
    {
        public Guid TaskId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}