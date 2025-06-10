using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.EntityTypes
{
    /// <summary>
    /// Represents main classes that will be main functions in this project
    /// will use this to check if assembly is installed or not
    /// </summary>
    public class EntityType : BaseEntity
    {
        /// <summary>
        /// Name of Class
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// System name of Class
        /// </summary>
        public string EntitySystemName { get; set; }
        /// <summary>
        /// Created Entity in this Entity Type
        /// </summary>
        public string EntityClassificationName { get; set; }
        /// <summary>
        /// Installed or UnInstalled
        /// </summary>
        public bool Installed { get; set; }
        public DateTime InstalledOn { get; set; }
        public DateTime? UnInstalledOn { get; set; }
        /// <summary>
        /// Parent entity in this Entity Type
        /// </summary>
        public Guid? ParentEntityTypeId { get; set; }

    }
}
