using AutoMapper;
using Newtonsoft.Json;
using RA.WebFramework.Extensions;
using RAerp.Domain.AccessRightControl;
using RAerp.Factories.CoreFactories;
using RAerp.Helpers.PluginHelper;
using RAerp.Models.AccessRightsControlModel;
using RAerp.Models.UsersModel;
using RAerp.Security.AccessRights;
using RAerp.Services.AccessRightsServices;
using RAerp.Services.UserServices;
using System.Reflection;

namespace RAerp.Factories.AccessRightsFactory
{
    public class AccessRightsModelFactory : IAccessRightsModelFactory
    {
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IAccessRightsService _accessRightsService;

        public AccessRightsModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IAccessRightsService accessRightsService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _accessRightsService = accessRightsService;
        }

        public virtual async Task<AccessRightsSearchModel> PrepareAccessRightsSearchModel(AccessRightsSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareBaseAdminSearchModel(searchModel, pageSize, pageNumber);

            searchModel.SystemName = typeof(AccessRights).FullName;
            searchModel.AccessRightsList = await PrepareAccessRightsListModel(searchModel);
            searchModel.TotalItems = (int)searchModel.AccessRightsList.TotalItems;
            searchModel.PageSize = searchModel.AccessRightsList.PageSize;

            return searchModel;
        }

        public virtual async Task<AccessRightsListModel> PrepareAccessRightsListModel(AccessRightsSearchModel searchModel)
        {
            var listModel = new AccessRightsListModel();
            // need to get access records from main/plugins/cores
            var accessRecordList = GetAccessRecordsFromAssemblies();

            var accessRightsModelList = new List<AccessRightsModel>();

            accessRightsModelList = accessRecordList.Select(accessRecord =>
            {
                var accessRightsModel = new AccessRightsModel();
                accessRightsModel = _mapper.Map(accessRecord, accessRightsModel);

                return accessRightsModel;

            }).ToList();

            var userRoleModelList = new List<UserRoleModel>();

            var userRoles = await _userService.GetUserRoleList();

            userRoleModelList = userRoles.Select(role =>
            {
                var userRoleModel = new UserRoleModel();
                userRoleModel = _mapper.Map(role, userRoleModel);

                return userRoleModel;

            }).ToList();
            
            // To verify if access rights has access record data or this access record
            var accessRightsRecordModelList = new List<AccessRecordModel>();
            var accessRights = await _accessRightsService.GetList();
            foreach (var access in accessRights)
            {
                if (access.AccessRecordData != null && access.AccessRecordData.IsNotNullOrEmptyJson())
                {
                    var systemNameList = JsonConvert.DeserializeObject<List<string>>(access.AccessRecordData);
                    foreach (var systemName in systemNameList)
                    {
                        var accessRightsRecordModel = new AccessRecordModel();
                        accessRightsRecordModel.RoleId = access.UserRoleId;
                        accessRightsRecordModel.SystemName = systemName;

                        accessRightsRecordModelList.Add(accessRightsRecordModel);
                    }
                }
            }

            listModel.UserRoles = userRoleModelList;
            listModel.AccessRightsRecords = accessRightsRecordModelList;

            _baseAdminModelFactory.PrepareBaseAdminListModel(listModel, accessRightsModelList, searchModel.PageSize, searchModel.PageNumber, searchModel.Skip, accessRecordList.Count());

            return listModel;
        }

        private List<AccessRecord> GetAccessRecordsFromAssemblies()
        {
            var accessRecordList = new List<AccessRecord>();
            var modulePluginAssemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();
            var mainWebAssembly = Assembly.GetExecutingAssembly();
            modulePluginAssemblies.Add(mainWebAssembly);

            if (modulePluginAssemblies.Any())
            {
                foreach (var pluginAssembly in modulePluginAssemblies)
                {
                    var ARTypeList = pluginAssembly.GetTypes().Where(c => c.Name.Contains("AccessRightsRecord") && c.IsClass).ToList();
                    foreach (var ARType in ARTypeList)
                    {
                        if (ARType != null)
                        {
                            var ARRInstance = Activator.CreateInstance(ARType);
                            var ARRfields = ARType.GetFields().Where(c => c.FieldType == typeof(AccessRecord)).ToList();
                            foreach (var ARRfield in ARRfields)
                            {
                                var accessRightsRecord = ARRfield.GetValue(ARRInstance) as AccessRecord;
                                if (accessRightsRecord == null)
                                    continue;

                                accessRecordList.Add(accessRightsRecord);
                            }
                        }
                    }
                }
            }

            return accessRecordList;
        }
    }
}
