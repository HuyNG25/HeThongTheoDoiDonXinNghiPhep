using Microsoft.AspNetCore.Mvc;
using NguyenDinhHuy.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NguyenDinhHuy.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai thông tin đăng nhập hoặc tài khoản chưa được kích hoạt.";
                return View();
            }

            // Lưu thông tin vào session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Dashboard", "Employee");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
