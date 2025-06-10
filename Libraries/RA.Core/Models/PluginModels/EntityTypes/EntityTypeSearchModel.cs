using RA.Core.Models.BaseModels;
using System;

namespace RA.Core.Models.PluginModels.EntityTypes
{
    public class EntityTypeSearchModel : BaseSearchModel
    {
        public EntityTypeSearchModel()
        {
            EntityTypes = new EntityTypeListModel();
        }
        public string EntityTypeSystemName { get; set; }
        public string EntityTypeName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchInstalledOn { get; set; }
        public Guid? SearchParentEntityTypeId { get; set; }
        public bool ChildEntitySearchEnabled { get; set; }
        public EntityTypeListModel EntityTypes { get; set; }
    }
}
