using KafkaFlow;
using Microsoft.Extensions.Logging;

namespace Big.Data.DataCollection.Middlewares;

public class EventErrorHandlingMiddleware : IMessageMiddleware
{
    private readonly ILogger<EventErrorHandlingMiddleware> _logger;

    public EventErrorHandlingMiddleware(ILogger<EventErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {Message}", context.Message);
        }
    }
}
