using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.WriteIndented = true);

var app = builder.Build();

// Fail the first N requests to /data, then succeed.
// Configure via env var FAIL_FIRST (default 2).
var failFirst = int.TryParse(Environment.GetEnvironmentVariable("FAIL_FIRST"), out var n) ? n : 2;
var counter = 0;

app.MapGet("/", () => Results.Ok(new
{
    name = "FlakyApi",
    endpoints = new[] { "/health", "/data" },
    failFirst
}));

app.MapGet("/health", () => Results.Ok(new { ok = true, failFirst }));

app.MapGet("/data", () =>
{
    counter++;

    if (counter <= failFirst)
    {
        return Results.Json(
            new { error = "Simulated transient fault", attempt = counter, suggestedFallback = "retry-with-backoff" },
            statusCode: 503
        );
    }

    return Results.Ok(new
    {
        message = "Success after transient faults",
        attempts = counter,
        failedFirst = failFirst,
        timestamp = DateTimeOffset.UtcNow
    });
});

app.Run();
