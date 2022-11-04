using RSS2Discord.Database.Tables;

namespace RSS2Discord.Repositories;

public interface ISettingRepository
{
    Setting? GetSetting(string key);
    void UpsertSetting(Setting setting);
}