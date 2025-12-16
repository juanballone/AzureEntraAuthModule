using Microsoft.EntityFrameworkCore;

namespace TestWebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<TestItem> TestItems => Set<TestItem>();
}

public class TestItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}