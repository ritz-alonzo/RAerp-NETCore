using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Core.Models.PluginModels.Catalogs;
using RA.EntityTypes.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Factories
{
    public class CatalogModelFactory : ICatalogModelFactory
    {
        private readonly IBaseEntityModelFactory _entityModelFactory;

        public CatalogModelFactory(IBaseEntityModelFactory entityModelFactory)
        {
            _entityModelFactory = entityModelFactory;
        }

        public virtual CatalogSearchModel PrepareCatalogSearchModel(CatalogSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //searchModel = _entityModelFactory.PrepareBaseEntitySearchModel(searchModel, pageSize, pageNumber);

            return searchModel;
        }

        public virtual CatalogModel PrepareCatalogModel(CatalogModel catalogModel)
        {
            if (catalogModel == null)
                throw new ArgumentNullException(nameof(catalogModel));

            //catalogModel = _entityModelFactory.PrepareBaseEntityModel<CatalogModel, CatalogSetting>(catalogModel);

            return catalogModel;
        }

        public virtual CatalogConfigureModel PrepareCatalogConfigureModel(Guid entityTypeId, string systemName)
        {
            var catalogConfigureModel = new CatalogConfigureModel();

            catalogConfigureModel = _entityModelFactory.PrepareBaseEntityConfigureModel<CatalogConfigureModel, Catalog, CatalogSetting>(catalogConfigureModel, entityTypeId, systemName);

            return catalogConfigureModel;
        }
    }
}
