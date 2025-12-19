using FindMeHome.Enums;
using FindMeHome.Models;
using FindMeHome.Repositories.AbstractionLayer;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.Services.Background
{
    public class ListingExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ListingExpirationService> _logger;

        public ListingExpirationService(IServiceProvider serviceProvider, ILogger<ListingExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ListingExpirationService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        // Find active properties that have expired
                        var expiredProperties = await unitOfWork.RealEstates.FindAsync(p =>
                            p.Status == PropertyStatus.Active &&
                            p.ExpirationDate != null &&
                            p.ExpirationDate < DateTime.Now);

                        if (expiredProperties.Any())
                        {
                            foreach (var property in expiredProperties)
                            {
                                property.Status = PropertyStatus.Expired;
                                unitOfWork.RealEstates.Update(property);
                            }

                            await unitOfWork.CompleteAsync();
                            _logger.LogInformation($"Expired {expiredProperties.Count()} properties.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for expired listings.");
                }

                // Check every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
