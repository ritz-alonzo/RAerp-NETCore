using RA.Core.Models.UserInfaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.BaseModels
{
    public class BaseListModel<T> where T : BaseModel
    {
        public BaseListModel()
        {
            Items = new List<T>();
            UserInterface = new UserInterfaceAccessModel();
        }
        public List<T> Items { get; set; }
        public long TotalItems { get; set; }
        public long PageNumber { get; set; }
        public int PageSize { get; set; }
        public UserInterfaceAccessModel UserInterface { get; set; }
    }
}
