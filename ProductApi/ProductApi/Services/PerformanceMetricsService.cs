namespace ProductApi.Services
{
    public class PerformanceMetricsService : BackgroundService
    {
        private readonly ILogger<PerformanceMetricsService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public PerformanceMetricsService(ILogger<PerformanceMetricsService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    LogSystemMetrics();
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error logging performance metrics");
                }
            }
        }

        private void LogSystemMetrics()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();

            _logger.LogInformation(
                "Performance Metrics: Memory Usage: {MemoryMB} MB, CPU Time: {CPUTime}, Threads: {ThreadCount}",
                process.WorkingSet64 / 1024 / 1024,
                process.TotalProcessorTime,
                process.Threads.Count);
        }
    }
}
