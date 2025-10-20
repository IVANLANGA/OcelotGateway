using Microsoft.AspNetCore.Mvc;

namespace UsersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly object[] Data =
    {
        new { Id = 1, Name = "Alice" },
        new { Id = 2, Name = "Bob" }
    };

    // GET /api/users
    [HttpGet]
    public IActionResult GetAll() => Ok(Data);

    // GET /api/users/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var user = Data.Cast<dynamic>().FirstOrDefault(u => (int)u.Id == id);
        return user is null ? NotFound() : Ok(user);
    }
}
