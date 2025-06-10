using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.DataChanges
{
    /// <summary>
    /// Unsaved/unapplied changes to forms/entity will be save to this table
    /// </summary>
    public class DataChange : BaseEntity
    {
        /// <summary>
        /// The main ID where data is stored,
        /// it can be from FormId, EntityId etc.
        /// </summary>
        public Guid DataId { get; set; }
        /// <summary>
        /// The item id of the Form/Entity
        /// </summary>
        public Guid? ItemDataId { get; set; }
        /// <summary>
        /// This is a json format where data modifications are stored
        /// </summary>
        public string Record { get; set; }
        public string SystemName { get; set; }
        public bool IsApplied { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
