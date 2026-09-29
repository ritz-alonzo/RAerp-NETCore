namespace RAerp.DTO.EntityAttributes
{
    public class EntityAttributeQueryRequestDto
    {
        public string SearchAttributeName { get; set; }
        public string SearchSystemName { get; set; }
        public string WebServiceEndpointName { get; set; }
        public List<int> ControlTypeIds { get; set; } = new List<int>();
        public bool ShowActiveOnly { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
