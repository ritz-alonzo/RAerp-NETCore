namespace RAerp.Security.Idempontency.Domain
{
    public class IdempotencyRecord
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string PayloadHash { get; set; } = null!;
        public string Endpoint { get; set; }
        public string Status { get; set; } = null!; // Processing | Completed | Failed
        public int StatusCode { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? CompletedUtc { get; set; }
    }
}
