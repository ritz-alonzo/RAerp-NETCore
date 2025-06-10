using AutoMapper;
using RA.Categories.Data;
using RA.Categories.Domain;
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
            CreateMap<Category, CategoryModel>();
            CreateMap<CategoryModel, Category>();

            CreateMap<CategorySetting, CategoryConfigureModel>();
            CreateMap<CategoryConfigureModel, CategorySetting>();
        }
    }
}
