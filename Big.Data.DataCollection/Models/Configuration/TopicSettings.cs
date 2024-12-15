namespace Big.Data.DataCollection.Models.Configuration;
public class TopicSettings
{
    public required string Topic { get; set; }
    public required string GroupId { get; set; }
    public required string WorkerName { get; set; }
}