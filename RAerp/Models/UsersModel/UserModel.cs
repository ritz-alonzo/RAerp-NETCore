using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.BaseModels;
using RA.Data.Data;
using System.ComponentModel;

namespace RAerp.Models.UsersModel
{
    public class UserModel : BaseAdminModel
    {
        public UserModel()
        {
            AvailableUserRoles = new List<SelectListItem>();
            AvailableAccounts = new List<SelectListItem>();
        }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Email")]
        public string Email { get; set; }
        [DisplayName("Username")]
        public string Username { get; set; }
        [DisplayName("Password")]
        public string Password { get; set; }
        [DisplayName("Current Password")]
        public string CurrentPassword { get; set; }
        [DisplayName("New Password")]
        public string NewPassword { get; set; }
        [DisplayName("Confirm New Password")]
        public string ConfirmNewPassword { get; set; }
        [DisplayName("Contact Number")]
        public string ContactNo { get; set; }
        [DisplayName("One Time PIN")]
        public string OneTimePIN { get; set; }
        [DisplayName("Created On")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("Modified On")]
        public DateTime? ModifiedOn { get; set; }
        public UserAccountStatus AccountStatus { get; set; }
        public string AccountStatusString { get; set; }
        [DisplayName("Status")]
        public int AccountStatusId
        {
            get { return (int)AccountStatus; }
            set { AccountStatus = (UserAccountStatus)value; }
        }
        [DisplayName("Deleted")]
        public bool Deleted { get; set; }
        [DisplayName("Role")]
        public Guid UserRoleId { get; set; }
        [DisplayName("Verified")]
        public bool IsVerified { get; set; }
        public List<SelectListItem> AvailableUserRoles { get; set; }
        public List<SelectListItem> AvailableAccounts { get; set; }

    }
}
