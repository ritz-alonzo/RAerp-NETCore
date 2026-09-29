using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.DTO
{
    public class BusinessEntityQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public List<int> SearchStatusIds { get; set; } = new List<int>();
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
