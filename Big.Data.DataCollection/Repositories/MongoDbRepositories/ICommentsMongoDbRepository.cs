using Big.Data.DataCollection.Models.DbEntities;

namespace Big.Data.DataCollection.Repositories.MongoDbRepositories;

public interface ICommentsMongoDbRepository
{
    Task AddCommentAsync(SocialMediaComment comment);
}
