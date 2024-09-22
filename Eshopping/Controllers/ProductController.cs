using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		//tim kiếm sản phẩm:
		public async Task<IActionResult> Search(string searchTerm)
		{
			var products=await _dataContext.Products.Where(p=>p.Name.Contains(searchTerm)||p.Description.Contains(searchTerm)).ToListAsync();  //ta tìm kiếm sp mà trong tên và mô tả của nó có chứa từ khóa tìm kiếm mà ta nhập vào 
			ViewBag.Keyword=searchTerm;  //hiển thị từ khóa tìm kiếm đó ra trang web 
			ViewBag.ProductCount = products.Count;  // Kiểm tra số lượng sản phẩm
			return View(products);
		}
	}
}
