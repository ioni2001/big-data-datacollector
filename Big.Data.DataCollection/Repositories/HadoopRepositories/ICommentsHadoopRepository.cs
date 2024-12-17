using Big.Data.DataCollection.Models.DbEntities;

namespace Big.Data.DataCollection.Repositories.HadoopRepositories;

public interface ICommentsHadoopRepository
{
    Task AddCommentAsync(SocialMediaComment comment);
}
