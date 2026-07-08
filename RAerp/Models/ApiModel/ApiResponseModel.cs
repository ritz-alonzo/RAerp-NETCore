using RA.Core.Domain;

namespace RAerp.Models.ApiModel
{
    public class ApiResponseModel<TEntity>
        where TEntity : BaseEntity
    {
        public string Status { get; set; }
        public TEntity Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
