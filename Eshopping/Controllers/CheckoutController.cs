using Eshopping.Areas.Admin.Repository;
using Eshopping.Models;
using Eshopping.Models.ViewModels;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.Xml;

namespace Eshopping.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;  //gọi đối tg này từ lớp IEmailSender để ta gửi xd phương thức gửi mail 

		public CheckoutController(IEmailSender emailSender,DataContext context)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}

		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				// Thêm dữ liệu vào bảng Orders
				var ordercode = Guid.NewGuid().ToString(); // Dùng để random mã đơn hàng 
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;
				orderItem.UserName = userEmail;
				orderItem.Status = 1; // 1 có nghĩa là đơn hàng mới
				orderItem.CreateDate = DateTime.Now;
				_dataContext.Add(orderItem); // Thêm dữ liệu
				_dataContext.SaveChanges();

				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach (var cart in cartItems)
				{
					var orderdetails = new OrderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = ordercode;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Price = cart.Price;
					orderdetails.Quantity = cart.Quantity;

					// Update product quantity
					//Kiểm tra giá trị của cart.ProductId trước khi thực hiện truy vấn để đảm bảo rằng sản phẩm tồn tại trong bảng Products.
					//Thêm xử lý ngoại lệ khi không tìm thấy sản phẩm bằng cách sử dụng phương thức FirstOrDefaultAsync() thay vì FirstAsync().Phương thức này sẽ trả về null nếu không tìm thấy sản phẩm, và bạn có thể kiểm tra điều này để tránh lỗi:
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstOrDefaultAsync();
					if (product == null)
					{
						TempData["error"] = "Sản phẩm không tồn tại trong hệ thống";
						return RedirectToAction("Index", "Cart");
					}

					product.Quantity -= cart.Quantity; // trừ đi số sản phẩm đã mua 
					product.Sold += cart.Quantity; // cộng thêm sản phẩm đã bán
					_dataContext.Update(product);
					//
					_dataContext.Add(orderdetails);
				}
				// Lưu tất cả thay đổi
				_dataContext.SaveChanges();
				// Xóa giỏ hàng khỏi session sau khi đặt hàng thành công
				HttpContext.Session.Remove("Cart");
				//sau khi đặt hàng và bấm checkout , ta sẽ xóa đi đơn hàng đã đặt và gửi mail cho khách hàng bằng cái email trandang ở file Emailseder 
				var receiver = userEmail;  //email nhận, nó đc gửi đi từ email trandang211, sau này ta sẽ thay email nhận này = email ng dùng nhập vào 
				var subject = "Đặt hàng thành công";  //tiêu đề
				var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé ";  //ND thông báo khi chạy ctrinh, nó gửi mail với ND này cho ng đã mua hàng 

				await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "Checkout thành công, vui lòng chờ đơn hàng được duyệt";
				return View("Index", "Cart");

			}
			return View();
		}
	}
}
