using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.OverviewModels
{
    public class UserOverviewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        [DisplayName("Created By")]
        public string FullName { get; set; }
    }
}
