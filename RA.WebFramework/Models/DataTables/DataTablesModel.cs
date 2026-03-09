using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebFramework.Models.DataTables
{
    /// <summary>
    /// Table model for plugins table
    /// will contains properties of the table creation
    /// </summary>
    public class DataTablesModel
    {
        public DataTablesModel()
        {
            ColumnCollection = new List<ColumnDefinition>();
        }
        public IList Records { get; set; }
        /// <summary>
        /// Represents table id of the table
        /// </summary>
        public string GridName { get; set; }
        /// <summary>
        /// Represents Read action of the table
        /// </summary>
        public string UrlRead { get; set; }
        /// <summary>
        /// Represents Create action of the table
        /// </summary>
        public string UrlCreate { get; set; }
        /// <summary>
        /// Represents Update action of the table
        /// </summary>
        public string UrlEdit { get; set; }
        /// <summary>
        /// Represents Delete action of the table
        /// </summary>
        public string UrlDelete { get; set; }
        /// <summary>
        /// Represents Controller name
        /// </summary>
        public string UrlController { get; set; }
        /// <summary>
        /// Show/hide Create button/icon in the table
        /// </summary>
        public bool CreateEnabled { get; set; }
        /// <summary>
        /// Show/hide Update button/icon in the table
        /// </summary>
        public bool EditEnabled { get; set; }
        /// <summary>
        /// Show/hide Delete button/icon in the table
        /// </summary>
        public bool DeleteEnabled { get; set; }
        /// <summary>
        /// Represents the defined class name for the table
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// Represents table columns to be shown in the table
        /// </summary>
        public List<ColumnDefinition> ColumnCollection { get; set; }
        /// <summary>
        /// Show/hide Configuration button in the table
        /// </summary>
        public bool ConfigurationEnabled { get; set; }
        /// <summary>
        /// Action to show the grid in the table
        /// Also as indicator for refresh in grid
        /// </summary>
        public string UrlGridPartialViewAction { get; set; }
        public bool IsEmbeddedTableToForm { get; set; }
        public string UrlGridPartialViewId { get; set; }
    }
}
