using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDriver _driver;
    private readonly string _db;
    public OrdersController(IDriver driver, IConfiguration cfg)
    {
        _driver = driver;
        _db = cfg.GetValue<string>("Neo4j:Database") ?? "neo4j";
    }

    // GET /api/orders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var list = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (o:Order)
                RETURN o { .id, .item, .total } AS order
                ORDER BY o.id ASC
            ");
            return await cursor.ToListAsync(r => r["order"]);
        });
        return Ok(list);
    }

    // GET /api/orders/101
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (o:Order {id: $id})
                RETURN o { .id, .item, .total } AS order
            ", new { id });
            var list = await cursor.ToListAsync(r => r["order"]);
            return list.FirstOrDefault();
        });
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/orders/user/1  → all orders placed by user 1
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_db));
        var list = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (:User {id: $userId})-[:PLACED]->(o:Order)
                RETURN o { .id, .item, .total } AS order
                ORDER BY o.id ASC
            ", new { userId });
            return await cursor.ToListAsync(r => r["order"]);
        });
        return Ok(list);
    }
}
