using SmartClass.Domaine.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Domain.Entities
{
    public class FileResource : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public Guid? ClassroomId { get; set; }
        public Guid? MaterialId { get; set; }
        public Guid? SubmissionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long SizeBytes { get; set; }
        public string BlobPath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
