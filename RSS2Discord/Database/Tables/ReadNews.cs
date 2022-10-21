namespace RSS2Discord.Database.Tables;

public class ReadNews
{
    public long Id { get; set; }
    public Uri? Url { get; set; }
    public DateTime PublishDate { get; set; }
}