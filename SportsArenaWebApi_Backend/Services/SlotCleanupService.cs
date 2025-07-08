using Microsoft.EntityFrameworkCore;
using SportsArenaWebApi_Backend.Models;

namespace SportsArenaWebApi_Backend.Services
{
    public class SlotCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SlotCleanupService> _logger;

        public SlotCleanupService(IServiceProvider serviceProvider, ILogger<SlotCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<SportsArenaDbContext>();

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var pastSlots = await context.Tblvenueslots
                        .Where(s => s.Date < today && !s.IsBooked)
                        .ToListAsync(stoppingToken);

                    if (pastSlots.Any())
                    {
                        context.Tblvenueslots.RemoveRange(pastSlots);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Cleaned up {pastSlots.Count} past unbooked slots");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up past slots");
                }

                // Run cleanup every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}