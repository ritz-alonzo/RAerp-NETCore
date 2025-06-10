using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.EntityTypes
{
    public class BaseEntityModel : BaseModel
    {
        public Guid EntityTypeId { get; set; }
        public string EntityTypeName { get; set; }
        public string EntityTypeSystemName { get; set; }
        [DisplayName("Code")]
        public string Code { get; set; }
        [DisplayName("Name")]
        public string Name { get; set; }
        [DisplayName("Description")]
        public string Description { get; set; }
        public Guid CreatedById { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }

        #region Settings
        
        public bool Enabled { get; set; }
        public bool AddressEnabled { get; set; }

        #endregion
    }
}
