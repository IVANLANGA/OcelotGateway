using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Add services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configure SQLite Database (database-per-service pattern) ---
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=orders.db"));

var app = builder.Build();

// --- Seed data (demo) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // create DB file if not exists

    if (!db.Orders.Any())
    {
        db.Orders.AddRange(
            new Order { Id = 101, Item = "Keyboard", Total = 899, UserId = 1 },
            new Order { Id = 102, Item = "Mouse", Total = 399, UserId = 2 }
        );
        db.SaveChanges();
        Console.WriteLine("[Seed] SQLite orders.db seeded successfully.");
    }
}

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Optional: disable if Ocelot uses HTTP
// app.UseHttpsRedirection();

app.UseAuthorization();

// --- Endpoints ---
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "OrdersApi", status = "ok" }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();