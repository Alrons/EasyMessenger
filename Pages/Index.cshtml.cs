using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EasyMessenger.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public string UserEmail { get; private set; } = "";

        public void OnGet()
        {
            
        }
    }
}
