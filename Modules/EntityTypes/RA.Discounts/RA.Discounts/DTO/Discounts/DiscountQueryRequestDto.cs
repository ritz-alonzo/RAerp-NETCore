using RA.Core.Domain;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.DTO.Discounts
{
    public class DiscountQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public bool ShowDeleted { get; set; }
        public List<int> SearchDiscountTypeIds { get; set; } = new();
        public List<int> SearchDiscountScopeIds { get; set; } = new();
        public bool ShowActiveDiscountsOnly { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
