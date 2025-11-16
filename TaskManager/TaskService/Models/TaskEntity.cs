using System.ComponentModel.DataAnnotations;

namespace TaskService.Models
{
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        InReview,
        Done
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public class TaskEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string CodeName { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public Guid ReporterId { get; set; }

        public Guid? AssigneeId { get; set; }

        public TaskStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

        public TaskEntity()
        {
            Id = Guid.NewGuid();

            CodeName = string.Empty;
            Name = string.Empty;
            Created = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            Status = TaskStatus.ToDo;
            Priority = TaskPriority.Medium;
        }
    }
}