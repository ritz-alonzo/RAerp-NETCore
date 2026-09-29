using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Domain.FileManager
{
    public class FileEntityMapping : BaseEntity
    {
        public Guid EntityId { get; set; }
        public string EntitySystemName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
