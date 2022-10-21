using RSS2Discord.Database.Tables;

namespace RSS2Discord.Repositories;

public interface IReadNewsRepository
{
    ReadNews? GetLatestReadNewsAsync();
    void AddLatestReadNewsAsync(ReadNews readNews);
}