using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Domain.EntityAttributes
{
    /// <summary>
    /// Entity Attribute for Combobox selection
    /// </summary>
    public class EntityAttributeOption : BaseEntity
    {
        public Guid AttributeId { get; set; }
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public bool IsDefault { get; set; }
    }
}
