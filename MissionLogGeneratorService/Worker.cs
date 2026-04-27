using AarhusSpaceProgram.Shared.DTOs;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace MissionLogGeneratorService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IHttpClientFactory _httpFactory;

    private readonly IMongoCollection<MissionLog> _logs;

    private readonly string _apiBaseUrl;

    public Worker(
        ILogger<Worker> logger,
        IHttpClientFactory httpFactory,
        IConfiguration config)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _apiBaseUrl = config["AspMissionManagementApiBaseUrl"] 
            ?? throw new Exception("API base URL missing");
        var mongoUrl = config["MongoConnectionString"] 
            ?? "mongodb://localhost:27017";
        var db = new MongoClient(mongoUrl).GetDatabase(config["MongoDatabaseName"]);
        _logs = db.GetCollection<MissionLog>("MissionLogs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var http = _httpFactory.CreateClient();

        while (!stoppingToken.IsCancellationRequested)
        {
            List<MissionSimpleListItemDTO> missions = new();
            try
            {
                var resp = await http.GetAsync(
                    $"{_apiBaseUrl}/Missions?status=Active",
                    stoppingToken);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync(stoppingToken);

                    _logger.LogInformation("Raw JSON from API: {Json}", json);

                    var wrapper = System.Text.Json.JsonSerializer.Deserialize<RestDTO<MissionSimpleListItemDTO[]>>(
                        json,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    missions = wrapper?.Data?.ToList() ?? new List<MissionSimpleListItemDTO>();

                    _logger.LogInformation("Fetched {Count} active missions", missions.Count);
                }
                else
                {
                    _logger.LogWarning("API call failed with status code: {StatusCode}", resp.StatusCode);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("API call exception: {Message}", e.Message);
            }

            foreach (var mission in missions)
            {
                var log = new MissionLog
                {
                    MissionId = mission.Id,
                    Message = GetRandomMessage(),
                    Timestamp = DateTime.Now
                };

                try
                {
                    await _logs.InsertOneAsync(log, stoppingToken);
                    _logger.LogInformation("Generated log for mission {MissionId}: {Message}", log.MissionId, log.Message);
                }
                catch (Exception e)
                {
                    _logger.LogWarning("Failed to insert log for mission {MissionId}: {Message}", log.MissionId, e.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private static string GetRandomMessage()
    {
        var messages = new[]
        {
            "Mission is proceeding as planned.",
            "Minor technical issue detected, but it's being handled.",
            "Crew is in good spirits and performing well.",
            "Unexpected weather conditions encountered, adjusting trajectory.",
            "Successful completion of a critical mission milestone.",
            "Communication with ground control is stable.",
            "Routine system checks completed without issues.",
            "Supplies are sufficient for the duration of the mission.",
            "Crew is conducting scientific experiments as scheduled.",
            "All systems are functioning within normal parameters."
        };
        var random = new Random();
        return messages[random.Next(messages.Length)];
    }
}
