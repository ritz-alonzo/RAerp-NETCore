using RA.Core.Domain;

namespace RAerp.Models.ApiModel
{
    public class ApiResponseAdminListModel<TEntity>
        where TEntity : BaseAdminEntity
    {
        public ApiResponseAdminListModel()
        {
            Data = new List<TEntity>();
        }
        public string Status { get; set; }
        public List<TEntity> Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
