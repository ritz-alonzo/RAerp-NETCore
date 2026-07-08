using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models;
using RA.Core.Models.BaseModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RAerp.Models.ApplicationSettingsModel
{
    public class ApplicationSettingModel : BaseAdminModel
    {
        [Required(ErrorMessage = "Application Name is required")]
        [Display(Name = "Application Name")]
        public string ApplicationName { get; set; } = string.Empty;

        [Display(Name = "Client ID")]
        public string ClientId { get; set; } = string.Empty;

        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; }
        public Guid CreatedById { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        [Display(Name = "User Login Attempt Limit")]
        public int LoginAttemptLimit { get; set; }
        [Display(Name = "OTP Attempt Limit")]
        public int OneTimePINAttemptLimit { get; set; }
        [Display(Name = "Email Resend Verification Limit")]
        public int EmailVerificationAttemptLimit { get; set; }
        [Display(Name = "Email Limit")]
        public int EmailLimit { get; set; }
        [Display(Name = "Email Verification Enabled")]
        public int IsEmailVerificationEnabled { get; set; }
        [Display(Name = "Default TimeZone")]
        public string DefaultTimeZone { get; set; }
        public List<SelectListItem> AvailableTimeZones { get; set; } = new List<SelectListItem>();
    }
}