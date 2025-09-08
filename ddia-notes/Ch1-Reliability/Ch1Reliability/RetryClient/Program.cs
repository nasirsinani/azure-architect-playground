using Polly;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = Environment.GetEnvironmentVariable("FLAKY_API_URL") ?? "http://localhost:5080";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

if (!await WaitForApiAsync(http))
{
    Console.WriteLine($"\nAPI not reachable at {baseUrl}");
    Console.WriteLine("Fix: start it with  ->  dotnet run --project FlakyApi --urls http://localhost:5080");
    return;
}

// --- Policy: retry exceptions and 5xx with exponential backoff ---
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2,4,8
        onRetryAsync: async (outcome, delay, attempt, _) =>
        {
            var reason = outcome.Exception != null
                ? outcome.Exception.GetType().Name
                : $"{(int)outcome.Result!.StatusCode} {outcome.Result!.ReasonPhrase}";
            Console.WriteLine($"[RetryClient] Fault observed (attempt {attempt}): {reason}. Backing off {delay.TotalSeconds:0}s…");
            await Task.CompletedTask;
        });

try
{
    Console.WriteLine("=== RetryClient story ===");
    Console.WriteLine($"Base URL: {baseUrl}");
    Console.WriteLine("1) Call /data on a flaky dependency.");
    Console.WriteLine("2) Observe faults (503/exception).");
    Console.WriteLine("3) Apply retry-with-exponential-backoff so the USER never sees a failure.\n");

    var response = await retryPolicy.ExecuteAsync(() => http.GetAsync("/data"));

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"USER-FACING FAILURE: {(int)response.StatusCode} {response.ReasonPhrase}");
        return;
    }

    var payload = await response.Content.ReadFromJsonAsync<object>();
    Console.WriteLine("\nUSER-FACING SUCCESS");
    Console.WriteLine("   The fault never became a failure — thanks to our retry policy.");
    Console.WriteLine("   Payload:");
    Console.WriteLine(JsonSerializer.Serialize(
        payload,
        options: new JsonSerializerOptions { WriteIndented = true }
    ));
}
catch (Exception ex)
{
    Console.WriteLine($"\nUSER-FACING FAILURE after all retries: {ex.Message}");
}

static async Task<bool> WaitForApiAsync(HttpClient http)
{
    // quick readiness probe: 5 tries, 500ms apart, 1s timeout each
    for (int i = 1; i <= 5; i++)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var resp = await http.GetAsync("/health", cts.Token);
            if (resp.IsSuccessStatusCode) return true;
        }
        catch { /* swallow; we will retry */ }
        await Task.Delay(1000);
    }
    return false;
}

Console.ReadKey();
