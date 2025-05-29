using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Data
{
    public class AccessToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string UserId { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = default!;
    }
}
