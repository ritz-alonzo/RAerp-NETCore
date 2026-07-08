using AutoMapper;
using RA.Core.Models.PluginModels.Discounts;
using RA.Discounts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Mapping
{
    public class DiscountMappingProfile : Profile
    {
        public DiscountMappingProfile()
        {
            #region Discount
            CreateMap<DiscountSetting, DiscountConfigureModel>();
            CreateMap<DiscountConfigureModel, DiscountSetting>();
            #endregion
        }
    }
}
