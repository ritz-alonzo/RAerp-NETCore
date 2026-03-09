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
            var attributeValue = propertyName;

            // will need to create shortcut for this
            var propertyOfModel = typeof(TModel).GetProperty(propertyName).GetCustomAttributesData();

            var displayNameAttribute = propertyOfModel.Where(c => c.AttributeType.Name.Equals("DisplayNameAttribute")).Select(c => c.ConstructorArguments).FirstOrDefault();

            if (displayNameAttribute != null && displayNameAttribute.Any())
            {
                attributeValue = displayNameAttribute.Select(c => c.Value.ToString()).FirstOrDefault();
            }

            return attributeValue;
        }

        public string GetAdminModelAttributeDisplayNameValue<TModel>(string propertyName)
            where TModel : BaseAdminModel
        {
            var attributeValue = propertyName;

            // will need to create shortcut for this
            var propertyOfModel = typeof(TModel).GetProperty(propertyName).GetCustomAttributesData();

            var displayNameAttribute = propertyOfModel.Where(c => c.AttributeType.Name.Equals("DisplayNameAttribute")).Select(c => c.ConstructorArguments).FirstOrDefault();

            if (displayNameAttribute != null && displayNameAttribute.Any())
            {
                attributeValue = displayNameAttribute.Select(c => c.Value.ToString()).FirstOrDefault();
            }

            return attributeValue;
        }

        public string GetModelDataType<TModel>(string propertyName)
            where TModel : class
        {
            var dataType = "";

            PropertyInfo propInfo = typeof(TModel).GetProperty(propertyName);
            if (propInfo != null)
            {
                if (propInfo.PropertyType == typeof(string))
                {
                    dataType = "String";
                }
                else if (propInfo.PropertyType == typeof(int))
                {
                    dataType = "Int";
                }
                else if (propInfo.PropertyType == typeof(Guid))
                {
                    dataType = "Guid";
                }
            }

            return dataType;
        }
    }
}
