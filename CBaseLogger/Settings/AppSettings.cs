using CBaseLogger.Enums;
using Microsoft.Extensions.Options;

namespace CBaseLogger.Settings;

public class AppSettings
{
    public const string SectionName = "App";
    
    public int Timeout { get; set; } = 60_000; // 1 minute by default
    public int LoggingInterval { get; set; } = 10_800_000; // 3 hours by default
    public IntervalType IntervalType { get; set; } = IntervalType.Relative; // Default to relative interval
    public int AbsoluteIntervalStartHour { get; set; } = 0; // Default to midnight
    public required string TimeZone { get; set; }
    public int RateLimitMaxRequestInHour { get; set; } = 10; // Default to 10 requests per hour
    public bool OfflineMode { get; set; }
}

public class AppSettingsValidation : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings settings)
    {
        if (settings.Timeout < 0)
        {
            return ValidateOptionsResult.Fail("Timeout must be greater than or equal to 0.");
        }
        
        if (settings.LoggingInterval < 6000)
        {
            return ValidateOptionsResult.Fail("Logging interval must be greater than or equal to 6000 (1 minute).");
        }
        
        if (settings is { IntervalType: IntervalType.Absolute, AbsoluteIntervalStartHour: < 0 or > 23 })
        {
            return ValidateOptionsResult.Fail("Absolute interval start hour must be between 0 and 23.");
        }
        
        return ValidateOptionsResult.Success;
    }
}
