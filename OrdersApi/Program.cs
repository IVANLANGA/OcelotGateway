using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var neo4jCfg = builder.Configuration.GetSection("Neo4j");
builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(
        neo4jCfg["Uri"],
        AuthTokens.Basic(neo4jCfg["Username"], neo4jCfg["Password"])
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "OrdersApi", status = "ok" }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
// For local dev you can disable the redirect if it confuses testing:
// app.UseHttpsRedirection();


app.Run();
