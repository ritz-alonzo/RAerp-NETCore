using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Payments
{
    public class PaymentRequestDto : BaseEntity
    {
        public string FormNbr { get; set; }
        public string PaymentRefNbr { get; set; }
        public string Description { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
