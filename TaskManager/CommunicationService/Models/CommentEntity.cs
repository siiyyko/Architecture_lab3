using System.ComponentModel.DataAnnotations;

namespace CommunicationService.Models
{
    public class CommentEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public CommentEntity()
        {
            Id = Guid.NewGuid();
            Content = string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}