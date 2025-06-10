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
        // For reference - to be added to model
        //[NopResourceDisplayName("Common.Operations.ServiceBayModel.Fields.ServiceBayStatusId")]
        public BusinessEntityStatus Status { get; set; }
        public string StatusString { get; set; }
        /// <summary>
        /// BusinessEntityStatus id of the entity
        /// </summary>
        /// 
        [DisplayName("Status")]
        public int StatusId
        {
            get { return (int)Status; }
            set { Status = (BusinessEntityStatus)value; }
        }
        [DisplayName("Category")]
        public Guid? CategoryId { get; set; }
        public List<SelectListItem> AvailableCategories { get; set; }

        // Address
        public AddressOverviewModel Address { get; set; }
    }
}
