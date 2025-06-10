using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Categories
{
    public class CategoryListModel : BaseListModel<CategoryModel>
    {
        public bool RedirectByCodeEnabled { get; set; }
    }
}
