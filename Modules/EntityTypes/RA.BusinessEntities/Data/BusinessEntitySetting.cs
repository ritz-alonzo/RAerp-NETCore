using RA.EntityTypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Data
{
    public class BusinessEntitySetting : BaseEntityTypeSetting
    {
        public BusinessEntitySetting()
        {
            MappedCategoryIds = new List<Guid>();
        }
        public bool IsUserMappingEnabled { get; set; }
        public List<Guid> MappedCategoryIds { get; set; }
    }
}
