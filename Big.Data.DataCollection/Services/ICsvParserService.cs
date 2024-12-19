using Big.Data.DataCollection.Models.Events;

namespace Big.Data.DataCollection.Services;

public interface ICsvParserService
{
    List<SocialMediaCommentEvent> GetCommentData();
}
