using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class BaseFormItemModel : BaseModel
    {
        public BaseFormItemModel()
        {
            AvailableCatalogs = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
        }
        public Guid FormId { get; set; }
        [DisplayName("Item")]
        public Guid? CatalogId { get; set; }
        [DisplayName("Item Code")]
        public string CatalogCode { get; set; }
        [DisplayName("Line No.")]
        public int LineNbr { get; set; }
        [DisplayName("Category")]
        public Guid? CategoryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
        public List<SelectListItem> AvailableCatalogs { get; set; }
        public List<SelectListItem> AvailableCategories { get; set; }
    }
}
