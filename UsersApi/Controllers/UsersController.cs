using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace UsersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDriver _driver;
    public UsersController(IDriver driver) => _driver = driver;

    public record CreateUserDto(int Id, string Name);
    public record UpdateUserDto(string Name);

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

    [HttpPost]  
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        await using var session = _driver.AsyncSession();

        // Prevent duplicate id (parameter name 'id' explicitly)
        var exists = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync("MATCH (u:User {id: $id}) RETURN u LIMIT 1", new { id = dto.Id });
            var list = await cursor.ToListAsync(r => r["u"]);
            return list.Any();
        });

        if (exists) return Conflict(new { message = "User with the same id already exists." });

        var created = await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                CREATE (u:User { id: $id, name: $name })
                RETURN u { .id, .name } AS user
            ", new { id = dto.Id, name = dto.Name });

            var list = await cursor.ToListAsync(r => r["user"]);
            return list.First();
        });

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        await using var session = _driver.AsyncSession();

        var existing = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync("MATCH (u:User {id: $id}) RETURN u { .id, .name } AS user", new { id });
            var list = await cursor.ToListAsync(r => r["user"]);
            return list.FirstOrDefault();
        });

        if (existing is null) return NotFound();

        var updated = await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (u:User {id: $id})
                SET u.name = $name
                RETURN u { .id, .name } AS user
            ", new { id, name = dto.Name });

            var list = await cursor.ToListAsync(r => r["user"]);
            return list.First();
        });

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var session = _driver.AsyncSession();

        var existing = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync("MATCH (u:User {id: $id}) RETURN u { .id, .name } AS user", new { id });
            var list = await cursor.ToListAsync(r => r["user"]);
            return list.FirstOrDefault();
        });

        if (existing is null) return NotFound();

        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync("MATCH (u:User {id: $id}) DETACH DELETE u", new { id });
            return true;
        });

        return NoContent();
    }
}