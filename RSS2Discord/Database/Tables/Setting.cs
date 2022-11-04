using LiteDB;

namespace RSS2Discord.Database.Tables;

public class Setting
{
    [BsonId]
    public string Key { get; set; }
    public string Value { get; set; }
}