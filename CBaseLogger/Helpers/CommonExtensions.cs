using Npgsql;
using NpgsqlTypes;

namespace CBaseLogger.Helpers;

public static class CommonExtensions
{
    public static CancellationTokenSource ExtendWithDelayedToken(this CancellationToken cancellationToken, TimeSpan delay)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
            new CancellationTokenSource(delay).Token);
    }
    
    public static void AddNullableFloatMetric(this NpgsqlParameterCollection parameterCollection, string name, float? value)
    {
        if (value.HasValue)
        {
            parameterCollection.AddWithValue(name, value);
        }
        else
        {
            parameterCollection.Add(new NpgsqlParameter(name, NpgsqlDbType.Real) { Value = DBNull.Value });
        }
    }
    
    public static string ToReadableString(this TimeSpan span)
    {
        return string.Format("{0:0} days, {1:00} hours, {2:00} minutes, {3:00} seconds",
            span.Days, span.Hours, span.Minutes, span.Seconds);
    }
}