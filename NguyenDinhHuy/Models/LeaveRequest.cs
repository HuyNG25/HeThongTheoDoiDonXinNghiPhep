using System;

namespace NguyenDinhHuy.Models
{
    public partial class LeaveRequest
    {
        public int Id { get; set; }

        public int? UserId { get; set; }                // Khóa ngoại đến User

        public DateTime? FromDate { get; set; }         // Ngày bắt đầu nghỉ
        public DateTime? ToDate { get; set; }           // Ngày kết thúc nghỉ

        public int? TotalDays { get; set; }             // Số ngày nghỉ (nullable để tránh lỗi null)

        public string? LeaveType { get; set; }          // Loại nghỉ phép

        public string? Reason { get; set; }             // Lý do nghỉ

        public string? Status { get; set; }             // Trạng thái: "Chờ duyệt", "Đã duyệt", "Từ chối"

        public DateTime? RequestDate { get; set; }      // Ngày gửi đơn
        public DateTime? ApprovedDate { get; set; }     // Ngày xét duyệt (nếu có)

        public virtual User? User { get; set; }         // Dữ liệu người dùng

        // Xóa StartDate, EndDate, EmployeeId vì đã có FromDate, ToDate, UserId
    }
}
