using AutoMapper;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.DTO;
using RA.Categories.Domain;
using RA.Categories.DTO;
using RA.Core.Models.PluginModels.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Mapping
{
    public class BusinessEntityMappingProfile : Profile
    {
        public BusinessEntityMappingProfile()
        {
            #region BusinessEntity

            CreateMap<BusinessEntity, BusinessEntityModel>();
            CreateMap<BusinessEntityModel, BusinessEntity>();

            CreateMap<BusinessEntitySetting, BusinessEntityConfigureModel>();
            CreateMap<BusinessEntityConfigureModel, BusinessEntitySetting>();

            #endregion

            #region Business Entity Dto
            CreateMap<BusinessEntity, BusinessEntityResponseDto>();
            CreateMap<BusinessEntityRequestDto, BusinessEntity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
