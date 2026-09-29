using RA.Core.Domain;
using RA.WebFramework.Data.DataTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Domain.EntityAttributes
{
    /// <summary>
    /// Entity Attribute for fields (Textbox, Number, DateTime etc.) that hardcoded values are needed.
    /// </summary>
    public class EntityAttribute : BaseAdminEntity
    {
        public string AttributeName { get; set; }
        public string AttributeDescription { get; set; }
        public string SystemName { get; set; }
        public int ControlTypeId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public ControlType ControlType
        {
            get { return (ControlType)ControlTypeId; }
            set { ControlTypeId = (int)value; }
        }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string RegexPattern { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
