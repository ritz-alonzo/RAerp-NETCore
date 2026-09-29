using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.DTO
{
    public class CatalogDetailResponseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public int AvailableStock { get; set; }
        public bool IsLowStock { get; set; }
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
