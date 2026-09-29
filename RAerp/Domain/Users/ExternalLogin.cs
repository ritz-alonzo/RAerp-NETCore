using RA.Core.Domain;

namespace RAerp.Domain.Users
{
    public class ExternalLogin : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Provider { get; set; }
        public string ProviderSubjectId { get; set; }
        public string ProviderEmail { get; set; }
        public string ProviderName { get; set; }
        public string ProviderPictureUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLoginOn { get; set; }
    }
}
