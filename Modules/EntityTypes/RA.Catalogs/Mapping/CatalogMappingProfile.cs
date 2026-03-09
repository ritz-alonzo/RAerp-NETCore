using AutoMapper;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Core.Models.PluginModels.Catalogs;
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
            CreateMap<CatalogModel, Catalog>();

            CreateMap<CatalogSetting, CatalogConfigureModel>();
            CreateMap<CatalogConfigureModel, CatalogSetting>();

            #endregion
        }
    }
}
