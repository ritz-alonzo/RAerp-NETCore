using AutoMapper;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.DTO;
using RA.Core.Models.AdminModels;
using RA.Core.Models.PluginModels.Catalogs;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Mapping
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            #region Catalog

            CreateMap<Catalog, CatalogModel>();
            CreateMap<CatalogModel, Catalog>()
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                // Safe fallback condition to skip null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CatalogSetting, CatalogConfigureModel>();
            CreateMap<CatalogConfigureModel, CatalogSetting>()
                // Safe fallback condition to skip null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // You must also explicitly map the inner items so AutoMapper knows how to transform them
            CreateMap<EntityAttributeValueModel, EntityAttributeValue>()
                .ReverseMap()
                // Safe fallback condition to skip null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region Catalog Dto

            CreateMap<Catalog, CatalogDetailResponseDto>();
            CreateMap<CatalogDetailResponseDto, Catalog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Catalog, CatalogResponseDto>();
            CreateMap<CatalogRequestDto, Catalog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            #endregion
        }
    }
}
