using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.Application
{
    public class ApplicationSetting : BaseAdminEntity
    {
        public string ApplicationName { get; set; }
        public string CompanyName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int LoginAttemptLimit { get; set; }
        public int OneTimePINAttemptLimit { get; set; }
        public int EmailVerificationAttemptLimit { get; set; }
        public int EmailLimit { get; set; }
        public bool IsEmailVerificationEnabled { get; set; }
        public string DefaultTimeZone { get; set; }
    }
}
