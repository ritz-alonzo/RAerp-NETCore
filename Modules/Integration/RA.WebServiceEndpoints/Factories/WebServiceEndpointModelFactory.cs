using AutoMapper;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Helpers;
using RA.Core.Models.PluginModels.WebServiceEndpoints;
using RA.WebServiceEndpoints.Services;
using RAerp.Factories.CoreFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Factories
{
    public class WebServiceEndpointModelFactory : IWebServiceEndpointModelFactory
    {
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IBaseModelFactory _baseModelFactory;
        private readonly IMapper _mapper;
        private readonly IEntityTypeManager _entityTypeManager;

        public WebServiceEndpointModelFactory(IWebServiceEndpointService webServiceEndpointService,
            IBaseModelFactory baseModelFactory,
            IMapper mapper,
            IEntityTypeManager entityTypeManager)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _baseModelFactory = baseModelFactory;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
        }

        public virtual async Task<WebServiceEndpointSearchModel> PrepareWebServiceEndpointSearchModel(WebServiceEndpointSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseModelFactory.PrepareBaseSearchModel(searchModel, pageSize, pageNumber);

            searchModel.WebServiceEndpoints = await PrepareWebServiceEndpointListModel(searchModel);
            searchModel.WebServiceEndpointName = "Web Service Endpoints";

            // system name mapping for active menu
            searchModel.WebServiceEndpointSystemName = typeof(WebServiceEndpoint).FullName;

            searchModel.TotalItems = (int)searchModel.WebServiceEndpoints.TotalItems;
            searchModel.PageSize = searchModel.WebServiceEndpoints.PageSize;
            searchModel.CurrentItemsShown = searchModel.WebServiceEndpoints.Items.Count;

            return searchModel;
        }

        public virtual async Task<WebServiceEndpointListModel> PrepareWebServiceEndpointListModel(WebServiceEndpointSearchModel searchModel)
        {
            var model = new WebServiceEndpointListModel();

            var webServiceEndpointList = await _webServiceEndpointService.GetList(
                searchQuery: searchModel.SearchQuery,
                createdOn: searchModel.SearchCreatedOn
                );

            var webServiceEndpoints = new List<WebServiceEndpointModel>();

            webServiceEndpoints = webServiceEndpointList.Select(webServiceEndpoint =>
            {
                var webServiceEndpointModel = new WebServiceEndpointModel();
                webServiceEndpointModel = _mapper.Map(webServiceEndpoint, webServiceEndpointModel);
                // will need to add check user, to set user data
                //var createdByUser = _userIdentity.GetUserDetails(businesEntity.CreatedById);
                //if (createdByUser != null)
                //    businessEntityModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);
                // END
                webServiceEndpointModel.EndpointEntityTypeName = _entityTypeManager.GetById(webServiceEndpoint.EndpointEntityTypeId.Value).Result.EntityName;

                return webServiceEndpointModel;

            }).ToList();

            _baseModelFactory.PrepareBaseListModel(model, webServiceEndpoints, searchModel, webServiceEndpointList.Count());

            return model;
        }

        public virtual async Task<WebServiceEndpointModel> PrepareWebServiceEndpointModel(WebServiceEndpointModel model, WebServiceEndpoint webServiceEndpoint)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (webServiceEndpoint == null)
            {
                webServiceEndpoint = new WebServiceEndpoint();
                model.CreatedOn = DateTime.Now;
            }
            else
            {
                model = _mapper.Map(webServiceEndpoint, model);
                if (webServiceEndpoint.CreatedById.IsNotNullOrEmpty())
                    model.CreatedByUser.Id = webServiceEndpoint.CreatedById;
            }

            model.WebServiceEndpointSystemName = typeof(WebServiceEndpoint).FullName;

            _baseModelFactory.PrepareBaseModel<WebServiceEndpointModel>(model);

            // need to prepare list of Entity Types for Selection
            if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
            {
                model.EndpointEntityTypeId = webServiceEndpoint.EndpointEntityTypeId;
            }

            model.AvailableEntityTypes = await _entityTypeManager.GetEntityTypesSelectList();

            return model;
        }


    }
}
