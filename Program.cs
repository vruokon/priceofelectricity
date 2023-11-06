// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Timers;


ManualResetEvent exitEvent = new ManualResetEvent(false);

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();

if (settings == null)
    throw new Exception("Settings can't be null");

System.Timers.Timer timer = new System.Timers.Timer();
timer.Elapsed += TimerElapsed;

timer.Interval = 60000*5;
timer.AutoReset = true;

timer.Start();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Press Ctrl+C to exit.");
Console.ForegroundColor = ConsoleColor.White;

await CheckElectricityPriceAndSetHueLampColor();

exitEvent.WaitOne();



void TimerElapsed(object? sender, ElapsedEventArgs e)
{
    Task.Run(async () =>
    {
        await CheckElectricityPriceAndSetHueLampColor();
    });
}

async Task CheckElectricityPriceAndSetHueLampColor()
{
    double? priceOfElectricity = await GetPrice();

    if(priceOfElectricity == null)
        throw new Exception( $"Could not get the price of electricity.");

    Console.Write($"[{DateTime.Now.ToString(@"yyyy-MM-dd hh:mm")}] Price of electricity is ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"{priceOfElectricity} ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("c/kWh ");

    HueColor hueColor = GetHueColor(priceOfElectricity.Value, settings);
    await SetHueColor(hueColor, $"http://{settings.HueBridgeIp}/api/{settings.HueUsername}/lights/{settings.HueLampId}/state");
}

static async Task<double?> GetPrice()
{
    using (var httpClient = new HttpClient())
    {
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        string hour = DateTime.Now.Hour.ToString();

        string url = $"https://api.porssisahko.net/v1/price.json?date={date}&hour={hour}";

        HttpResponseMessage response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();

            PriceResponse? priceResponse = JsonSerializer.Deserialize<PriceResponse>(responseContent);

            if (priceResponse == null)
                return null;

            return priceResponse.Price;

        }

        return null;
    }
}

static async Task SetHueColor(HueColor hueColor, string huebridgeUrl)
{
    using (var httpClient = new HttpClient())
    {
        string jsonContent = JsonSerializer.Serialize<HueColor>(hueColor);
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PutAsync(huebridgeUrl, requestContent);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"successfully set a color of huelamp");
            return;
        }

        Console.WriteLine($"Error in setting color of huelamp: statusCode {response.StatusCode}");
    }
}


static HueColor GetHueColor(double priceOfElectricity, Settings settings)
{
    if (priceOfElectricity < settings.LowValue)
        return settings.HueColorSettings.Green;

    if (priceOfElectricity < settings.HighValue)
        return settings.HueColorSettings.Orange;

    return settings.HueColorSettings.Red;
}
