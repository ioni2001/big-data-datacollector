using Big.Data.DataCollection.Models.Events;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Big.Data.DataCollection.Services;

public class CsvParserService : ICsvParserService
{
    private readonly Lazy<List<SocialMediaCommentEvent>> _commentData;

    public CsvParserService()
    {
        _commentData = new Lazy<List<SocialMediaCommentEvent>>(LoadCsvData);
    }

    public List<SocialMediaCommentEvent> GetCommentData()
    {
        return _commentData.Value;
    }

    private List<SocialMediaCommentEvent> LoadCsvData()
    {
        var filePath = Path.Combine("C:\\Users\\Ioni\\source\\repos\\Big.Data.DataCollection\\Big.Data.DataCollection\\DataSetFiles\\dataset_for_database.csv");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file {filePath} was not found.");
        }

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        return csv.GetRecords<SocialMediaCommentEvent>().ToList();
    }
}
