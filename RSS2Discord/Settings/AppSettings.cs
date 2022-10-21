namespace RSS2Discord.Settings;

public class AppSettings
{
    public Uri RssSourceUrl { get; set; } = null!;
    public Uri DiscordWebHookUrl { get; set; } = null!;
    public int UpdateCheckInterval { get; set; }
}