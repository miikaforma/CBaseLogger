using CBaseLogger.Enums;
using Microsoft.Extensions.Options;

namespace CBaseLogger.Settings;

public class CBaseSettings
{
    public const string SectionName = "CBase";

    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public required int PanelQty { get; set; }
    public required int PanelOutput { get; set; }
    public double? InverterCapacity { get; set; }
    public required TrackingOption Tracking { get; set; }
    public required int Slope { get; set; }
    public required int Azimuth { get; set; }
    public required string ApiKey { get; set; }
}

public class CBaseSettingsValidation : IValidateOptions<CBaseSettings>
{
    public ValidateOptionsResult Validate(string? name, CBaseSettings settings)
    {
        // Min/Max validations
        if (settings.Latitude is < -90 or > 90)
        {
            return ValidateOptionsResult.Fail("Latitude must be between -90 and 90.");
        }

        if (settings.Longitude is < -180 or > 180)
        {
            return ValidateOptionsResult.Fail("Longitude must be between -180 and 180.");
        }
        
        if (settings.PanelQty < 1)
        {
            return ValidateOptionsResult.Fail("Panel quantity must be greater than 0.");
        }
        
        if (settings.PanelOutput < 1)
        {
            return ValidateOptionsResult.Fail("Panel output must be greater than 0.");
        }
        
        if (settings.InverterCapacity is < 0)
        {
            return ValidateOptionsResult.Fail("Inverter capacity must be greater than or equal to 0.");
        }
        
        // if (settings.Slope is < 0 or > 90)
        // {
        //     return ValidateOptionsResult.Fail("Slope must be between 0 and 90.");
        // }
        //
        // if (settings.Azimuth is < 0 or > 360)
        // {
        //     return ValidateOptionsResult.Fail("Azimuth must be between 0 and 360.");
        // }
        
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return ValidateOptionsResult.Fail("API key must be provided. More information from https://www.cbase.fi/.");
        }

        // Decimal place validations
        var decimalPlacesLatitude = BitConverter.GetBytes(decimal.GetBits((decimal)settings.Latitude)[3])[2];
        if (decimalPlacesLatitude > 6)
        {
            return ValidateOptionsResult.Fail("Latitude can have up to 6 decimal places.");
        }

        var decimalPlacesLongitude = BitConverter.GetBytes(decimal.GetBits((decimal)settings.Longitude)[3])[2];
        if (decimalPlacesLongitude > 6)
        {
            return ValidateOptionsResult.Fail("Longitude can have up to 6 decimal places.");
        }
        
        var decimalPlacesInverterCapacity = settings.InverterCapacity.HasValue
            ? BitConverter.GetBytes(decimal.GetBits((decimal)settings.InverterCapacity.Value)[3])[2]
            : 0;
        if (decimalPlacesInverterCapacity > 2)
        {
            return ValidateOptionsResult.Fail("Inverter capacity can have up to 2 decimal places.");
        }
                
        // Validate TrackingOption
        if (!Enum.IsDefined(typeof(TrackingOption), settings.Tracking))
        {
            return ValidateOptionsResult.Fail("Invalid TrackingOption value.");
        }
        
        // TrackingOption validation
        switch (settings.Tracking)
        {
            case TrackingOption.FixedAngle:
                if (settings.Slope is < 0 or > 90)
                {
                    return ValidateOptionsResult.Fail("Slope must be between 0 and 90 for FixedAngle tracking.");
                }
                if (settings.Azimuth is < 0 or > 360)
                {
                    return ValidateOptionsResult.Fail("Azimuth must be between 0 and 360 for FixedAngle tracking.");
                }
                break;
            case TrackingOption.YAxis:
                if (settings.Azimuth is < 0 or > 360)
                {
                    return ValidateOptionsResult.Fail("Azimuth must be between 0 and 360 for YAxis tracking.");
                }
                break;
            case TrackingOption.XAxis:
                if (settings.Slope is < 0 or > 90)
                {
                    return ValidateOptionsResult.Fail("Slope must be between 0 and 90 for XAxis tracking.");
                }
                break;
            case TrackingOption.YxAxis:
                break;
        }

        return ValidateOptionsResult.Success;
    }
}
