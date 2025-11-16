using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Required]
        public Guid ReporterId { get; set; }
        public Guid? AssigneeId { get; set; }

        public int Priority { get; set; }
        public int Status { get; set; }
    }
}