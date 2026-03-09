using RA.Core.Models.BaseModels;

namespace RAerp.Helpers.HtmlHelper
{
    public interface IModelAttributeHelper
    {
        string GetModelAttributeDisplayNameValue<TModel>(string propertyName) where TModel : BaseModel;
        string GetAdminModelAttributeDisplayNameValue<TModel>(string propertyName)
            where TModel : BaseAdminModel;
        string GetModelDataType<TModel>(string propertyName)
            where TModel : class;
    }
}