namespace RA.Core.Models.BaseModels
{
    public class BaseSearchModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int Skip { get; set; }
        public int TotalItems { get; set; }
        public int CurrentItemsShown { get; set; }
    }
}
