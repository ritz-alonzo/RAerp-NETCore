using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.PluginData.EntityTypes.Categories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Categories
{
    public class CategoryModel : BaseEntityModel
    {
        public CategoryStatus Status { get; set; }
        public string StatusString { get; set; }
        /// <summary>
        /// CategoryStatus id of the entity
        /// </summary>
        /// 
        [DisplayName("Status")]
        public int StatusId
        {
            get { return (int)Status; }
            set { Status = (CategoryStatus)value; }
        }
    }
}
