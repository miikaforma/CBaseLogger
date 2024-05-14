using System.Globalization;
using CBaseLogger.CBase.Models;
using CBaseLogger.Enums;
using CBaseLogger.Settings;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CBaseLogger.CBase;

public class CBaseClient(
    ILogger<CBaseClient> logger,
    IOptions<AppSettings> appSettings,
    IOptionsSnapshot<CBaseSettings> cBaseSettings)
    : ICBaseClient
{
    private const string ApiEndpoint = "https://www.cbase.fi/api/pvfcst_request";
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly CBaseSettings _cBaseSettings = cBaseSettings.Value;
    
    private int _requestCount = 0;
    private DateTime _resetTime = DateTime.UtcNow;

    public async Task<PvForecastEntry[]?> GetForecast(CancellationToken cancellationToken = default)
    {
        var forecastCsv = await GetForecastCsv(cancellationToken);
        return forecastCsv == null ? null : ParseForecastCsv(forecastCsv);
    }
    
    private async Task<string?> GetForecastCsv(CancellationToken cancellationToken)
    {
        if (_appSettings.OfflineMode)
        {
            return await File.ReadAllTextAsync("example.csv", cancellationToken);
        }
        
        // Fetch the forecast data from the server
        return await FetchPvForecast();
    }
    
    private static PvForecastEntry[] ParseForecastCsv(string forecastCsv)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };

        using var reader = new StringReader(forecastCsv);
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<PvForecastEntryMap>();

        var records = new List<PvForecastEntry>();
        while (csv.Read())
        {
            var record = csv.GetRecord<PvForecastEntry>();
            records.Add(record);
        }

        return records.ToArray();
    }
    
    private async Task<string?> FetchPvForecast()
    {
        try
        {
            // Check if the current hour has changed since the last request
            if (_resetTime.Hour != DateTime.UtcNow.Hour)
            {
                // Reset the request count and the reset time
                _requestCount = 0;
                _resetTime = DateTime.UtcNow;
            }

            // If the request count exceeds the limit, stop sending requests
            if (_requestCount >= _appSettings.RateLimitMaxRequestInHour)
            {
                logger.LogWarning("Request limit exceeded. Waiting until the next hour to send more requests");
                await Task.Delay(TimeSpan.FromHours(1) - DateTime.UtcNow.TimeOfDay);
                return null;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            var queryString = SerializeCBaseSettingsToQueryString(_cBaseSettings);
            var finalUrl = $"{ApiEndpoint}?{queryString}";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(finalUrl),
            };
            logger.LogDebug("Fetching new photovoltaic production forecast data from {Url}", finalUrl);
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            logger.LogInformation("New photovoltaic production forecast fetched at {UtcNow}", DateTimeOffset.UtcNow);

            return body;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch new photovoltaic production forecast data");
            return null;
        }
        finally
        {
            // Increment the request count
            _requestCount++;
        }
    }
    
    private static string SerializeCBaseSettingsToQueryString(CBaseSettings settings)
    {
        var parameters = new List<string>
        {
            $"lat={settings.Latitude.ToString(CultureInfo.InvariantCulture)}",
            $"lon={settings.Longitude.ToString(CultureInfo.InvariantCulture)}",
            $"panel_qty={settings.PanelQty}",
            $"panel_out={settings.PanelOutput}",
            $"inv_cap={settings.InverterCapacity?.ToString(CultureInfo.InvariantCulture) ?? "0"}",
            $"tracking={(int)settings.Tracking}"
        };

        switch (settings.Tracking)
        {
            case TrackingOption.FixedAngle:
                parameters.Add($"slope={settings.Slope}");
                parameters.Add($"azi={settings.Azimuth}");
                break;
            case TrackingOption.YAxis:
                parameters.Add($"azi={settings.Azimuth}");
                break;
            case TrackingOption.XAxis:
                parameters.Add($"slope={settings.Slope}");
                break;
            case TrackingOption.YxAxis:
                break;
        }
        
        parameters.Add($"apikey={settings.ApiKey}");

        return string.Join("&", parameters);
    }
}