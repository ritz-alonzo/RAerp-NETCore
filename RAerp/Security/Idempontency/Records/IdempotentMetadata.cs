namespace RAerp.Security.Idempontency.Records
{
    public record IdempotentMetadata(
        string IdempotencyKey,
        string Status, // "InFlight" or "Completed"
        string PayloadHash,
        int? StatusCode = null,
        string ResponseBody = null,
        DateTime? CreatedAtUtc = null)
    {
    }
}
