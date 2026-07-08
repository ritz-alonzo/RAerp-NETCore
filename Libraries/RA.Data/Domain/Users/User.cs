using RA.Core.Domain;
using RA.Data.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RA.Data.Domain.Users
{
    public class User : BaseAdminEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string OneTimePIN { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? LastPasswordChangedDate { get; set; }
        public int AccountStatusId { get; set; }
        // added NotMapped to exclude in reading in database
        [NotMapped]
        public UserAccountStatus AccountStatus
        {
            get { return (UserAccountStatus)AccountStatusId; }
            set { AccountStatusId = (int)value; }
        }
        public bool? IsVerified { get; set; }
        public DateTime? OneTimePINValidUntil { get; set; }
        public int? OneTimePINAttempt { get; set;  }
        public int? FailedLoginAttempt { get; set; }
        public int? EmailResendAttempt { get; set; }
        public bool? IsLoggedOn { get; set; }
        public string Salt { get; set; }
    }
}
