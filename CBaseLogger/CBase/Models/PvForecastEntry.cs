using System.ComponentModel.DataAnnotations.Schema;
using CBaseLogger.Helpers;
using Npgsql;

namespace CBaseLogger.CBase.Models;

/// <summary>
/// Represents a forecast entry for a photovoltaic (PV) system.
/// </summary>
public class PvForecastEntry
{
    /// <summary>
    /// Timestamp (UTC) - the hourly averages refer to the hour preceding the timestamp unless otherwise stated.
    /// </summary>
    [Column("time")]
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Air temperature, the average of two consecutive instantaneous hourly readings (C).
    /// </summary>
    [Column("temp_avg")]
    public float? TempAvg { get; set; }

    /// <summary>
    /// Wind speed, the average of two consecutive instantaneous hourly readings (m/s).
    /// </summary>
    [Column("wind_avg")]
    public float? WindAvg { get; set; }

    /// <summary>
    /// Total cloudiness, the moment corresponding to the timestamp (%).
    /// </summary>
    [Column("cl_tot")]
    public float? ClTot { get; set; }

    /// <summary>
    /// Low clouds, the moment corresponding to the timestamp (%).
    /// </summary>
    [Column("cl_low")]
    public float? ClLow { get; set; }

    /// <summary>
    /// Medium clouds, the moment corresponding to the timestamp (%).
    /// </summary>
    [Column("cl_med")]
    public float? ClMed { get; set; }

    /// <summary>
    /// High clouds, the moment corresponding to the timestamp (%).
    /// </summary>
    [Column("cl_high")]
    public float? ClHigh { get; set; }

    /// <summary>
    /// Precipitation amount, accumulation over the forecast period (mm).
    /// </summary>
    [Column("prec_amt")]
    public float? PrecAmt { get; set; }

    /// <summary>
    /// Global radiation on a horizontal surface (W/m2).
    /// </summary>
    [Column("s_glob")]
    public float? SGlob { get; set; }

    /// <summary>
    /// Diffuse radiation on a horizontal surface (W/m2).
    /// </summary>
    [Column("s_dif")]
    public float? SDif { get; set; }

    /// <summary>
    /// Direct radiation on a horizontal surface (W/m2).
    /// </summary>
    [Column("s_dir_hor")]
    public float? SDirHor { get; set; }

    /// <summary>
    /// Direct radiation, DNI (W/m2).
    /// </summary>
    [Column("s_dir")]
    public float? SDir { get; set; }

    /// <summary>
    /// Shortwave net radiation (W/m2).
    /// </summary>
    [Column("s_sw_net")]
    public float? SSwNet { get; set; }

    /// <summary>
    /// Angle between the solar panel normal and the sun, at the midpoint of the previous hour (degrees).
    /// </summary>
    [Column("solar_angle_vs_panel")]
    public float? SolarAngleVsPanel { get; set; }

    /// <summary>
    /// Ground albedo.
    /// </summary>
    [Column("albedo")]
    public float? Albedo { get; set; }

    /// <summary>
    /// Global radiation on the solar panel surface (W/m2).
    /// </summary>
    [Column("s_glob_pv")]
    public float? SGlobPv { get; set; }

    /// <summary>
    /// Reflected radiation from the ground on the solar panel surface (W/m2).
    /// </summary>
    [Column("s_ground_dif_pv")]
    public float? SGroundDifPv { get; set; }

    /// <summary>
    /// Direct radiation on the solar panel surface (W/m2).
    /// </summary>
    [Column("s_dir_pv")]
    public float? SDirPv { get; set; }

    /// <summary>
    /// Diffuse radiation on the solar panel surface (W/m2).
    /// </summary>
    [Column("s_dif_pv")]
    public float? SDifPv { get; set; }

    /// <summary>
    /// PV system production (W).
    /// </summary>
    [Column("pv_po")]
    public float? PvPo { get; set; }

    /// <summary>
    /// Temperature of the PV system panels (C).
    /// </summary>
    [Column("pv_T")]
    public float? PvT { get; set; }

    /// <summary>
    /// Nominal efficiency of the PV system (compared to STC conditions).
    /// </summary>
    [Column("pv_eta")]
    public float? PvEta { get; set; }
    
    public void AddMetrics(NpgsqlParameterCollection parameterCollection)
    {
        parameterCollection.AddWithValue("time", Time);
        parameterCollection.AddNullableFloatMetric("temp_avg", TempAvg);
        parameterCollection.AddNullableFloatMetric("wind_avg", WindAvg);
        parameterCollection.AddNullableFloatMetric("cl_tot", ClTot);
        parameterCollection.AddNullableFloatMetric("cl_low", ClLow);
        parameterCollection.AddNullableFloatMetric("cl_med", ClMed);
        parameterCollection.AddNullableFloatMetric("cl_high", ClHigh);
        parameterCollection.AddNullableFloatMetric("prec_amt", PrecAmt);
        parameterCollection.AddNullableFloatMetric("s_glob", SGlob);
        parameterCollection.AddNullableFloatMetric("s_dif", SDif);
        parameterCollection.AddNullableFloatMetric("s_dir_hor", SDirHor);
        parameterCollection.AddNullableFloatMetric("s_dir", SDir);
        parameterCollection.AddNullableFloatMetric("s_sw_net", SSwNet);
        parameterCollection.AddNullableFloatMetric("solar_angle_vs_panel", SolarAngleVsPanel);
        parameterCollection.AddNullableFloatMetric("albedo", Albedo);
        parameterCollection.AddNullableFloatMetric("s_glob_pv", SGlobPv);
        parameterCollection.AddNullableFloatMetric("s_ground_dif_pv", SGroundDifPv);
        parameterCollection.AddNullableFloatMetric("s_dir_pv", SDirPv);
        parameterCollection.AddNullableFloatMetric("s_dif_pv", SDifPv);
        parameterCollection.AddNullableFloatMetric("pv_po", PvPo);
        parameterCollection.AddNullableFloatMetric("pv_t", PvT);
        parameterCollection.AddNullableFloatMetric("pv_eta", PvEta);
    }
}
