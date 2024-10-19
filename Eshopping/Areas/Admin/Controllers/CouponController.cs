using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Coupon")]
	//[Authorize(Roles = "Admin")]// phân quyền truy cập manage branch 
	public class CouponController : Controller
	{
		private readonly DataContext _dataContext;
		public CouponController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
			return View();
		}
	}
}
