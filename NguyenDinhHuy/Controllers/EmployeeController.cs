using Microsoft.AspNetCore.Mvc;
using NguyenDinhHuy.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace NguyenDinhHuy.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 0. Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // 1. GET: Tạo đơn xin nghỉ
        public IActionResult CreateLeaveRequest()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            ViewBag.EmployeeCode = user?.EmployeeCode ?? "N/A";
            ViewBag.FullName = user?.FullName ?? "N/A"; // Thêm dòng này
            ViewBag.Department = user?.Department ?? "N/A";

            return View();
        }

        // 1. POST: Gửi đơn xin nghỉ
        [HttpPost]
        public IActionResult CreateLeaveRequest(int? totalDays, string reason)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // Kiểm tra dữ liệu đầu vào
            if (totalDays == null || totalDays <= 0 || string.IsNullOrWhiteSpace(reason))
            {
                ViewBag.Message = "❌ Vui lòng nhập đầy đủ thông tin.";
                ViewBag.EmployeeCode = user?.EmployeeCode ?? "N/A";
                ViewBag.Department = user?.Department ?? "N/A";
                return View();
            }

            var request = new LeaveRequest
            {
                UserId = userId,
                TotalDays = totalDays,
                Reason = reason.Trim(),
                Status = "Chờ duyệt",
                RequestDate = DateTime.Now,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now.AddDays(totalDays.Value)
            };

            _context.LeaveRequests.Add(request);
            _context.SaveChanges();

            ViewBag.Message = "✅ Gửi đơn thành công!";
            ViewBag.EmployeeCode = user?.EmployeeCode ?? "N/A";
            ViewBag.Department = user?.Department ?? "N/A";

            return View();
        }

        // 2. Lịch sử xin nghỉ phép
        public IActionResult LeaveHistory()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");

            var leaveRequests = _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.RequestDate)
                .ToList();

            return View(leaveRequests);
        }

        // 3. Thông tin cá nhân
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(user);
        }

        // 4. Nhận thông báo từ admin
        public IActionResult Notifications()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentDate)
                .ToList();

            return View(notifications);
        }

        // 5. Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
