using AutoMapper;
using Big.Data.DataCollection.Handlers;
using Big.Data.DataCollection.Middlewares;
using Big.Data.DataCollection.Models.Configuration;
using Big.Data.DataCollection.Models.Events;
using Big.Data.DataCollection.Profiles;
using Big.Data.DataCollection.Repositories.HadoopRepositories;
using Big.Data.DataCollection.Repositories.MongoDbRepositories;
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace Big.Data.DataCollection;

public class Program
{
    private static ILogger<Program>? _logger;

    static async Task Main(string[] args)
    {
        IHost appHost = Host
            .CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
            })
            .ConfigureLogging((hbc, logging) =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;

                    options.AddConsoleExporter();
                });
            })
            .ConfigureServices((hbc, services) =>
            {
                services.AddSingleton<ICommentsHadoopRepository, CommentsHadoopRepository>();
                services.AddSingleton<ICommentsMongoDbRepository, CommentsMongoDbRepository>();

                services.Configure<HadoopSettings>(hbc.Configuration.GetRequiredSection("HadoopSettings"));
                services.Configure<MongoDbSettings>(hbc.Configuration.GetRequiredSection("MongoDbSettings"));

                var mapperConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfile());
                });

                IMapper mapper = mapperConfig.CreateMapper();
                services.AddSingleton(mapper);

                var kafkaSettings = hbc.Configuration.GetRequiredSection("KafkaSettings").Get<KafkaSettings>();
                var commentsConsumerSettings = hbc.Configuration.GetRequiredSection("CommentsConsumerSettings").Get<TopicSettings>();

                services.AddKafkaFlowHostedService(kafka => kafka
                    .UseMicrosoftLog()
                    .AddCluster(cluster => cluster
                    .WithBrokers(kafkaSettings?.BootstrapServers)
                    .WithSchemaRegistry(schema =>
                    {
                        schema.Url = kafkaSettings?.SchemaRegistry;
                        schema.BasicAuthCredentialsSource = Confluent.SchemaRegistry.AuthCredentialsSource.UserInfo;
                        schema.BasicAuthUserInfo = $"{kafkaSettings?.SaslUserName}:{kafkaSettings?.SaslPassword}";
                    })
                    .WithSecurityInformation(information =>
                    {
                        information.SecurityProtocol = SecurityProtocol.SaslSsl;
                        information.SaslMechanism = SaslMechanism.ScramSha256;
                        information.SaslUsername = kafkaSettings?.SaslUserName;
                        information.SaslPassword = kafkaSettings?.SaslPassword;
                    })
                    .AddConsumer(consumer => consumer
                        .Topic(commentsConsumerSettings?.Topic)
                        .WithName(commentsConsumerSettings?.WorkerName)
                        .WithGroupId(commentsConsumerSettings?.GroupId)
                        .WithBufferSize(kafkaSettings!.BufferSize)
                        .WithWorkersCount(kafkaSettings!.WorkersCount)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(middlewares => middlewares
                                                       .AddSingleTypeDeserializer<SocialMediaCommentEvent, NewtonsoftJsonDeserializer>()
                                                       .Add<EventErrorHandlingMiddleware>()
                                                       .AddTypedHandlers(handlers => handlers
                                                       .AddHandler<SocialMediaCommentsHandler>()
                                                       .WhenNoHandlerFound(context =>
                                                                           Console.WriteLine($"Messages from partition {context.ConsumerContext.Partition} or Offset {context.ConsumerContext.Offset} are unhandled")))))
                    ).AddOpenTelemetryInstrumentation());
            }).Build();

        _logger = appHost.Services.GetRequiredService<ILogger<Program>>();

        _logger.LogInformation("App Host created successfully");

        await appHost.RunAsync();
    }
}