using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenGraphNet;
using OpenGraphNet.Metadata;
using RSS2Discord.Database.Tables;
using RSS2Discord.Models;
using RSS2Discord.Repositories;
using RSS2Discord.Settings;

namespace RSS2Discord.Services;

public class OpenGraphSupportedNewsReader : INewsReader
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<OpenGraphSupportedNewsReader> _logger;
    private readonly ISettingRepository _settingRepository;

    public OpenGraphSupportedNewsReader(
        AppSettings appSettings,
        ILoggerFactory? loggerFactory,
        ISettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
        _appSettings = appSettings;
        _logger = loggerFactory?.CreateLogger<OpenGraphSupportedNewsReader>()
                  ?? NullLoggerFactory.Instance.CreateLogger<OpenGraphSupportedNewsReader>();
    }

    public async Task<IEnumerable<NewsMetadata>> GetUpdatesAsync()
    {
        var rssFeed = LoadRss(_appSettings.RssSourceUrl);
        
        var setting = _settingRepository.GetSetting("lastNewsTimestamp");
        var result = new List<NewsMetadata>();

        if (rssFeed == null)
        {
            return result;
        }

        DateTimeOffset.TryParse(setting?.Value, out var latestPublishDate);

        foreach (var item in rssFeed.Items)
        {
            var newsLink = item.Links.FirstOrDefault()?.Uri;
            
            if (newsLink == null)
            {
                _logger.LogError("Can't find RSS item source link. {Item}", item);
                continue;
            }

            if (latestPublishDate >= item.PublishDate)
            {
                continue;
            }

            latestPublishDate = item.PublishDate;

            try
            {
                result.Add(await GetOpenGraphMetadata(newsLink));
                
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occured during reading Open Graph metadata. From {newsLink}.", newsLink);
            }
        }

        if (setting == null)
        {
            setting = new Setting
            {
                Key = "lastNewsTimestamp",
                Value = latestPublishDate.ToString()
            };
        }
        else
        {
            setting.Value = latestPublishDate.ToString();
        }

        _settingRepository.UpsertSetting(setting);
        
        return result;
    }

    private SyndicationFeed? LoadRss(Uri endpoint)
    {
        var xmlReader = XmlReader.Create(endpoint.ToString());

        SyndicationFeed? rssFeed = null;
        
        try
        {
            rssFeed = SyndicationFeed.Load(xmlReader);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "An error occured while getting RSS feed from {endpoint}", _appSettings.RssSourceUrl);
        }

        return rssFeed;
    }

    private static async Task<NewsMetadata> GetOpenGraphMetadata(Uri newsSource)
    {
        var openGraph = await OpenGraph.ParseUrlAsync(newsSource);

        var result = new NewsMetadata
        {
            Title = openGraph.Title,
            CoverImageUrl = openGraph.Image,
            Url = openGraph.Url
        };

        if (openGraph.Metadata.TryGetValue("og:description", out var description))
            result.Description = description.Value();

        return result;
    }
}