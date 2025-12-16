using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestWebApp.Data;

namespace TestWebApp.Pages;

[Authorize]
public class DbTestModel : PageModel
{
    private readonly AppDbContext _db;
    public List<TestItem> Items { get; private set; } = new();

    public DbTestModel(AppDbContext db) => _db = db;

    public void OnGet()
    {
        Items = _db.TestItems.ToList();
    }
}