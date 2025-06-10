using RA.Core.Models.BaseModels;

namespace RAerp.Models.AccessRightsControlModel
{
    public class AccessRightsSearchModel : BaseSearchModel
    {
        public AccessRightsSearchModel()
        {
            AccessRightsList = new AccessRightsListModel();
        }
        public string SystemName { get; set; }
        public string SearchRoleName { get; set; }
        public AccessRightsListModel AccessRightsList { get; set; }
    }
}
