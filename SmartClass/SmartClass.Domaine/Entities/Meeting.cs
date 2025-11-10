using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Domain.Entities
{
    public class Meeting : BaseEntity
    {
        public Guid? ClassroomId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public Guid CreatedBy { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public string? RecordingUrl { get; set; }
    }
}
