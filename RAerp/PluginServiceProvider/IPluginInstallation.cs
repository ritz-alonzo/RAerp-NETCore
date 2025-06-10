namespace RAerp.PluginServiceProvider
{
    public interface IPluginInstallation
    {
        Task PluginTypeInstall(IApplicationBuilder app);
    }
}
