using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.PluginData.EntityTypes.Catalogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Catalogs
{
    public class CatalogModel : BaseEntityModel
    {
        public CatalogModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableCatalogTypes = new List<SelectListItem>();
        }

        [DisplayName("Type")]
        public int TypeId { get; set; }
        public CatalogType Type { get; set; }
        [DisplayName("Status")]
        public int StatusId { get; set; }
        public CatalogStatus Status { get; set; }
        [DisplayName("UOM")]
        public Guid? UOMId { get; set; }
        [DisplayName("Price")]
        public decimal? Price { get; set; }
        [DisplayName("Image File")]
        public IFormFile ImageFile { get; set; }
        [DisplayName("Image URL")]
        public string ImageUrl { get; set; }
        [DisplayName("Image")]
        public string ImagePath { get; set; }
        [DisplayName("Use Image URL")]
        public bool UseImageUrlEnabled { get; set; }
        public string SKU { get; set; }
        public string BarcodeValue { get; set; }
        public List<SelectListItem> AvailableCategories { get; set; }
        public List<SelectListItem> AvailableCatalogTypes { get; set; }
    }
}
