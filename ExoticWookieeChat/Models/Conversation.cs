using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExoticWookieeChat.Models
{
    [Table("conversations")]
    public class Conversation
    {
        public enum States : byte { New, InProgress, Closed };

        [Key]
        public int Id { get; set; }

        public Guid SocketGuid { get; set; }

        public byte State { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}