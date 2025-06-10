using Microsoft.Extensions.DependencyInjection;

namespace RAerp.PluginServiceProvider
{
    public interface IPluginStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
