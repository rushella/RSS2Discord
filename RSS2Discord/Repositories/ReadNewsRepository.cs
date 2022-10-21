using LiteDB;
using RSS2Discord.Database.Tables;

namespace RSS2Discord.Repositories;

public class ReadNewsRepository : IReadNewsRepository
{
    private readonly LiteDatabase _db;

    public ReadNewsRepository(LiteDatabase db)
    {
        _db = db;
    }

    public ReadNews? GetLatestReadNewsAsync()
    {
        var readNewsCollection = _db.GetCollection<ReadNews>("ReadNews"); 
        
        readNewsCollection.EnsureIndex("PublishDate");

        return readNewsCollection.FindOne(Query.All("PublishDate", Query.Descending));
    }

    public void AddLatestReadNewsAsync(ReadNews readNews)
    {
        var readNewsCollection = _db.GetCollection<ReadNews>("ReadNews");
        
        readNewsCollection.EnsureIndex("PublishDate");
        
        readNewsCollection.Insert(readNews);
    }
}