using RSS2Discord.Models;

namespace RSS2Discord.Services;

public interface INewsReader
{
    Task<IEnumerable<NewsMetadata>> GetUpdatesAsync();
}