using Microsoft.AspNetCore.SignalR;

public class ClockTimerService : BackgroundService
{
    private readonly IHubContext<KlokkenHub> _hub;
    private static readonly Dictionary<int, DateTime> StartTimes = new();

    public ClockTimerService(IHubContext<KlokkenHub> hub)
    {
        _hub = hub;

        if (StartTimes.Count == 0)
        {
            for (int i = 1; i <= 4; i++)
                StartTimes[i] = DateTime.UtcNow;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var kv in StartTimes)
            {
                var id = kv.Key;
                var start = kv.Value;
                var elapsed = (long)(DateTime.UtcNow - start).TotalMilliseconds;

                await _hub.Clients.All.SendAsync("ClockUpdate", new
                {
                    id,
                    startTime = start,
                    elapsed
                }, cancellationToken: stoppingToken);
            }

            await Task.Delay(100, stoppingToken);
        }
    }

    public static void ResetClockForLocation(string locatie)
    {
        int id = locatie switch
        {
            "Naaldwijk" => 1,
            "Aalsmeer" => 2,
            "Rijnsburg" => 3,
            "Eelde" => 4,
            _ => 1
        };

        StartTimes[id] = DateTime.UtcNow;
    }

}

