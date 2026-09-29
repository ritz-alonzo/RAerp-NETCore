using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.DTO
{
    public class CatalogQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public List<int> SearchStatusIds { get; set; } = new List<int>();
        public List<Guid> SearchCategoryTypeIds { get; set; } = new List<Guid>();
        public bool ShowDeleted { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
