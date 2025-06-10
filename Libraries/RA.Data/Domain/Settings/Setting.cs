using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.Settings
{
    /// <summary>
    /// Setting table will serve as all setting of Entities, Forms
    /// </summary>
    public class Setting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Data { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? EntityTypeId { get; set; }
        public Guid? FormTypeId { get; set; }
    }
}
