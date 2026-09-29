namespace RAerp.DTO.FileMapping
{
    public class FileMappingRequestDto
    {
        public IFormFile File { get; set; }
        public Guid EntityId { get; set; }
        public string SystemName { get; set; }
        public string FilePath { get; set; }
        public string Endpoint { get; set; }
    }
}
