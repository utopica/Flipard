using Flipard.Domain.Interfaces;
using Flipard.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flipard.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, string tessDataPath)
    {
        services.AddSingleton<IOcrService>(provider => 
        {
            var logger = provider.GetRequiredService<ILogger<TesseractOcrService>>();
            return new TesseractOcrService(tessDataPath, logger);
        });
    }
}