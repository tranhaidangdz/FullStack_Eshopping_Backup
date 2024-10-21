using Eshopping.Areas.Admin.Repository;
using Eshopping.Models;
using Eshopping.Models.ViewModels;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace Eshopping.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManage;
		private SignInManager<AppUserModel> _signInManager;
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		public AccountController(IEmailSender emailSender,SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManage,DataContext context)
		{
			_emailSender = emailSender;
			_signInManager = signInManager;
			_userManage = userManage;
			_dataContext = context;
		}
		//trả về view đăng nhập TK 
		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl}); // Trả về login view model (username, password)
		}
		//trả về view quên mật khẩu TK 

		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			//ta ktra xem email của user yêu cầu đổi mk có giống vs email yêu cầu ko 
			var checkuser = await _userManage.Users
			.Where(u => u.Email == user.Email)
			.Where(u => u.Token== user.Token).FirstOrDefaultAsync();  //FirstOrDefaultAsync(); lấy ra dòng đầu tiên của dữ liệu thôi 
			//lấy thông tin email và đoạn token 
			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Email không tìm thấy hoặc  token không đúng !";
				return RedirectToAction("Forgot Pass", "Account");
			}
			return View();
		}
		//trả về view tạo mk mới   
		public async Task<IActionResult> ForgetPass(string returnUrl)
		{
			return View(); 
		}


		//CHỨC NĂNG GỬI MAIL QUÊN MK :
		[HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
			if (checkMail == null)
			{
				TempData["error"] = "Email không tồn tại";
				return RedirectToAction("ForgetPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();

				//update token to user
				var receiver = checkMail.Email;
				var subject = "Đổi mật khẩu cho tài khoản " + checkMail.Email;
				var message = "Bấm vào link để đổi mật khẩu : " +
				"<a href='" + $" {Request.Scheme}://{Request.Host}/Account/NewPass" +
				$"?email=" + checkMail.Email + "&token=" + token + "'>";

				await _emailSender.SendEmailAsync(receiver, subject, message);
			}
			TempData["success"] = "Đã gửi email đặt lại mật khẩu đến địa chỉ email của bạn!";
			return RedirectToAction("ForgetPass", "Account");
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if(ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
				if (result.Succeeded)
				{
					return Redirect(loginVM.ReturnUrl ?? "/"); // ??: nếu không cái trước thì cái sau
				}

				ModelState.AddModelError("", "Invalid username or password");
			}
			return View(loginVM);
		}

		public IActionResult Create()
		{
			return View();
		}
		//lịch sử đơn hàng :
		public async  Task<IActionResult> History()
		{
			//goi đơn hàng dựa trên user vừa đăng nhập:

			//ktra ng dùng đã đăng nhập chưa, nếu chưa thì bắt đăng nhập: 
			if((bool)!User.Identity?.IsAuthenticated)
			{
					return RedirectToAction("Login", "Account");
			}
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var Orders = await _dataContext.Orders
            .Where(od => od.UserName == userEmail).OrderBy(od => od.Id).ToListAsync();

			ViewBag.UserEmail = userEmail;
            return View(Orders);
		}

		//chức năng hủy đơn hàng trong checkout: KHI  NGƯỜI DÙNG ĐẶT HÀNG VÀ BẤM CHECKOUT-> NÓ CHUYỂN SANG TRANG HISTORY VÀ NG DÙNG CÓ THỂ XEM NỘI DUNG ĐƠN MÌNH VỪA ĐẶT , NẾU KO ƯNG NGƯỜI DÙNG CÓ THỂ HỦY ĐƠN 
		public async Task<IActionResult> CancelOrder(string ordercode) {
			if ((bool) !User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account");
			}
				try
				{
				var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                    order.Status = 3;  //ktra nếu trạng thái đơn hàng trong file History.cshtml =3 => hủy 
					_dataContext.Update(order);  //lưu trạng thái hủy
					await _dataContext.SaveChangesAsync();  //lưu thay đổi trong csdl 
				}
				catch (Exception ex)
				{
					return BadRequest("An error occurred while canceling the order.");
				}
				return RedirectToAction("History", "Account");
			
		}
[HttpPost]
		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
				IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
				if(result.Succeeded)
				{
					TempData["success"] = "Tạo user thành công";
					return Redirect("/account/login");  //hoặc NẾU ĐĂNG NHẬP THÀNH CÔNG TRẢ VỀ TRANG CHỦ :"/views/home/index.cshtml"
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(user);
		}

		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
			return Redirect(returnUrl);
		}


		
	}
}
