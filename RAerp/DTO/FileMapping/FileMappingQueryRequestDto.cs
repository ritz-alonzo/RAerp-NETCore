namespace RAerp.DTO.FileMapping
{
    public class FileMappingQueryRequestDto
    {
        public Guid? SearchEntityId { get; set; }
        public string SearchEntitySystemName { get; set; }
        public string SearchFileName { get; set; }
        public string SearchFilePath { get; set; }
        public string SearchContentType { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreatedAtTo { get; set; }
    }
}
