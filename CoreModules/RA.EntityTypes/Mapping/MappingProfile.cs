using AutoMapper;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.EntityTypes;

namespace RA.EntityTypes.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Entity Types
            CreateMap<EntityType, EntityTypeModel>();
            CreateMap<EntityTypeModel, EntityType>();
            #endregion
        }
    }
}
