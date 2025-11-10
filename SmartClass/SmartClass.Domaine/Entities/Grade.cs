using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Domain.Entities
{
    public class Grade : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public Guid GradedBy { get; set; }
        public decimal Score { get; set; }
        public DateTime GradedAt { get; set; } = DateTime.UtcNow;
        public string? FeedbackHtml { get; set; }
    }
}
