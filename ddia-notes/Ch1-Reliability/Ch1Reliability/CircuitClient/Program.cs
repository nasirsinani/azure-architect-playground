using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;

var baseUrl = Environment.GetEnvironmentVariable("FLAKY_API_URL") ?? "http://localhost:5080";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

// Circuit opens after 2 handled failures, stays open for 10s, then half-open (one trial).
var circuit = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(10),
        onBreak: (outcome, breakDelay) =>
        {
            var reason = outcome.Exception != null
                ? outcome.Exception.GetType().Name
                : $"{(int)outcome.Result!.StatusCode} {outcome.Result!.ReasonPhrase}";
            Console.WriteLine($"[Circuit] OPEN ({reason}) for {breakDelay.TotalSeconds}s.");
        },
        onReset: () => Console.WriteLine("[Circuit] CLOSED (recovered)."),
        onHalfOpen: () => Console.WriteLine("[Circuit] HALF-OPEN (trial call).")
    );

// A tiny pre-breaker retry to smooth a single blip
var quickRetry = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
    .WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(300),
        onRetryAsync: async (_, __, attempt, ___) =>
        {
            Console.WriteLine($"[Retry] quick retry before breaker (attempt {attempt}).");
            await Task.CompletedTask;
        });

AsyncPolicyWrap<HttpResponseMessage> pipeline = quickRetry.WrapAsync(circuit);

// Make several calls to observe CLOSED → OPEN → HALF-OPEN → CLOSED transitions.
for (var i = 1; i <= 8; i++)
{
    try
    {
        Console.WriteLine($"Call #{i} ...");
        var response = await pipeline.ExecuteAsync(() => http.GetAsync("/data"));

        Console.WriteLine(response.IsSuccessStatusCode
            ? "OK"
            : $"{(int)response.StatusCode} {response.ReasonPhrase}");
    }
    catch (BrokenCircuitException)
    {
        Console.WriteLine("Fast-fail (circuit open).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
    }

    await Task.Delay(1000);
}

Console.ReadKey();
