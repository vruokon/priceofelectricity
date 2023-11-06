// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class PriceResponse
{
    [JsonPropertyName("price")]
    public double Price { get; set; }
}