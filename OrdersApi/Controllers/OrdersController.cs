using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll() => Ok(_db.Orders.OrderBy(o => o.Id).ToList());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var order = _db.Orders.Find(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("user/{userId:int}")]
    public IActionResult GetByUser(int userId) =>
        Ok(_db.Orders.Where(o => o.UserId == userId).OrderBy(o => o.Id).ToList());
}
