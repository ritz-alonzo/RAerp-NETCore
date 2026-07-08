namespace RAerp.Services.FileServices
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
    }
}