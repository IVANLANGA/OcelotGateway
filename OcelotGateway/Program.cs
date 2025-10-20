using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Register Ocelot and enable the Polly provider for QoS
builder.Services.AddOcelot().AddPolly();

var app = builder.Build();


app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next();
    sw.Stop();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {ctx.Request.Method} {ctx.Request.Path} → {ctx.Response.StatusCode} in {sw.ElapsedMilliseconds}ms");
});

// health endpoints already mapped:
app.MapGet("/", () => Results.Ok(new { service = "Ocelot Gateway", status = "ok" }));

var securityEnabled = builder.Configuration.GetValue<bool>("Security:Enabled", false);
var apiKey = builder.Configuration["Security:ApiKey"];

app.Use(async (ctx, next) =>
{
    // Allow health without key
    if (ctx.Request.Path == "/" || ctx.Request.Path.StartsWithSegments("/healthz"))
    {
        await next();
        return;
    }

    if (!ctx.Request.Headers.TryGetValue("x-api-key", out var key) || key != apiKey)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await ctx.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next();
    sw.Stop();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {ctx.Request.Method} {ctx.Request.Path} → {ctx.Response.StatusCode} in {sw.ElapsedMilliseconds}ms");
});

// ----------------------
// Simple Service Registry Demo
// ----------------------
var services = new[]
{
    new { Name = "UsersApi", Url = "https://localhost:7140", Health = "Healthy" },
    new { Name = "OrdersApi", Url = "http://localhost:5201", Health = "Healthy" }
};

app.MapGet("/registry", () => Results.Ok(services));


await app.UseOcelot();

app.Run();
