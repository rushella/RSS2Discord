using System;

namespace RSS2Discord.Models;

public class NewsMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Uri? Url { get; set; }
    public Uri? CoverImageUrl { get; set; }
}