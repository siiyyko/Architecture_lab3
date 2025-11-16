using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserEntity()
        {
            Id = Guid.NewGuid();
            Email = string.Empty;
            UserName = string.Empty;
            PasswordHash = string.Empty;
        }
    }
}