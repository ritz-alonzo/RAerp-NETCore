using RA.BusinessEntities.Data;
using RA.Core.PluginData.EntityTypes.BusinessEntities;
using RA.EntityTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Domain
{
    public class BusinessEntity : BaseEntityType
    {
        public int StatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public BusinessEntityStatus Status
        {
            get { return (BusinessEntityStatus)StatusId; }
            set { StatusId = (int)value; }
        }

        // need to add Category here
        public Guid? CategoryId { get ; set; }
        // Address
        public Guid? AddressId { get; set; }
    }
}
