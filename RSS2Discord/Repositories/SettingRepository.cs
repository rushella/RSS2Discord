using LiteDB;
using RSS2Discord.Database.Tables;

namespace RSS2Discord.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly LiteDatabase _db;

    public SettingRepository(LiteDatabase db)
    {
        _db = db;
    }
    public Setting? GetSetting(string key)
    {
        var settings = _db.GetCollection<Setting>("Settings");
        var setting = settings.FindOne(x => x.Key.Equals(key));
        return setting;
    }

    public void UpsertSetting(Setting setting)
    {
        var settings = _db.GetCollection<Setting>("Settings");
        settings.Upsert(setting);
    }
}