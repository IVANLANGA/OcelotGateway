using Microsoft.EntityFrameworkCore;
using OrdersApi.Models;
using System.Collections.Generic;

namespace OrdersApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
    public DbSet<Order> Orders => Set<Order>();
}
