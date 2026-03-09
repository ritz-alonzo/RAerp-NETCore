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
using RA.Core.Models.PortableViewModels;
using RAerp.Helpers.PluginHelper;
using RAerp.PluginServiceProvider;
using RAerp.Helpers.UserHelper;

namespace RA.WebServiceEndpoints.Factories
{
    public class WebServiceEndpointModelFactory : IWebServiceEndpointModelFactory
    {
        #region Constants
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IBaseModelFactory _baseModelFactory;
        private readonly IMapper _mapper;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        #endregion

        #region Ctor
        public WebServiceEndpointModelFactory(IWebServiceEndpointService webServiceEndpointService,
            IBaseModelFactory baseModelFactory,
            IMapper mapper,
            IEntityTypeManager entityTypeManager,
            IUserIdentity userIdentity)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _baseModelFactory = baseModelFactory;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
            _userIdentity = userIdentity;
        }
        #endregion

        public virtual async Task<WebServiceEndpointSearchModel> PrepareWebServiceEndpointSearchModelAsync(WebServiceEndpointSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseModelFactory.PrepareBaseSearchModel(searchModel, pageSize, pageNumber);

            searchModel.WebServiceEndpoints = await PrepareWebServiceEndpointListModelAsync(searchModel);
            searchModel.WebServiceEndpointName = "Web Service Endpoints";

            // system name mapping for active menu
            searchModel.WebServiceEndpointSystemName = typeof(WebServiceEndpoint).FullName;

            searchModel.TotalItems = (int)searchModel.WebServiceEndpoints.TotalItems;
            searchModel.PageSize = searchModel.WebServiceEndpoints.PageSize;
            searchModel.CurrentItemsShown = searchModel.WebServiceEndpoints.Items.Count;

            return searchModel;
        }

        public virtual async Task<WebServiceEndpointListModel> PrepareWebServiceEndpointListModelAsync(WebServiceEndpointSearchModel searchModel)
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
                var createdByUser = _userIdentity.GetUserDetailsAsync(webServiceEndpoint.CreatedById).Result;
                if (createdByUser != null)
                    webServiceEndpointModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);
                // END
                webServiceEndpointModel.EndpointEntityTypeName = _entityTypeManager.GetByIdAsync(webServiceEndpoint.EndpointEntityTypeId.Value).Result.EntityName;

                return webServiceEndpointModel;

            }).ToList();

            _baseModelFactory.PrepareBaseListModel(model, webServiceEndpoints, searchModel, webServiceEndpointList.Count());

            return model;
        }

        public virtual async Task<WebServiceEndpointModel> PrepareWebServiceEndpointModelAsync(WebServiceEndpointModel model, WebServiceEndpoint webServiceEndpoint)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (webServiceEndpoint == null)
            {
                webServiceEndpoint = new WebServiceEndpoint();
                model.CreatedOn = DateTime.Now;
                model.IsMappingVisible = false;
            }
            else
            {
                model = _mapper.Map(webServiceEndpoint, model);
                if (webServiceEndpoint.CreatedById.IsNotNullOrEmpty())
                    model.CreatedByUser.Id = webServiceEndpoint.CreatedById;

                model.IsMappingVisible = true;
                // plugin view component of the current endpoint domain 
                var pluginAssemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();
                if (pluginAssemblies.Any())
                {
                    foreach (var assembly in pluginAssemblies)
                    {
                        var pluginPortableView = assembly.GetTypes()
                            .Where(t => t.GetInterfaces().Contains(typeof(IPluginViewComponent)))
                            .FirstOrDefault();

                        if (pluginPortableView != null)
                        {
                            var pluginInstance = Activator.CreateInstance(pluginPortableView) as IPluginViewComponent;

                            if (pluginInstance != null)
                            {
                                // Ex. Inventory (referencing BusinessEntities)
                                var viewComponentList = pluginInstance.ManagePluginViewComponent();
                                if (viewComponentList.Any())
                                {
                                    var pluginComponentList = viewComponentList.Where(c => c.TargetEntity == nameof(WebServiceEndpoint) && c.SourceEntity == model.EndpointDomain).ToList();
                                    model.PluginComponents = pluginComponentList.Any() ? pluginComponentList : new List<PluginViewComponentModel>();
                                }
                            }
                        }
                    }
                }
            }

            model.WebServiceEndpointSystemName = typeof(WebServiceEndpoint).FullName;

            await _baseModelFactory.PrepareBaseModelAsync<WebServiceEndpointModel>(model);

            // need to prepare list of Entity Types for Selection
            if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                model.EndpointEntityTypeId = webServiceEndpoint.EndpointEntityTypeId;

            model.AvailableEntityTypes = await _entityTypeManager.GetEntityTypesSelectListAsync();

            return model;
        }


    }
}
