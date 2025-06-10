using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.FormTypes
{
    /// <summary>
    /// Represents main classes that will be main functions in this project
    /// will use this to check if assembly is installed or not
    /// </summary>
    public class FormType : BaseEntity
    {
        /// <summary>
        /// Name of Class
        /// </summary>
        public string FormTypeName { get; set; }
        /// <summary>
        /// System name of Class
        /// </summary>
        public string FormTypeSystemName { get; set; }
        /// <summary>
        /// Created Form in this Form Type
        /// </summary>
        public string FormTypeClassificationName { get; set; }
        /// <summary>
        /// Installed or UnInstalled
        /// </summary>
        public bool Installed { get; set; }
        public DateTime InstalledOn { get; set; }
        public DateTime? UnInstalledOn { get; set; }
    }
}
