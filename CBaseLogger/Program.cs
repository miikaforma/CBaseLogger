using CBaseLogger.CBase;
using CBaseLogger.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace CBaseLogger;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        // Create service collection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        // Create service provider
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Example log level prints
        Log.Verbose("Verbose");
        Log.Debug("Debug");
        Log.Information("Information");
        Log.Warning("Warning");
        Log.Error("Error");
        Log.Fatal("Fatal");

        // Entry to run app
        await serviceProvider.GetRequiredService<Logger>().RunAsync(cancellationTokenSource.Token);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Add instances to DI
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add Serilog
        services.AddLogging(config => 
        {
            config.ClearProviders();
            config.AddSerilog(dispose: true);
        });

        // Add logger
        services.AddTransient<Logger>();
        
        // Add settings validators
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();
        services.AddSingleton<IValidateOptions<CBaseSettings>, CBaseSettingsValidation>();
        services.AddSingleton<IValidateOptions<TimescaleDbSettings>, TimescaleDbSettingsValidation>();

        // Add settings
        services
            .AddOptions<AppSettings>()
            .Bind(configuration.GetSection(AppSettings.SectionName))
            .ValidateOnStart();
        services
            .AddOptions<CBaseSettings>()
            .Bind(configuration.GetSection(CBaseSettings.SectionName))
            .ValidateOnStart();
        services
            .AddOptions<TimescaleDbSettings>()
            .Bind(configuration.GetSection(TimescaleDbSettings.SectionName))
            .ValidateOnStart();


        // CBaseClient
        services.AddTransient<ICBaseClient, CBaseClient>();
    }
}