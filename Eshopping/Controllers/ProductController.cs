using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Eshopping.Controllers
{
    public class ProductController : Controller
    {
		private readonly DataContext _dataContext;
		public ProductController(DataContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Details(int Id)
		{
			var productsById = _dataContext.Products.Where(p => p.Id == Id).FirstOrDefault();
			// Nếu không tìm thấy sản phẩm, chuyển hướng về trang Index
			if (productsById == null)
			{
				return RedirectToAction("Index");
			}
			// Trả về view với sản phẩm đã tìm thấy
			return View(productsById);
		}
	}
}
