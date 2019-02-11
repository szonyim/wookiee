using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExoticWookieeChat.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        public int Id { get; set;  }

        public String Content { get; set; }

        public String Sender { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public virtual Conversation Conversation { get; set; }

        public virtual User Employee { get; set; }
    }
}