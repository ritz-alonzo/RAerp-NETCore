using RA.Core.Domain;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.DTO
{
    public class CatalogRequestDto : BaseEntity
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
        public decimal Price { get; set; }
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
