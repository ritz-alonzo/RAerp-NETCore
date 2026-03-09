using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Domain
{
    public class BaseFormItem : BaseEntity
    {
        /// <summary>
        /// Represents the related FormId of this Form Item
        /// </summary>
        public Guid FormId { get; set; }
        /// <summary>
        /// Represents as Catalog.
        /// </summary>
        public Guid CatalogId { get; set; }
        public int LineNbr { get; set; }
        public Guid? CategoryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
