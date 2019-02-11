using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExoticWookieeChat.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MinLength(5, ErrorMessage = "Display name must be at least 5 characters")]
        [Display(Name = "Display name")]
        public String DisplayName { get; set; }

        [Required, MinLength(5, ErrorMessage = "User name must be at least 5 characters")]
        [Display(Name = "User name")]
        public String UserName { get; set; }

        [Required, MinLength(5, ErrorMessage = "User name must be at least 5 characters")]
        public String Password { get; set; }

        [NotMapped]
        [Display(Name = "Confirm password")]
        public String ConfirmPassword { get; set; }

        public DateTime CreatedAt { get; set; }

        public String Role { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}