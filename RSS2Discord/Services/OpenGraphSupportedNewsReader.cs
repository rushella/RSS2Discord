using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
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
    private readonly IReadNewsRepository _readNewsRepository;

    public OpenGraphSupportedNewsReader(
        AppSettings appSettings,
        ILoggerFactory? loggerFactory,
        IReadNewsRepository readNewsRepository)
    {
        _readNewsRepository = readNewsRepository;
        _appSettings = appSettings;
        _logger = loggerFactory?.CreateLogger<OpenGraphSupportedNewsReader>()
                  ?? NullLoggerFactory.Instance.CreateLogger<OpenGraphSupportedNewsReader>();
    }

    public async Task<IEnumerable<NewsMetadata>> GetUpdatesAsync()
    {
        var xmlReader = XmlReader.Create(_appSettings.RssSourceUrl.ToString());
        var rssFeed = SyndicationFeed.Load(xmlReader);
        var lastReadNews = _readNewsRepository.GetLatestReadNewsAsync();
        var result = new List<NewsMetadata>();

        foreach (var item in rssFeed.Items)
        {
            var newsLink = item.Links.FirstOrDefault()?.Uri;

            if (newsLink == null)
            {
                _logger.LogError("Can't find RSS item source link. {Item}", item);
                continue;
            }

            if (lastReadNews != null &&
                (lastReadNews.PublishDate >= item.PublishDate.UtcDateTime || lastReadNews.Url == newsLink))
            {
                continue;
            }

            try
            {
                result.Add(await GetOpenGraphMetadata(newsLink));
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occured during reading Open Graph metadata. From {newsLink}.", newsLink);
            }

            _readNewsRepository.AddLatestReadNewsAsync(new ReadNews
            {
                Url = newsLink,
                PublishDate = item.PublishDate.UtcDateTime
            });
        }

        return result;
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