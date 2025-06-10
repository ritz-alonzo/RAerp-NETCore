using RA.Catalogs.Data;
using RA.EntityTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Domain
{
    public class Catalog : BaseEntityType
    {
        public Guid? UOMId { get; set; }
        public int StatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public CatalogStatus Status
        {
            get { return (CatalogStatus)StatusId; }
            set { StatusId = (int)value; }
        }
        // For reference - to be added to model
        //[NopResourceDisplayName("Common.Operations.ServiceBayModel.Fields.ServiceBayStatusId")]
        //public ServiceBayStatus ServiceBayStatus { get; set; }
        //public string ServiceBayStatusString { get; set; }
        ///// <summary>
        ///// ServiceBayStatus id of the entity
        ///// </summary>
        //public int ServiceBayStatusId
        //{
        //    get { return (int)ServiceBayStatus; }
        //    set { ServiceBayStatus = (ServiceBayStatus)value; }
        //}
    }
}
