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
    public class BusinessEntityResponseDto : BaseEntity
    {
        public Guid EntityTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AddressId { get; set; }
        public Guid? UserId { get; set; }
        // For Audit Purposes
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public AddressOverviewModel Address { get; set; } = new AddressOverviewModel();
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
