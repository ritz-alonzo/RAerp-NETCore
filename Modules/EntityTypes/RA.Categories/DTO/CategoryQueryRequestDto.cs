using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.DTO
{
    public class CategoryQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
