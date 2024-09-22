using Eshopping.Models;
using Eshopping.Models.ViewModels;
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
			// Nếu không tìm thấy sản phẩm, chuyển hướng về trang Index
			if (Id == null)
			{
				return RedirectToAction("Index");
			}
			var productsById = _dataContext.Products.Include(p=>p.Ratings).Where(p => p.Id == Id).FirstOrDefault();
			//realated product: sp liên quan:
			var relatedProducts= await _dataContext.Products.Where(p=>p.CategoryId==productsById.CategoryId&&p.Id!=productsById.Id).Take(4).ToListAsync();  //lấy những sp có cùng category  với sp A, nhưng lại khác nhau về id (các sp khác nhau, nhưng cùng 1 danh mục category) 

			ViewBag.RelatedProducts = relatedProducts;  //đầy vào 1 CTDL viewBag

			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productsById,
				RatingDetails=productsById.Ratings
			};
			// Trả về view với sản phẩm đã tìm thấy
			return View(viewModel);
		}
		//tim kiếm sản phẩm:
		public async Task<IActionResult> Search(string searchTerm)
		{
			var products=await _dataContext.Products.Where(p=>p.Name.Contains(searchTerm)||p.Description.Contains(searchTerm)).ToListAsync();  //ta tìm kiếm sp mà trong tên và mô tả của nó có chứa từ khóa tìm kiếm mà ta nhập vào 
			ViewBag.Keyword=searchTerm;  //hiển thị từ khóa tìm kiếm đó ra trang web 
			ViewBag.ProductCount = products.Count;  // Kiểm tra số lượng sản phẩm
			return View(products);
		}
		public IActionResult CommentProduct(RatingModel rating)
		{
			return RedirectToAction("Details","Product");
		}
	}
}
