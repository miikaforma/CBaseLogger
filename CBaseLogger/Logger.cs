using CBaseLogger.CBase;
using CBaseLogger.CBase.Models;
using CBaseLogger.Enums;
using CBaseLogger.Helpers;
using CBaseLogger.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CBaseLogger;

public class Logger
{
    private readonly ILogger<Logger> _logger;
    private readonly ICBaseClient _cBaseClient;
    private readonly AppSettings _appSettings;
    private readonly TimescaleDbSettings _timescaleDbSettings;

    public Logger(ILogger<Logger> logger, ICBaseClient cBaseClient, IOptions<AppSettings> appSettings, IOptions<TimescaleDbSettings> timescaleDbSettings)
    {
        _logger = logger;
        _cBaseClient = cBaseClient;
        _timescaleDbSettings = timescaleDbSettings.Value;
        _appSettings = appSettings.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Logger");
        
        // Execute the first request immediately when the application starts
        await FetchAndLogPvForecast(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Calculate the delay for AbsoluteInterval
                var delay = _appSettings.IntervalType == IntervalType.Absolute
                    ? CalculateDelayForAbsoluteInterval(_appSettings.AbsoluteIntervalStartHour)
                    : TimeSpan.FromMilliseconds(_appSettings.LoggingInterval);

                _logger.LogInformation("Next fetch at {DateTime} which is in {Delay}", DateTimeOffset.UtcNow + delay, delay.ToReadableString());
                await Task.Delay(delay, cancellationToken);

                await FetchAndLogPvForecast(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred executing {FetchAndLogPvForecast}", nameof(FetchAndLogPvForecast));
            }
        }
    }
    
    private async Task FetchAndLogPvForecast(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching PV forecast data");
            
            // Create a CancellationToken that gets canceled after a timeout period
            using var cts =
                cancellationToken.ExtendWithDelayedToken(TimeSpan.FromMilliseconds(_appSettings.Timeout));
            
            var forecastData = await _cBaseClient.GetForecast(cancellationToken: cts.Token);
            if (forecastData != null)
            {
                // Write results to TimescaleDB
                await WriteForecastEntriesToTimescaleDb(forecastData, cancellationToken: cts.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unknown error retrieving PV forecast data");
        }
    }
    
    private async Task WriteForecastEntriesToTimescaleDb(PvForecastEntry[]? forecastEntries, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_timescaleDbSettings.Enabled && forecastEntries != null)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_timescaleDbSettings.ConnectionString);
                    await conn.OpenAsync(cancellationToken);
                    
                    await using var tran = await conn.BeginTransactionAsync(cancellationToken);

                    foreach (var entry in forecastEntries)
                    {
                        var sql = $@"
                            INSERT INTO {_timescaleDbSettings.TableName} (time, temp_avg, wind_avg, cl_tot, cl_low, cl_med, cl_high, prec_amt, s_glob, s_dif, s_dir_hor, s_dir, s_sw_net, solar_angle_vs_panel, albedo, s_glob_pv, s_ground_dif_pv, s_dir_pv, s_dif_pv, pv_po, pv_t, pv_eta)
                            VALUES (@time, @temp_avg, @wind_avg, @cl_tot, @cl_low, @cl_med, @cl_high, @prec_amt, @s_glob, @s_dif, @s_dir_hor, @s_dir, @s_sw_net, @solar_angle_vs_panel, @albedo, @s_glob_pv, @s_ground_dif_pv, @s_dir_pv, @s_dif_pv, @pv_po, @pv_t, @pv_eta)
                            ON CONFLICT (time)
                            DO UPDATE SET
                                temp_avg = @temp_avg, wind_avg = @wind_avg, cl_tot = @cl_tot, cl_low = @cl_low, cl_med = @cl_med, cl_high = @cl_high, prec_amt = @prec_amt, s_glob = @s_glob, s_dif = @s_dif, s_dir_hor = @s_dir_hor, s_dir = @s_dir, s_sw_net = @s_sw_net, solar_angle_vs_panel = @solar_angle_vs_panel, albedo = @albedo, s_glob_pv = @s_glob_pv, s_ground_dif_pv = @s_ground_dif_pv, s_dir_pv = @s_dir_pv, s_dif_pv = @s_dif_pv, pv_po = @pv_po, pv_t = @pv_t, pv_eta = @pv_eta
                        ";

                        await using var cmd = new NpgsqlCommand(sql, conn);
                        entry.AddMetrics(cmd.Parameters);

                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    await tran.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to TimescaleDB");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unknown error while saving data to TimescaleDB");
        }
    }
    
    private TimeSpan CalculateDelayForAbsoluteInterval(int startHour)
    {
        var now = DateTime.UtcNow;
        var nextIntervalStart = new DateTime(now.Year, now.Month, now.Day, startHour, 0, 0);
        while (now > nextIntervalStart)
        {
            nextIntervalStart = nextIntervalStart.AddHours(TimeSpan.FromMilliseconds(_appSettings.LoggingInterval).TotalHours);
        }

        return nextIntervalStart - now;
    }
}