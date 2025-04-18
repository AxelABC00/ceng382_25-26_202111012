using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorApp.Models;
using System.Text.Json;

namespace MyRazorApp.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/data/users.json");

            if (!System.IO.File.Exists(filePath))
            {
                ErrorMessage = "User data not found.";
                return Page();
            }

            var usersJson = System.IO.File.ReadAllText(filePath);
            var users = JsonSerializer.Deserialize<List<User>>(usersJson);

            var user = users.FirstOrDefault(u => 
                u.Username == Username && 
                u.Password == Password && 
                u.IsActive);

            if (user != null)
            {
                var token = Guid.NewGuid().ToString();

                // Store in session
                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("token", token);
                HttpContext.Session.SetString("session_id", HttpContext.Session.Id);

                // Store in cookies
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("username", user.Username, options);
                Response.Cookies.Append("token", token, options);
                Response.Cookies.Append("session_id", HttpContext.Session.Id, options);

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}
