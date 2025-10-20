namespace OrdersApi.Models;

public class Order
{
    public int Id { get; set; }
    public string Item { get; set; } = "";
    public decimal Total { get; set; }
    public int UserId { get; set; }
}
