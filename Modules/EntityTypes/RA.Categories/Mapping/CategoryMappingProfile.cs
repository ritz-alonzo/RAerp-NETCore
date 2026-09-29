using AutoMapper;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.DTO;
using RA.Core.Models.PluginModels.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            #region Category
            CreateMap<Category, CategoryModel>();
            CreateMap<CategoryModel, Category>()
                // Safe fallback condition to skip null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CategorySetting, CategoryConfigureModel>();
            CreateMap<CategoryConfigureModel, CategorySetting>()
                // Safe fallback condition to skip null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Category Dto
            CreateMap<Category, CategoryResponseDto>();
            CreateMap<CategoryRequestDto, Category>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
