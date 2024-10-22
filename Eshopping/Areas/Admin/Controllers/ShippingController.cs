using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Shipping")]
	[Authorize(Roles= "Admin")]
	public class ShippingController : Controller
	{
        private readonly DataContext _dataContext;
        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }
		[Route("Index")]
        public async Task<IActionResult> Index()
		{
			var shippingList=await _dataContext.Shippings.ToListAsync();
			ViewBag.Shippings=shippingList;
			return View();
		}
		[HttpPost]
		[Route("StoreShipping")]
		public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
		{
			shippingModel.City = tinh;
			shippingModel.District = quan;
			shippingModel.Ward = phuong;
			shippingModel.Price = price;

			try
			{
				//lấy dữ liệu địa chỉ ng dùng nhập 
				var existingShipping = await _dataContext.Shippings.AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
				//kiểm tra xem nó có data chưa(có trả về tỉnh, quận, phương )ko 
				if (existingShipping)
				{
					return Ok(new { duplicate = true, message = "Dữ liệu trùng lặp!" });
				}
				_dataContext.Shippings.Add(shippingModel);
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Thêm shipping thành công !" });

			}
			catch (Exception)
			{
				return StatusCode(500, "Có lỗi khi thêm shipping!");
			}
		}
		public async Task<IActionResult> Delete(int Id)
		{
			ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);  //tìm đchi shipping dựa vào id của nó
			_dataContext.Shippings.Remove(shipping);  //xóa 
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Địa chỉ shipping đã được xóa thành công!";
			return RedirectToAction("Index","Shipping");  //trả về action index của folder shipping 
		}
	}
}
