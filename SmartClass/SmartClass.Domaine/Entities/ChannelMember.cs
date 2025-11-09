using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Domain.Entities
{
    public class ChannelMember : BaseEntity
    {
        public Guid ChannelId { get; set; }
        public Guid UserId { get; set; }
        public bool IsMuted { get; set; }
    }
}
