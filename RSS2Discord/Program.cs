using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RSS2Discord.Repositories;
using RSS2Discord.Services;
using RSS2Discord.Settings;

namespace RSS2Discord;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var serviceProvider = ConfigureServices();
        await StartAsync(serviceProvider);
    }

    private static async Task StartAsync(IServiceProvider serviceProvider)
    {
        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(appSettings.UpdateCheckInterval));
        var newsReader = serviceProvider.GetRequiredService<INewsReader>();
        var newsPublisher = serviceProvider.GetRequiredService<DiscordNewsPublisher>();

        do
        {
            foreach (var news in await newsReader.GetUpdatesAsync())
            {
                await newsPublisher.PublishAsync(news);
                await Task.Delay(1000);
            }
        } while (await timer.WaitForNextTickAsync());
    }

    private static IServiceProvider ConfigureServices()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(LoggerFactory.Create(builder => { builder.AddConsole(); }))
            .AddSingleton<SettingsFactory>()
            .AddSingleton(x => x.GetRequiredService<SettingsFactory>().GetAppSettings())
            .AddSingleton(new LiteDatabase(@"Rss2discordDb.db"))
            .AddSingleton<ISettingRepository, SettingRepository>()
            .AddSingleton<INewsReader, OpenGraphSupportedNewsReader>()
            .AddSingleton<DiscordWebhookClient>()
            .AddSingleton<DiscordNewsPublisher>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}