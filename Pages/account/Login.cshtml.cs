using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EasyMessenger.Pages.account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; }

        public IActionResult OnPost()
        {
            if (this.Credential.UserName == "admin" && this.Credential.Password == "admin")
            {
                return RedirectToPage("/index"); // Добавьте return здесь
            }

            // Если аутентификация не удалась, можно вернуть ту же страницу с ошибкой
            return Page();
        }
    }
    public class Credential
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
