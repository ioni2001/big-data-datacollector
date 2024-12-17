using Big.Data.DataCollection.Models.Configuration;
using Big.Data.DataCollection.Models.DbEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebHdfs.Client;

namespace Big.Data.DataCollection.Repositories.HadoopRepositories;

public class CommentsHadoopRepository : ICommentsHadoopRepository
{
    private readonly WebHdfsClient _webHdfsClient;
    private readonly HadoopSettings _hadoopSettings;
    private readonly ILogger<CommentsHadoopRepository> _logger;

    public CommentsHadoopRepository(IOptions<HadoopSettings> hadoopSettings, ILogger<CommentsHadoopRepository> logger)
    {
        _hadoopSettings = hadoopSettings.Value;
        _webHdfsClient = new WebHdfsClient(_hadoopSettings.BaseAddress, _hadoopSettings.Username);
        _logger = logger;
    }

    public async Task AddCommentAsync(SocialMediaComment comment)
    {
        // Ensure directory exists
        await _webHdfsClient.MakeDirectoryAsync(_hadoopSettings.CommentsDirectoryPath);

        var filename = $"{comment.Name}_{DateTime.Now.Ticks}.json";
        var filePath = $"{_hadoopSettings.CommentsDirectoryPath}/{filename}";

        var jsonComment = JsonConvert.SerializeObject(comment, Formatting.Indented);

        await _webHdfsClient.WriteStreamAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonComment)), filePath);

        _logger.LogInformation("Comment with id {Id} was successfully saved in HDFS", comment.Id);
    }
}
