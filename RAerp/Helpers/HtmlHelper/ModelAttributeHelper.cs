using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Helpers.HtmlHelper
{
    public class ModelAttributeHelper : IModelAttributeHelper
    {
        public string GetModelAttributeDisplayNameValue<TModel>(string propertyName)
            where TModel : BaseModel
        {
            var attributeValue = "";

            // will need to create shortcut for this
            var propertyOfModel = typeof(TModel).GetProperty(propertyName).GetCustomAttributesData();

            var displayNameAttribute = propertyOfModel.Where(c => c.AttributeType.Name.Equals("DisplayNameAttribute")).Select(c => c.ConstructorArguments).FirstOrDefault();

            if (displayNameAttribute.Any())
            {
                attributeValue = displayNameAttribute.Select(c => c.Value.ToString()).FirstOrDefault();
            }

            return attributeValue;
        }

        public string GetAdminModelAttributeDisplayNameValue<TModel>(string propertyName)
            where TModel : BaseAdminModel
        {
            var attributeValue = "";

            // will need to create shortcut for this
            var propertyOfModel = typeof(TModel).GetProperty(propertyName).GetCustomAttributesData();

            var displayNameAttribute = propertyOfModel.Where(c => c.AttributeType.Name.Equals("DisplayNameAttribute")).Select(c => c.ConstructorArguments).FirstOrDefault();

            if (displayNameAttribute.Any())
            {
                attributeValue = displayNameAttribute.Select(c => c.Value.ToString()).FirstOrDefault();
            }

            return attributeValue;
        }
    }
}
