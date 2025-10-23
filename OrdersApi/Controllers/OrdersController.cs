using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data;
using OrdersApi.Models;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    public record CreateOrderDto(string Item, decimal Total, int UserId);
    public record UpdateOrderDto(string Item, decimal Total, int UserId);

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

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderDto dto)
    {
        if (dto is null) return BadRequest();

        var order = new Order
        {
            Item = dto.Item ?? string.Empty,
            Total = dto.Total,
            UserId = dto.UserId
        };

        try
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create order: {ex}");
            return StatusCode(500, new { error = "Failed to create order" });
        }

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateOrderDto dto)
    {
        if (dto is null) return BadRequest();

        var order = _db.Orders.Find(id);
        if (order is null) return NotFound();

        order.Item = dto.Item ?? order.Item;
        order.Total = dto.Total;
        order.UserId = dto.UserId;

        try
        {
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to update order {id}: {ex}");
            return StatusCode(500, new { error = "Failed to update order" });
        }

        return NoContent();     
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var order = _db.Orders.Find(id);
        if (order is null) return NotFound();

        try
        {
            _db.Orders.Remove(order);
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to delete order {id}: {ex}");
            return StatusCode(500, new { error = "Failed to delete order" });
        }

        return NoContent();
    }
}
