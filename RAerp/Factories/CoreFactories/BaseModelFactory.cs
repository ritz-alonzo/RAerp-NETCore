using Microsoft.AspNetCore.Http;
using RA.Core.Models.BaseModels;
using RA.WebFramework.Extensions;
using RAerp.Helpers.UserHelper;
using RAerp.Services.Configurations;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace RAerp.Factories.CoreFactories
{
    /// <summary>
    /// Base factory to be used in preparing searchModel, listModel, model 
    /// </summary>
    /// will try to add mapper here if model can map generic
    /// if will work
    public class BaseModelFactory : IBaseModelFactory
    {
        #region Constants
        private readonly IUserIdentity _userIdentity;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public BaseModelFactory(IUserIdentity userIdentity, IHttpContextAccessor httpContextAccessor, ISettingService settingService)
        {
            _userIdentity = userIdentity;
            _httpContextAccessor = httpContextAccessor;
            _settingService = settingService;
        }
        #endregion

        /// <summary>
        /// Preparing base search model
        /// </summary>
        /// <typeparam name="TSearch"></typeparam>
        /// <param name="searchModel"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public TSearch PrepareBaseSearchModel<TSearch>(TSearch searchModel, int pageSize, int pageNumber)
            where TSearch : BaseSearchModel
        {
            searchModel.PageSize = pageSize;
            searchModel.PageNumber = pageNumber;

            return searchModel;
        }

        /// <summary>
        /// Preparing base model
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TModel> PrepareBaseModelAsync<TModel>(TModel model)
            where TModel : BaseModel
        {
            if (model.CreatedByUser.Id.IsNullOrEmpty())
            {
                var currentUser = await _userIdentity.GetCurrentUserAsync(_httpContextAccessor.HttpContext);
                if (currentUser != null)
                    model.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(currentUser);
            }
            else
            {
                var user = await _userIdentity.GetUserDetailsAsync(model.CreatedByUser.Id);
                if (user != null)
                    model.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(user);
            }

            return model;
        }

        public TList PrepareBaseListModel<TList, TModel, TSearch>(TList list, List<TModel> listModel, TSearch searchModel, int totalItems)
            where TList : BaseListModel<TModel>
            where TModel : BaseModel
            where TSearch : BaseSearchModel
        {
            if (listModel.Count == 1)
                list.Items = listModel.Take(searchModel.PageSize).ToList();
            else
                list.Items = listModel.Skip((searchModel.PageNumber - 1) * searchModel.PageSize).Take(searchModel.PageSize).ToList();

            var totalPages = (int)Math.Ceiling(totalItems / (double)searchModel.PageSize);

            if (searchModel.PageNumber > 2)
                list.TotalItems = searchModel.PageNumber;
            else
                list.TotalItems = totalPages;

            list.PageSize = searchModel.PageSize;
            list.PageNumber = searchModel.PageNumber;
            searchModel.CurrentItemsShown = list.Items.Count;

            return list;
        }
    }
}
