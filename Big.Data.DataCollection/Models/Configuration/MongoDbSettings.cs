namespace Big.Data.DataCollection.Models.Configuration;

public class MongoDbSettings
{
    public required string Uri { get; set; }
    public required string DatabaseName { get; set; }
}
