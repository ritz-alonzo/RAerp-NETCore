using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Categories
{
    public class CategorySearchModel : BaseEntitySearchModel
    {
        public CategorySearchModel() 
        {
            Categories = new CategoryListModel();
        }

        public CategoryListModel Categories { get; set; }
    }
}
