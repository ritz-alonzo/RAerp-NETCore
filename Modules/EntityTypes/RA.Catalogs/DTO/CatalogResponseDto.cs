using RA.Core.Domain;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.DTO
{
    /// <summary>
    /// Response Dto for Catalog API
    /// </summary>
    public class CatalogResponseDto : BaseEntity
    {
        public Guid EntityTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TypeId { get; set; }
        public Guid? UOMId { get; set; }
        public int StatusId { get; set; }
        public string SKU { get; set; }
        public string BarcodeValue { get; set; }
        public string ImagePath { get; set; }
        public decimal Price { get; set; }
        // For Audit Purposes
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
