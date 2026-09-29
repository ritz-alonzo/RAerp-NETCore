using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Carts
{
    public class CartQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public List<Guid> SearchServiceIds { get; set; } = new();
        public Guid? SearchCustomerId { get; set; }
        public string SearchCustomerName { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public List<int> SearchStatusIds { get; set; } = new();
        public bool ShowDeleted { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
