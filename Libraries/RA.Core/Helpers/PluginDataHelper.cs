using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Helpers
{
    public static class PluginDataHelper
    {
        public static List<SelectListItem> EnumToSelectListItems<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = Convert.ToInt32(c).ToString(),
                    Text = c.ToString()
                }).ToList();
        }

        public static List<SelectListItem> EnumToSelectListItems<TEnum>(List<int> valuesToInclude)
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Where(c => valuesToInclude.Contains(Convert.ToInt32(c)))
                .Select(c => new SelectListItem()
                {
                    Value = Convert.ToInt32(c).ToString(),
                    Text = c.ToString()
                }).ToList();
        }

        public static List<SelectListItem> EnumToSelectListItems<TEnum>(bool showDefaultNoneValue = false)
            where TEnum : Enum
        {
            var enumSelectListItems = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = Convert.ToInt32(c).ToString(),
                    Text = c.ToString()
                }).ToList();

            if (showDefaultNoneValue)
                enumSelectListItems.Add(new SelectListItem()
                {
                    Value = "0",
                    Text = "None"
                });

            return enumSelectListItems.OrderBy(c => c.Value).ToList();
        }
    }
}
