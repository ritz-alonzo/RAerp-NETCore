using Microsoft.AspNetCore.Mvc.Rendering;
using RA.WebFramework.Data.DataTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebFramework.Models.DataTables
{
    /// <summary>
    /// Represents table data definition
    /// </summary>
    public class ColumnDefinition
    {
        public ColumnDefinition(string recordData)
        {
            RecordData = recordData;
            ColumnSelectList = new List<SelectListItem>();
        }
        /// <summary>
        /// Represents the primary key or Id of a row
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// Represents the model data for this specific column
        /// </summary>
        public string RecordData { get; set; }
        /// <summary>
        /// Represents the header of the column
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Represents the width size of the column
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Enable/Disable auto width size of the column
        /// </summary>
        public bool AutoWidth { get; set; }
        /// <summary>
        /// Gets the render function added in view for the column
        /// </summary>
        public string RenderFunction { get; set; }
        /// <summary>
        /// Represents the preferred class name for the column
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// Show/Hide the column
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Enable/Disable editing of column
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// Represents data control type
        /// </summary>
        public ControlType ControlType { get; set; }
        public string ControlTypeString { get; set; }
        public int ControlTypeId
        {
            get { return (int)ControlType; }
            set { ControlType = (ControlType)value; }
        }
        /// <summary>
        /// Represents the alignment of columns
        /// </summary>
        public ColumnAlignment ColumnAlignment { get; set; }
        public string ColumnAlignmentString { get; set; }
        public int ColumnAlignmentId 
        {
            get { return (int)ColumnAlignment; }
            set { ColumnAlignment = (ColumnAlignment)value; }
        }
        /// <summary>
        /// To show/hide configuration
        /// </summary>
        public bool ShowConfiguration { get; set; }
        /// <summary>
        /// Represents the data for the footer of the column
        /// </summary>
        public string FooterData { get; set; }
        /// <summary>
        /// Flag for SystemName
        /// </summary>
        public bool IsSystemName { get; set; }
        /// <summary>
        /// If the Property is type of class
        /// Indicate in the properties of the class where to get the value
        /// </summary>
        public string PropertyValueHolder { get; set; }
        /// <summary>
        /// Indicates whether to use Code row data as means of Redirection
        /// </summary>
        public bool RedirectUsingCodeEnabled { get; set; }
        /// <summary>
        /// Decimal places for decimal or float columns
        /// </summary>
        public int DecimalPlaces { get; set; }
        /// <summary>
        /// Column binding of List of SelecListItem for dropdown
        /// </summary>
        public List<SelectListItem> ColumnSelectList { get; set; }

    }
}
