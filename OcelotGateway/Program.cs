using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();


app.MapGet("/", () => Results.Ok(new { service = "Ocelot Gateway", status = "ok" }));

await app.UseOcelot();

app.Run();
