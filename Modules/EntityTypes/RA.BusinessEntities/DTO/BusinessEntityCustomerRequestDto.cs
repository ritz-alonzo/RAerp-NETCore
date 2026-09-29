using RA.Core.Domain;
using RA.Core.Models.OverviewModels;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.DTO
{
    public class BusinessEntityCustomerRequestDto : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public AddressOverviewModel Address { get; set; } = new AddressOverviewModel();
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
