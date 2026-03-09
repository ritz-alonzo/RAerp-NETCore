using RA.Core.Domain;
using RA.Core.Models.BaseModels;
using RA.WebFramework.Extensions;
using RAerp.Helpers.UserHelper;
using RAerp.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Factories.CoreFactories
{
    public class BaseAdminModelFactory : IBaseAdminModelFactory
    {
        #region Constants
        private readonly IUserIdentity _userIdentity;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public BaseAdminModelFactory(IUserIdentity userIdentity, 
            IHttpContextAccessor httpContextAccessor,
            ISettingService settingService)
        {
            _userIdentity = userIdentity;
            _httpContextAccessor = httpContextAccessor;
            _settingService = settingService;
        }
        #endregion

        public TSearch PrepareBaseAdminSearchModel<TSearch>(TSearch searchModel, int pageSize, int pageNumber)
            where TSearch : BaseSearchModel
        {
            searchModel.PageSize = pageSize;
            searchModel.PageNumber = pageNumber;

            return searchModel;
        }

        public TList PrepareBaseAdminListModel<TList, TModel>(TList list, List<TModel> listModel, int pageSize, int pageNumber, int skip, int totalItems)
            where TModel : BaseAdminModel
            where TList : BaseAdminListModel<TModel>
        {

            if (listModel.Count == 1)
                list.Items = listModel.Take(pageSize).ToList();
            else
                list.Items = listModel.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (pageNumber >= 2)
                list.TotalItems = pageNumber;
            else
                list.TotalItems = totalPages;

            list.PageSize = pageSize;
            list.PageNumber = pageNumber;

            return list;
        }

        public async Task<TModel> PrepareBaseAdminModelAsync<TModel, TEntity>(TModel model, TEntity entity)
            where TModel: BaseAdminModel
            where TEntity: BaseAdminEntity
        {
            model.SystemName = typeof(TEntity).FullName;

            if (model.CreatedByUser.Id.IsNullOrEmpty() && model.CreatedByUser == null)
            {
                var currentUser = await _userIdentity.GetCurrentUserAsync(_httpContextAccessor.HttpContext);
                if (currentUser != null && model.CreatedByUser == null)
                {
                    model.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(currentUser);
                }
            }
            else
            {
                var user = await _userIdentity.GetUserDetailsAsync(model.CreatedByUser.Id);
                if (user != null)
                    model.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(user);
            }

            return model;
        }
    }
}
