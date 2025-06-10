using RA.BusinessEntities.App_Data;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.EntityTypes.Services;

namespace RA.BusinessEntities.Services
{
    public interface IBusinessEntityService : IEntityTypeService<BusinessEntity, BusinessEntitySetting, RABusinessEntityContext>
    {
        void Test();
    }
}