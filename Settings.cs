// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class Settings
{
    public string HueUsername { get; set; } = null!;
    public double LowValue { get; set; }
    public double HighValue { get; set; }
    public string HueBridgeIp { get; set; } = null!;
    public int HueLampId { get; set; }
    public HueColorSettings HueColorSettings { get; set; } = null!;
}

public class HueColorSettings
{
    public HueColor Green { get; set; } = null!;
    public HueColor Orange { get; set; } = null!;
    public HueColor Red { get; set; } = null!;
}

public class HueColor
{
    [JsonPropertyName("hue")]
    public int Hue { get; set; }
    [JsonPropertyName("on")]
    public bool On { get; set; }
    [JsonPropertyName("bri")]
    public int Bri { get; set; }

}
