using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RA.Core.Models.PluginModels.EntityTypes
{
    public class EntityTypeModel : BaseModel
    {
        public Guid? ParentEntityTypeId { get; set; }
        public string EntityName { get; set; }
        public string EntitySystemName { get; set; }
        public DateTime InstalledOn { get; set; }
        public DateTime? UnInstalledOn { get; set; }
        public string PluginController { get; set; }
        public string PluginConfigurationUrl { get; set; }
        public bool Deleted { get; set; }
        public bool Installed { get; set; }
        public bool ModalConfigureEnabled { get; set; }

    }
}
