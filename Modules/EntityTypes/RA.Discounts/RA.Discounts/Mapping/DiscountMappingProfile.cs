using AutoMapper;
using RA.Core.Models.PluginModels.Discounts;
using RA.Discounts.Data;
using RA.Discounts.Domain;
using RA.Discounts.DTO.DiscountRedemptions;
using RA.Discounts.DTO.Discounts;
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

            #region Discount/Discount Redemption Dto
            CreateMap<Discount, DiscountResponseDto>();
            CreateMap<DiscountRequestDto, Discount>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<DiscountRedemption, DiscountRedemptionResponseDto>();
            CreateMap<DiscountRedemptionRequestDto, DiscountRedemption>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
