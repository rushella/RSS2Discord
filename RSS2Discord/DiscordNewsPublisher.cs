using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using RSS2Discord.Models;
using RSS2Discord.Settings;

namespace RSS2Discord;

public class DiscordNewsPublisher
{
    private readonly DiscordWebhookClient _discordWebhookClient;

    public DiscordNewsPublisher(
        AppSettings appSettings,
        DiscordWebhookClient discordWebhookClient)
    {
        _discordWebhookClient = discordWebhookClient;
        _discordWebhookClient.AddWebhookAsync(appSettings.DiscordWebHookUrl);
    }

    public async Task PublishAsync(NewsMetadata newsMetadata)
    {
        var webHookBuilder = new DiscordWebhookBuilder();

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(newsMetadata.Title, newsMetadata.Url?.ToString())
            .WithImageUrl(newsMetadata.CoverImageUrl);

        if (!string.IsNullOrWhiteSpace(newsMetadata.Description)) embed.WithDescription(newsMetadata.Description);

        webHookBuilder.AddEmbed(embed);

        await _discordWebhookClient.BroadcastMessageAsync(webHookBuilder);
    }
}