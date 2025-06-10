using AutoMapper;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
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
        }
    }
}
