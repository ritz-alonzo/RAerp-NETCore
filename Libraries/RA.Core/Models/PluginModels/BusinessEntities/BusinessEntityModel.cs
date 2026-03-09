using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.OverviewModels;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.PluginData.EntityTypes.BusinessEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.BusinessEntities
{
    public class BusinessEntityModel : BaseEntityModel
    {
        public BusinessEntityModel()
        {
            AvailableCategories = new List<SelectListItem>();
            Address = new AddressOverviewModel();
        }
        [DisplayName("Status")]
        public int StatusId
        {
            get { return (int)Status; }
            set { Status = (BusinessEntityStatus)value; }
        }
        public BusinessEntityStatus Status { get; set; }
        [DisplayName("Category")]
        public Guid? CategoryId { get; set; }
        public List<SelectListItem> AvailableCategories { get; set; }
        // Address
        public AddressOverviewModel Address { get; set; }
    }
}
