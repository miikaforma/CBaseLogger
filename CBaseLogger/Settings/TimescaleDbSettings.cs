using Microsoft.Extensions.Options;

namespace CBaseLogger.Settings;

public class TimescaleDbSettings
{
    public const string SectionName = "TimescaleDB";

    public bool Enabled { get; set; }
    public required string ConnectionString { get; set; }
    public string TableName { get; set; } = "cbase_pv_forecast";
}

public class TimescaleDbSettingsValidation : IValidateOptions<TimescaleDbSettings>
{
    public ValidateOptionsResult Validate(string? name, TimescaleDbSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Connection string must be provided.");
        }

        // Disabled just in case there's some weird working connection string that doesn't match the pattern
        // var connectionStringPattern = @"^Host=.+;Username=.+;Password=.+;Database=.+$";
        // if (!Regex.IsMatch(settings.ConnectionString, connectionStringPattern))
        // {
        //     return ValidateOptionsResult.Fail("Connection string is not in the correct format.");
        // }

        return ValidateOptionsResult.Success;
    }
}
