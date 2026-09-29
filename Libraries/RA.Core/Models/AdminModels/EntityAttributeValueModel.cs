using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.AdminModels
{
    public class EntityAttributeValueModel
    {
        public Guid Id { get; set; }
        public Guid AttributeId { get; set; }
        //public Guid EntityId { get; set; }
        //public string AttributeName { get; set; }
        public string Value { get; set; }
    }
}
