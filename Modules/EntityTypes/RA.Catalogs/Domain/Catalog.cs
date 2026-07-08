using RA.Catalogs.Data;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.EntityTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Domain
{
    public class Catalog : BaseEntityType
    {
        public int TypeId { get; set; }
        [NotMapped]
        public CatalogType Type
        {
            get { return (CatalogType)TypeId; }
            set { TypeId = (int)value; }
        }
        // From Categories module
        public Guid? UOMId { get; set; }
        public int StatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public CatalogStatus Status
        {
            get { return (CatalogStatus)StatusId; }
            set { StatusId = (int)value; }
        }
        public string SKU { get; set; }
        public string ImagePath { get; set; }
        public decimal Price { get; set; }
    }
}
