using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestWebApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public List<string> Groups { get; private set; } = new();

    public void OnGet()
    {
        Groups = User.FindAll("groups").Select(c => c.Value).ToList();
    }
}
