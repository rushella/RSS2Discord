using Microsoft.Extensions.Configuration;
using RSS2Discord.Models;

namespace RSS2Discord.Settings;

public class SettingsFactory
{
    private readonly IConfigurationRoot _configuration;

    public SettingsFactory()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();
    }

    public AppSettings GetAppSettings()
    {
        return _configuration.Get<AppSettings>(delegate(BinderOptions options)
        {
            options.ErrorOnUnknownConfiguration = true;
        })!;
    }
}