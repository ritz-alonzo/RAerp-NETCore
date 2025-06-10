using RA.Core.Models.BaseModels;
using RA.Data.Data;
using RAerp.Security.AccessRights;

namespace RAerp.Models.AccessRightsControlModel
{
    public class AccessRightsModel : BaseAdminModel
    {
        public string Name { get; set; }
        public string Classification { get; set; }
        public AccessType AccessRecordType { get; set; }
        public int AccessRecordTypeId
        {
            get { return (int)AccessRecordType; }
            set { AccessRecordType = (AccessType)value; }
        }
        public bool HasAccess { get; set; }
    }
}
