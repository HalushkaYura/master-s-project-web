using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChannelId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime? EditedAt { get; set; }
        public Guid? ParentMessageId { get; set; }
    }
}
