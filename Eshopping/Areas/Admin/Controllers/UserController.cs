using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/User")]
	public class UserController : Controller
	{
		private readonly UserManager<AppUserModel> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;
		public UserController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_dataContext = context;
		}
		[HttpGet]
		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			var usersWithRoles = await (from u in _dataContext.Users
										join ur in _dataContext.UserRoles on u.Id equals ur.UserId
										join r in _dataContext.Roles on ur.RoleId equals r.Id
										select new { User = u, RoleName = r.Name }).ToListAsync();
			return View(usersWithRoles);
		}

		[HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(new AppUserModel());
		}
		//khi ta ấn nút edit, nó sẽ trả về thông tin ban đầu của user đó(thông qua id): chính là hàm này 
		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string id)  //lấy id của user đó để thực hiện eidt 
		{
			//ktra nếu id của user đó ko có => trả về trang 404 not fond
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);
		}

		//tao user 
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Create")]
		public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				var createuserResult = await _userManager.CreateAsync(user, user.PasswordHash); // Tao user
				if (createuserResult.Succeeded)
				{
					var createUser = await _userManager.FindByEmailAsync(user.Email); // Tim user dua tren email
					var userId = createUser.Id; // Lay user id
					var role = _roleManager.FindByIdAsync(user.RoleId); // Lay role id


					// Gan quyen truy cap
					var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);

					if (!addToRoleResult.Succeeded)
					{
						foreach (var error in createuserResult.Errors)
						{
							ModelState.AddModelError(string.Empty, error.Description);
						}
					}

					return RedirectToAction("Index", "User");
				}
				else
				{
					foreach (var error in createuserResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View(user);
				}
			}
			else
			{
				TempData["error"] = "Model có 1 vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}

			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);
		}
		//xoa user 

		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string id)
		{
			//ktra nếu id của user đó ko có => trả về trang 404 not fond
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var deleteResult = await _userManager.DeleteAsync(user);
			//neu xoa ko thanh cong 
			if (!deleteResult.Succeeded)
			{
				return View("Error");
			}
			TempData["success"] = "User đã xóa thành công ";
			return RedirectToAction("Index");
		}
		//edit user:khi ta nhấn nút update thông tin mới 
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string id, AppUserModel user)
		{
			var existingUser = await _userManager.FindByIdAsync(id);  //lấy ra user dựa vào id 
			if (existingUser == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)  //nếu tất cả các dòng dữu liệu trả về từ user đó  ok hết => ta bắt đầu chỉnh sửa 
			{
				// Update other user properties (excluding password):thay the du lieu cu=du lieu ta nhap vao 
				existingUser.UserName = user.UserName;
				existingUser.Email = user.Email;
				existingUser.PhoneNumber = user.PhoneNumber;
				existingUser.RoleId = user.RoleId;
				var updateUserResult = await _userManager.UpdateAsync(existingUser);
				if (updateUserResult.Succeeded)
				{
					return RedirectToAction("Index", "User");
				}
				else
				{
					AddIdentityErrors(updateUserResult);
					return View(existingUser);
				}
			}
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");

			TempData["error"] = "Model có 1 vài thứ đang bị lỗi";
			var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();

			string errorMessage = string.Join("\n", errors);
			return View(existingUser);
		}
		private void AddIdentityErrors(IdentityResult identityResult)
		{
			foreach (var error in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

	}



}

