using System.ComponentModel.DataAnnotations;

namespace CommunicationService.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}