using RA.Core.Domain;

namespace RAerp.Models.ApiModel
{
    public class ApiResponseListModel<TEntity>
        where TEntity : BaseEntity
    {
        public ApiResponseListModel()
        {
            Data = new List<TEntity>();
        }
        public string Status { get; set; }
        public List<TEntity> Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
