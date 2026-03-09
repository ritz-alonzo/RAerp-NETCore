using RA.EntityTypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Data
{
    public class CatalogSetting : BaseEntityTypeSetting
    {
        public CatalogSetting()
        {
            MappedCategoryIds = new List<Guid>();
        }
        public List<Guid> MappedCategoryIds { get; set; }
    }
}
