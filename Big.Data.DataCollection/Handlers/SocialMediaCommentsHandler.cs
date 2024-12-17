using AutoMapper;
using Big.Data.DataCollection.Models.DbEntities;
using Big.Data.DataCollection.Models.Events;
using Big.Data.DataCollection.Repositories.HadoopRepositories;
using Big.Data.DataCollection.Repositories.MongoDbRepositories;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Big.Data.DataCollection.Handlers;

public class SocialMediaCommentsHandler : IMessageHandler<SocialMediaCommentEvent>
{
    private readonly ILogger<SocialMediaCommentsHandler> _logger;
    private readonly ICommentsHadoopRepository _commentsHadoopRepository;
    private readonly ICommentsMongoDbRepository _commentsMongoDbRepository;
    private readonly IMapper _mapper;

    public SocialMediaCommentsHandler(
        ILogger<SocialMediaCommentsHandler> logger,
        ICommentsHadoopRepository commentsHadoopRepository,
        ICommentsMongoDbRepository commentsMongoDbRepository,
        IMapper mapper)
    {
        _logger = logger;
        _commentsHadoopRepository = commentsHadoopRepository;
        _commentsMongoDbRepository = commentsMongoDbRepository;
        _mapper = mapper;
    }

    public async Task Handle(IMessageContext context, SocialMediaCommentEvent message)
    {
        _logger.LogInformation("New social media comment received");
        _logger.LogDebug(
            "Message: {MessageJson}",
            JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }));

        var comment = _mapper.Map<SocialMediaComment>(message);
        comment.Id = Guid.NewGuid().ToString();

        await _commentsHadoopRepository.AddCommentAsync(comment);
        await _commentsMongoDbRepository.AddCommentAsync(comment);
    }
}
