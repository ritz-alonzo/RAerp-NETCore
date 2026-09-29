using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Payments
{
    public class PaymentQueryRequestDto
    {
        public string SearchQuery { get; set; }
        public string SearchPaymentRefNbr { get; set; }
        public string SearchPaymentOrderNbr { get; set; }
        public List<Guid> SearchServiceIds { get; set; } = new();
        public Guid? SearchCustomerId { get; set; }
        public string SearchCustomerName { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public DateTime? SearchPaymentDate { get; set; }
        public List<int> SearchStatusIds { get; set; } = new();
        public List<int> SearchPaymentStatusIds { get; set; } = new();
        public bool ShowDeleted { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
