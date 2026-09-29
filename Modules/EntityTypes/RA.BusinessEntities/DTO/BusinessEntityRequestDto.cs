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
    public class BusinessEntityRequestDto : BaseEntity
    {
        public Guid EntityTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? UserId { get; set; }
        public AddressOverviewModel Address { get; set; } = new AddressOverviewModel();
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
