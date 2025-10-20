using Microsoft.AspNetCore.Mvc;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly object[] Data =
    {
        new { Id = 101, UserId = 1, Item = "Keyboard", Total = 899.00 },
        new { Id = 102, UserId = 2, Item = "Mouse",    Total = 399.00 }
    };

    // GET /api/orders
    [HttpGet]
    public IActionResult GetAll() => Ok(Data);

    // GET /api/orders/user/1
    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUser(int userId)
        => Ok(Data.Where(o => (int)o.GetType().GetProperty("UserId")!.GetValue(o)! == userId));
}
