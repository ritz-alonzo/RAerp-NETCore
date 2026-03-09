using RA.Core.Models.PluginModels.FormTypes;
using RA.Core.PluginData.FormTypes.OrdersManagement.Payments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Payments
{
    public class PaymentModel : BaseFormModel
    {
        public PaymentModel()
        {
            Items = new PaymentItemListModel();
        }
        [DisplayName("Order No.")]
        public Guid OrderId { get; set; }
        [DisplayName("Payment Ref No.")]
        public string PaymentRefNbr { get; set; }
        [DisplayName("Payment Status")]
        public int PaymentStatusId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        [DisplayName("Payment Date")]
        public DateTime PaymentDate { get; set; }
        [DisplayName("Total Qty")]
        public decimal TotalQty { get; set; }
        [DisplayName("Total Discount Amount")]
        public decimal TotalDiscountAmount { get; set; }
        [DisplayName("VAT Amount")]
        public decimal TotalVatAmount { get; set; }
        [DisplayName("Total Gross Amount")]
        public decimal TotalGrossAmount { get; set; }
        [DisplayName("Total Amount")]
        public decimal TotalNetAmount { get; set; }
        [DisplayName("Amount Paid")]
        public decimal AmountPaid { get; set; }
        [DisplayName("Change Amount")]
        public decimal ChangeAmount { get; set; }
        public PaymentItemListModel Items { get; set; }
    }
}
