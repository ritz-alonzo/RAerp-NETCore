using RA.Core.Domain;
using RA.WebFramework.Data.DataTables;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAerp.DTO.EntityAttributes
{
    public class EntityAttributeResponseDto : BaseEntity
    {
        public string AttributeName { get; set; }
        public string AttributeDescription { get; set; }
        public string SystemName { get; set; }
        public int ControlTypeId { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string RegexPattern { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
