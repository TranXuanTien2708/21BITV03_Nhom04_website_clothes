using Microsoft.AspNetCore.Mvc; // Sử dụng không gian tên ASP.NET Core MVC để truy cập Controller và IActionResult.
using System.Security.Claims; // Cung cấp quyền truy cập vào các claim của người dùng, bao gồm cả vai trò.
using System.Linq; // Sử dụng LINQ để dễ dàng truy vấn các claim của người dùng.

namespace _21BITV03_Nhom04_website_clothes.Helper
{
    public static class NavigationHelper
    {
        // Phương thức này xử lý logic chuyển hướng dựa trên vai trò của người dùng.
        public static IActionResult RedirectToRoleBasedPage(Controller controller)
        {
            // Kiểm tra xem người dùng đã được xác thực (đăng nhập) hay chưa
            if (!controller.User.Identity.IsAuthenticated)
            {
                return null; // Nếu người dùng chưa xác thực, không có chuyển hướng nào được thực hiện.
            }

            // Lấy claim đầu tiên có kiểu Role từ danh sách claim của người dùng
            var roleClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var roleName = roleClaim?.Value; // Lấy tên vai trò nếu có

            // Nếu người dùng không có vai trò nào được gán, không thực hiện chuyển hướng
            if (string.IsNullOrEmpty(roleName))
            {
                return null; // Không chuyển hướng cho người dùng không có vai trò
            }

            // Lấy tên của controller hiện tại mà người dùng đang truy cập
            var currentController = controller.ControllerContext.ActionDescriptor.ControllerName;

            // Kiểm tra nếu người dùng là Admin và hiện không ở trong controller Admin
            if (roleName == "Admin" && currentController != "Admin")
            {
                // Chuyển hướng người dùng Admin đến controller Admin nếu họ đang ở trang khác
                return controller.RedirectToAction("Index", "Admin"); 
            }
            else
            {
                // Người dùng không phải Admin hoặc người dùng Admin đã ở trong controller Admin sẽ không bị chuyển hướng
                return null; // Không chuyển hướng cho người dùng không phải Admin hoặc Admin đã ở trong khu vực Admin
            }
        }
    }
}
