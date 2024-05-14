using CBaseLogger.CBase.Models;

namespace CBaseLogger.CBase;

public interface ICBaseClient
{
    Task<PvForecastEntry[]?> GetForecast(CancellationToken cancellationToken = default);
}