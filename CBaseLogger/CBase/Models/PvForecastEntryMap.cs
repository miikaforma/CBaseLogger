using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CBaseLogger.CBase.Models;

public class UtcDateTimeConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        return DateTimeOffset.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }
}

public class CustomFloatConverter : CsvHelper.TypeConversion.SingleConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.Equals(text, "NA", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return base.ConvertFromString(text, row, memberMapData);
    }
}

public sealed class PvForecastEntryMap : ClassMap<PvForecastEntry>
{
    public PvForecastEntryMap()
    {
        Map(m => m.Time).Name("Time.UTC").TypeConverter<UtcDateTimeConverter>();
        Map(m => m.TempAvg).Name("temp_avg").TypeConverter<CustomFloatConverter>();
        Map(m => m.WindAvg).Name("wind_avg").TypeConverter<CustomFloatConverter>();
        Map(m => m.ClTot).Name("cl_tot").TypeConverter<CustomFloatConverter>();
        Map(m => m.ClLow).Name("cl_low").TypeConverter<CustomFloatConverter>();
        Map(m => m.ClMed).Name("cl_med").TypeConverter<CustomFloatConverter>();
        Map(m => m.ClHigh).Name("cl_high").TypeConverter<CustomFloatConverter>();
        Map(m => m.PrecAmt).Name("prec_amt").TypeConverter<CustomFloatConverter>();
        Map(m => m.SGlob).Name("s_glob").TypeConverter<CustomFloatConverter>();
        Map(m => m.SDif).Name("s_dif").TypeConverter<CustomFloatConverter>();
        Map(m => m.SDirHor).Name("s_dir_hor").TypeConverter<CustomFloatConverter>();
        Map(m => m.SDir).Name("s_dir").TypeConverter<CustomFloatConverter>();
        Map(m => m.SSwNet).Name("s_sw_net").TypeConverter<CustomFloatConverter>();
        Map(m => m.SolarAngleVsPanel).Name("solar_angle_vs_panel").TypeConverter<CustomFloatConverter>();
        Map(m => m.Albedo).Name("albedo").TypeConverter<CustomFloatConverter>();
        Map(m => m.SGlobPv).Name("s_glob_pv").TypeConverter<CustomFloatConverter>();
        Map(m => m.SGroundDifPv).Name("s_ground_dif_pv").TypeConverter<CustomFloatConverter>();
        Map(m => m.SDirPv).Name("s_dir_pv").TypeConverter<CustomFloatConverter>();
        Map(m => m.SDifPv).Name("s_dif_pv").TypeConverter<CustomFloatConverter>();
        Map(m => m.PvPo).Name("pv_po").TypeConverter<CustomFloatConverter>();
        Map(m => m.PvT).Name("pv_T").TypeConverter<CustomFloatConverter>();
        Map(m => m.PvEta).Name("pv_eta").TypeConverter<CustomFloatConverter>();
    }
}