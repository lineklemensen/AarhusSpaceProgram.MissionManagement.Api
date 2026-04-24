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
            ?? "Mongodb://localhost:27017";
        var db = new MongoClient(mongoUrl).GetDatabase("MissionLogs");
        _logs = db.GetCollection<MissionLog>("Logs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var http = _httpFactory.CreateClient();

        while (!stoppingToken.IsCancellationRequested)
        {
            List<MissionSimpleListItemDTO> missions = new();
            try
            {
                var resp = http.GetAsync(
                    $"{_apiBaseUrl}/Missions?status=Active",
                    stoppingToken);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync(stoppingToken);

                    missions = System.Text.Json.JsonSerializer.Deserialize<List<MissionSimpleListItemDTO>>(json) 
                        ?? new List<MissionSimpleListItemDTO>();
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
                var log = new MissionLogGeneratorService
                {
                    MissionId = mission.Id,
                    Message = GetRandommessage(),
                    TimestampAttribute = DateTime.Now
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

}
