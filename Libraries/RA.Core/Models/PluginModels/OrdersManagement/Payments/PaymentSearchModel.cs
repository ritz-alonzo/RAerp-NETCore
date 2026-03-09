using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Payments
{
    public class PaymentSearchModel : BaseFormSearchModel
    {
        public PaymentSearchModel()
        {
            Items = new PaymentListModel();
            AvailablePaymentStatus = new List<SelectListItem>();
        }
        [DisplayName("Order No.")]
        public string SearchOrderNbr { get; set; }
        [DisplayName("Payment Ref No.")]
        public string SearchPaymentRefNbr { get; set; }
        [DisplayName("Payment Date")]
        public DateTime? SearchPaymentDate { get; set; }
        [DisplayName("Payment Status")]
        public int SearchPaymentStatusId { get; set; }
        public List<SelectListItem> AvailablePaymentStatus { get; set; }
        public PaymentListModel Items { get; set; }
    }
}
