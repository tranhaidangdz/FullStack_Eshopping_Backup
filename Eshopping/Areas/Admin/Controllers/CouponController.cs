using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Coupon")]
	[Authorize(Roles = "Admin")]// phân quyền truy cập manage branch 
	public class CouponController : Controller
	{
		private readonly DataContext _dataContext;
		public CouponController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
            var coupon_list=await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons=coupon_list;
			return View();
		}

        //create :
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {

            if (ModelState.IsValid)
            {


                _dataContext.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Add coupon successfully!";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "The model has a few things wrong!";
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
            return View();
        }
    }
}
