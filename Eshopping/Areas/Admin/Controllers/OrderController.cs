using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly DataContext _dataContext;
		public OrderController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(P => P.Id).ToListAsync());
		}
		
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();

			// Lấy shipping cost
			var ShippingCost = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
			ViewBag.ShippingCost = ShippingCost.ShippingCost;


			return View(DetailsOrder);
		}

		//cập nhật trạng thái sản phẩm: 0=đã xử lý, 1=đơn hàng mới
		[HttpPost]
		[Route("UpdateOrder")]
		public async Task<IActionResult> UpdateOrder(string ordercode,int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o=>o.OrderCode == ordercode);  //lấy ra sp có ordercode giống ordercode mình nhập vào 
			if (order == null)
			{
				return NotFound();
			}
			order.Status = status;
			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công !" });
			}catch(Exception ex)
			{
				return StatusCode(500, "An error orcurred while updating the order status!");
			}
		}

	}
}