using Big.Data.DataCollection.Models.Events;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Big.Data.DataCollection.Handlers;

public class SocialMediaCommentsHandler : IMessageHandler<SocialMediaCommentEvent>
{
    private readonly ILogger<SocialMediaCommentsHandler> _logger;

    public SocialMediaCommentsHandler(ILogger<SocialMediaCommentsHandler> logger)
    {
        _logger = logger;
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
    }
}
