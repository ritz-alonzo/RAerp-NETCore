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
        public bool IsImageEnabled { get; set; }
        public string SKUTemplate { get; set; } // Count will be based on Template Count
        public string BarcodeTemplate { get; set; } // Count will be based on Template Count
        public bool IsSKUEnabled { get; set; }
        public bool IsBarcodeEnabled { get; set; }
        public List<Guid> MappedCategoryIds { get; set; }
    }
}
