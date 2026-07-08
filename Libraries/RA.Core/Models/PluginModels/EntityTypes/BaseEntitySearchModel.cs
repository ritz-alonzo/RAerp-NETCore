using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.EntityTypes
{
    public class BaseEntitySearchModel : BaseSearchModel
    {
        public Guid SearchEntityTypeId { get; set; }
        public string EntityTypeSystemName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public bool ShowDeleted { get; set; }
        public string EntityTypeName { get; set; }
        public Guid? CreatedById { get; set; }
    }
}
