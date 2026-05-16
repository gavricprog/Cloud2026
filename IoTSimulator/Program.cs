using System.Net.Http.Json;
using System.Text.Json;

var apiBase = "https://localhost:7014";
var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

Console.WriteLine("=== Smart Apiary IoT Simulator ===");
Console.WriteLine();

// Step 1: Serijski broj
Console.Write("Unesite serijski broj (format SA-YYYY-XXXXX, npr. SA-2024-12345): ");
var serialNumber = Console.ReadLine()?.Trim() ?? "SA-2024-12345";

// Step 2: Handshake
Console.WriteLine("\nPokrecem handshake...");
var uniqueDeviceId = Guid.NewGuid().ToString();
var handshakeResponse = await http.PostAsJsonAsync("/api/devices/handshake", new
{
    serialNumber,
    uniqueDeviceId
});

if (!handshakeResponse.IsSuccessStatusCode)
{
    var err = await handshakeResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"Handshake neuspešan: {err}");
    Console.WriteLine("Pritisnite Enter za izlaz...");
    Console.ReadLine();
    return;
}

var handshakeResult = await handshakeResponse.Content.ReadFromJsonAsync<JsonElement>();
var accessToken = handshakeResult.GetProperty("accessToken").GetString()!;
Console.WriteLine($"Uparen! Access Token: {accessToken}");

// Step 3: Istorijski podaci - generisi 7 dana unazad za testiranje
var historyDays = 7;

var rnd = new Random();
double weight = 45.0;
double temperature = 35.0;
double humidity = 60.0;
double battery = 100.0;

async Task SendReading(DateTime recordedAt)
{
    var req = new HttpRequestMessage(HttpMethod.Post, "/api/telemetry/ingest");
    req.Headers.Add("X-Device-Token", accessToken);
    req.Content = JsonContent.Create(new
    {
        weight = Math.Round(weight, 2),
        humidity = Math.Round(humidity, 1),
        internalTemperature = Math.Round(temperature, 1),
        batteryLevel = Math.Round(battery, 1),
        recordedAt = recordedAt
    });
    var res = await http.SendAsync(req);
    var status = res.IsSuccessStatusCode ? "OK" : "GREŠKA";
    Console.WriteLine($"[{recordedAt:dd.MM HH:mm}] Tezina: {weight:F2}kg | Temp: {temperature:F1}°C | Vlaga: {humidity:F1}% | Baterija: {battery:F1}% [{status}]");
}

if (historyDays > 0)
{
    Console.WriteLine($"\nSlanje istorijskih podataka za proslih {historyDays} dana...\n");

    for (int day = historyDays; day >= 1; day--)
    {
        var date = DateTime.UtcNow.Date.AddDays(-day);

        // Jutarnje merenje ~8:00
        weight += rnd.NextDouble() * 0.3 - 0.1;
        temperature += rnd.NextDouble() * 0.4 - 0.2;
        humidity += rnd.NextDouble() * 2 - 1;
        weight = Math.Max(10, weight);
        temperature = Math.Clamp(temperature, 30, 42);
        humidity = Math.Clamp(humidity, 40, 90);
        await SendReading(date.AddHours(8).AddMinutes(rnd.Next(0, 15)));

        // Par merenja tokom dana
        for (int h = 10; h <= 17; h += 2)
        {
            weight += rnd.NextDouble() * 0.15 - 0.05;
            temperature += rnd.NextDouble() * 0.3 - 0.15;
            humidity += rnd.NextDouble() * 1 - 0.5;
            weight = Math.Max(10, weight);
            temperature = Math.Clamp(temperature, 30, 42);
            humidity = Math.Clamp(humidity, 40, 90);
            await SendReading(date.AddHours(h).AddMinutes(rnd.Next(0, 30)));
        }

        // Večernje merenje ~20:00
        weight += rnd.NextDouble() * 0.4 - 0.1;
        temperature += rnd.NextDouble() * 0.4 - 0.2;
        humidity += rnd.NextDouble() * 2 - 1;
        weight = Math.Max(10, weight);
        temperature = Math.Clamp(temperature, 30, 42);
        humidity = Math.Clamp(humidity, 40, 90);
        await SendReading(date.AddHours(20).AddMinutes(rnd.Next(0, 15)));
    }

    Console.WriteLine("\nIstorijski podaci poslati!\n");
}

// Step 4: Live telemetry
// Po specu interval je 1 sat (3600s). Smanjeno na 5s za potrebe testiranja.
var interval = 5;
Console.WriteLine($"\nSlanje live telemetrije svakih {interval}s (testni rezim). Ctrl+C za stop.\n");

while (true)
{
    weight += rnd.NextDouble() * 0.02 - 0.008;
    temperature += rnd.NextDouble() * 0.4 - 0.2;
    humidity += rnd.NextDouble() * 1 - 0.5;
    battery -= rnd.NextDouble() * 0.01;

    weight = Math.Max(10, weight);
    temperature = Math.Clamp(temperature, 30, 42);
    humidity = Math.Clamp(humidity, 40, 90);
    battery = Math.Max(0, battery);

    await SendReading(DateTime.UtcNow);
    await Task.Delay(interval * 1000);
}
