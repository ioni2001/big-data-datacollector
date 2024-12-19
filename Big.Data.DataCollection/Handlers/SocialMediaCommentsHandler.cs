using AutoMapper;
using Big.Data.DataCollection.Models.Configuration;
using Big.Data.DataCollection.Models.DbEntities;
using Big.Data.DataCollection.Models.Events;
using Big.Data.DataCollection.Repositories.HadoopRepositories;
using Big.Data.DataCollection.Repositories.MongoDbRepositories;
using Big.Data.DataCollection.Services;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Big.Data.DataCollection.Handlers;

public class SocialMediaCommentsHandler : IMessageHandler<SocialMediaCommentEvent>
{
    private readonly ILogger<SocialMediaCommentsHandler> _logger;
    private readonly ICommentsHadoopRepository _commentsHadoopRepository;
    private readonly ICommentsMongoDbRepository _commentsMongoDbRepository;
    private readonly ICsvParserService _csvParserService;
    private readonly IMapper _mapper;
    private readonly ApplicationFlags _applicationFlags;

    public SocialMediaCommentsHandler(
        ILogger<SocialMediaCommentsHandler> logger,
        ICommentsHadoopRepository commentsHadoopRepository,
        ICommentsMongoDbRepository commentsMongoDbRepository,
        ICsvParserService csvParserService,
        IMapper mapper,
        IOptions<ApplicationFlags> applicationFlags)
    {
        _logger = logger;
        _commentsHadoopRepository = commentsHadoopRepository;
        _commentsMongoDbRepository = commentsMongoDbRepository;
        _csvParserService = csvParserService;
        _mapper = mapper;
        _applicationFlags = applicationFlags.Value;
    }

    public async Task Handle(IMessageContext context, SocialMediaCommentEvent message)
    {
        if (_applicationFlags.IsHadoopFullDataIntegrationEnabled)
        {
            await IntegrateFullDataToHadoop();
            _applicationFlags.IsHadoopFullDataIntegrationEnabled = false;
        }
        else
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

    private async Task IntegrateFullDataToHadoop()
    {
        var comments = _csvParserService.GetCommentData();

        foreach (var comment in comments)
        {
            var dbComment = _mapper.Map<SocialMediaComment>(comment);
            dbComment.Id = Guid.NewGuid().ToString();

            await _commentsHadoopRepository.AddCommentAsync(dbComment);
        }
    }
}
