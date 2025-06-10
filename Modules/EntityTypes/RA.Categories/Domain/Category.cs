using RA.Core.PluginData.EntityTypes.Categories;
using RA.EntityTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Domain
{
    public class Category : BaseEntityType
    {
        public int StatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public CategoryStatus Status
        {
            get { return (CategoryStatus)StatusId; }
            set { StatusId = (int)value; }
        }
    }
}
