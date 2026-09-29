using RA.Core.PluginData.FormTypes.OrdersManagement.Payments;
using RA.FormTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Domain.Payments
{
    public class Payment : BaseForm
    {
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PaymentRefNbr { get; set; }
        public int PaymentStatusId { get; set; }
        [NotMapped]
        public PaymentStatus PaymentStatus
        {
            get { return (PaymentStatus)PaymentStatusId; }
            set { PaymentStatusId = (int)value; }
        }
        public Guid PaymentMethodId { get; set; }
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
