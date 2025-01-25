using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;

namespace EasyMessenger.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var token = HttpContext.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("account/Login");
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Проверка срока действия токена
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return RedirectToPage("account/Login");
                }
            }
            catch
            {
                return RedirectToPage("account/Login");
            }

            return Page();
        }


    }
}
