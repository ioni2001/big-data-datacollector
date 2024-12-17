using Big.Data.DataCollection.Models.Configuration;
using Big.Data.DataCollection.Models.DbEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Big.Data.DataCollection.Repositories.MongoDbRepositories;

public class CommentsMongoDbRepository : ICommentsMongoDbRepository
{
    private readonly MongoClient _mongoClient;
    private readonly MongoDbSettings _mongoDbSettings;
    private readonly ILogger<CommentsMongoDbRepository> _logger;

    public CommentsMongoDbRepository(IOptions<MongoDbSettings> mongoSettings, ILogger<CommentsMongoDbRepository> logger)
    {
        _mongoDbSettings = mongoSettings.Value;
        var settings = MongoClientSettings.FromConnectionString(_mongoDbSettings.Uri);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        _mongoClient = new MongoClient(settings);

        _logger = logger;
    }

    public async Task AddCommentAsync(SocialMediaComment comment)
    {
        var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
        var collection = database.GetCollection<SocialMediaComment>("Comments");

        await collection.InsertOneAsync(comment);

        _logger.LogInformation("Comment with id {Id} was successfully saved in MongoDB", comment.Id);
    }
}
