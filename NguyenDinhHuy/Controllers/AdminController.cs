using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhHuy.Models;

namespace NguyenDinhHuy.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === 0. Trang chính của Admin ===
        public IActionResult Dashboard()
        {
            return View();
        }

        // === 1. Danh sách tất cả đơn xin nghỉ phép ===
        public IActionResult LeaveRequests()
        {
            var requests = _context.LeaveRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(requests);
        }

        // === 1.1. Danh sách đơn chờ duyệt ===
        [HttpGet]
        public IActionResult LeaveRequestsToReview()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var pendingRequests = _context.LeaveRequests
                .Include(r => r.User)
                .Where(l => l.Status == "Chờ duyệt")
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(pendingRequests);
        }

        // === 1.2. Duyệt đơn ===
        public IActionResult Approve(int id)
        {
            var request = _context.LeaveRequests.FirstOrDefault(r => r.Id == id);
            if (request != null && request.Status == "Chờ duyệt")
            {
                var balance = _context.LeaveBalances.FirstOrDefault(b => b.UserId == request.UserId && b.Year == DateTime.Now.Year);
                if (balance == null)
                {
                    balance = new LeaveBalance
                    {
                        UserId = request.UserId,
                        Year = DateTime.Now.Year,
                        UsedDays = 0
                    };
                    _context.LeaveBalances.Add(balance);
                    _context.SaveChanges();
                }

                if (balance.UsedDays + request.TotalDays > 24)
                {
                    TempData["Error"] = $"Không thể duyệt đơn. Người dùng đã vượt quá số ngày phép cho phép trong năm.";
                    return RedirectToAction("LeaveRequests");
                }

                request.Status = "Approved";
                balance.UsedDays += request.TotalDays;

                _context.Notifications.Add(new Notification
                {
                    UserId = request.UserId,
                    Message = $"Đơn xin nghỉ từ {request.FromDate:dd/MM/yyyy} đến {request.ToDate:dd/MM/yyyy} đã được duyệt.",
                    IsRead = false,
                    SentDate = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = "Đã duyệt đơn thành công.";
            }

            return RedirectToAction("LeaveRequests");
        }

        // === 1.3. Từ chối đơn ===
        public IActionResult Reject(int id)
        {
            var request = _context.LeaveRequests.FirstOrDefault(r => r.Id == id);
            if (request != null && request.Status == "Chờ duyệt")
            {
                request.Status = "Rejected";

                _context.Notifications.Add(new Notification
                {
                    UserId = request.UserId,
                    Message = $"Đơn xin nghỉ từ {request.FromDate:dd/MM/yyyy} đến {request.ToDate:dd/MM/yyyy} đã bị từ chối.",
                    IsRead = false,
                    SentDate = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = "Đã từ chối đơn.";
            }

            return RedirectToAction("LeaveRequests");
        }

        // === 2. Quản lý tài khoản nhân viên ===
        public IActionResult UserAccounts()
        {
            var users = _context.Users
                .Where(u => u.Role == "Employee")
                .OrderBy(u => u.FullName)
                .ToList();
            return View(users);
        }

        public IActionResult Activate(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsActive = true;
                _context.SaveChanges();
                TempData["Success"] = "Đã kích hoạt tài khoản.";
            }
            return RedirectToAction("UserAccounts");
        }

        // === 3. Quản lý số dư ngày phép ===
        public IActionResult LeaveBalances()
        {
            var balances = _context.LeaveBalances
                .Include(b => b.User)
                .Where(b => b.Year == DateTime.Now.Year)
                .ToList();

            return View(balances);
        }

        // === 3.1. Gửi cảnh báo nếu vượt quá giới hạn 24 ngày ===
        [HttpPost]
        public IActionResult SendWarning(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("LeaveBalances");
            }

            var leaveBalance = _context.LeaveBalances
                .FirstOrDefault(b => b.UserId == userId && b.Year == DateTime.Now.Year);

            if (leaveBalance != null && (leaveBalance.UsedDays ?? 0) >= 12)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = "🔴 Bạn đã dùng hết số ngày xin nghỉ!",
                    IsRead = false,
                    SentDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["Success"] = $"Đã gửi cảnh báo đến nhân viên {user.FullName}.";
            }
            else
            {
                TempData["Error"] = "Nhân viên chưa vượt quá số ngày nghỉ cho phép.";
            }

            return RedirectToAction("LeaveBalances");
        }


        // === 4. Đăng xuất ===
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
