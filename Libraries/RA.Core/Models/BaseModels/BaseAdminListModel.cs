using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.BaseModels
{
    public class BaseAdminListModel<T> where T : BaseAdminModel
    {
        public BaseAdminListModel()
        {
            Items = new List<T>();
        }
        public List<T> Items { get; set; }
        public long TotalItems { get; set; }
        public long PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
