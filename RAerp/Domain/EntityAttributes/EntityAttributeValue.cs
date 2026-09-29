using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Domain.EntityAttributes
{
    /// <summary>
    /// Mapping of values of Entity (Entities and Forms) and Entity Attribute
    /// </summary>
    public class EntityAttributeValue : BaseEntity
    {
        public Guid AttributeId { get; set; }
        public Guid EntityId { get; set; }
        public string Value { get; set; }
    }
}
