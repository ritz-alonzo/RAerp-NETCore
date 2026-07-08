using RA.Core.Domain;

namespace RAerp.Models.ApiModel
{
    public class ApiResponseAdminModel<TEntity>
        where TEntity : BaseAdminEntity
    {
        public string Status { get; set; }
        public TEntity Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
