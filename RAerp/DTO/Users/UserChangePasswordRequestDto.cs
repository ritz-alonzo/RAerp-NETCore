using RA.Core.Domain;

namespace RAerp.DTO.Users
{
    public class UserChangePasswordRequestDto : BaseEntity
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
