using RA.Core.Domain;

namespace RAerp.DTO.Users
{
    public class UserResponseDto : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Username { get; set; }
        public int AccountStatusId { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsLoggedOn { get; set; }
    }
}
