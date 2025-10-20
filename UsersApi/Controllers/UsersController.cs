using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace UsersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDriver _driver;
    public UsersController(IDriver driver) => _driver = driver;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await using var session = _driver.AsyncSession();
        var list = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (u:User)
                RETURN u { .id, .name } AS user
                ORDER BY u.id ASC
            ");
            return await cursor.ToListAsync(r => r["user"]);
        });
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await using var session = _driver.AsyncSession();
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (u:User {id: $id})
                RETURN u { .id, .name } AS user
            ", new { id });

            var list = await cursor.ToListAsync(r => r["user"]);
            return list.FirstOrDefault();
        });

        return result is null ? NotFound() : Ok(result);
    }
}
