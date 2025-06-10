using RA.Core.Domain;
using RA.Data.Data.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Domain
{
    public class BaseForm : BaseEntity
    {
        // hidden field no need to add in mapping
        [NotMapped]
        public string FormTypeSystemName { get; set; }
        public string FormNbr { get; set; }
        // status here
        public int StatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public FormStatus Status
        {
            get { return (FormStatus)StatusId; }
            set { StatusId = (int)value; }
        }
        public string Description { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
