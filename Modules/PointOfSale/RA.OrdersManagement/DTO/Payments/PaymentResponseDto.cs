using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Payments
{
    public class PaymentResponseDto : BaseEntity
    {
        public string FormNbr { get; set; }
        public int StatusId { get; set; }
        public string Description { get; set; }
        public string PaymentRefNbr { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int PaymentStatusId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
    }
}
