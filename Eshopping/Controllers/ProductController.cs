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
			var productsById = _dataContext.Products.Include(p => p.Ratings).Where(p => p.Id == Id).FirstOrDefault();
			//realated product: sp liên quan:
			var relatedProducts = await _dataContext.Products.Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id).Take(4).ToListAsync();  //lấy những sp có cùng category  với sp A, nhưng lại khác nhau về id (các sp khác nhau, nhưng cùng 1 danh mục category) 

			ViewBag.RelatedProducts = relatedProducts;  //đầy vào 1 CTDL viewBag

			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productsById
			};
			// Trả về view với sản phẩm đã tìm thấy
			return View(viewModel);
		}
		//tim kiếm sản phẩm:ta phải để phương thức httpPost,nếu ko nó mặc định là httpGet. ta để post tức là lấy từ khóa do ng dùng nhập vào 
		[HttpPost]
		public async Task<IActionResult> Search(string searchTerm)
		{
			var products = await _dataContext.Products.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToListAsync();  //ta tìm kiếm sp mà trong tên và mô tả của nó có chứa từ khóa tìm kiếm mà ta nhập vào 
			ViewBag.Keyword = searchTerm;  //hiển thị từ khóa tìm kiếm đó ra trang web 
			ViewBag.ProductCount = products.Count;  // Kiểm tra số lượng sản phẩm
			return View(products);
		}
		//đánh giá sản phẩm:Đoạn code xử lý CommentProduct trong controller của bạn liên quan đến việc người dùng gửi đánh giá sản phẩm.
		//Điều này rõ ràng thuộc về chức năng dành cho người dùng thông thường, vì người dùng là người đánh giá sản phẩm trên website.

		//Vì vậy, bạn nên giữ đoạn code xử lý comment này trong file ProductController dành cho người dùng thông thường, không phải trong phần dành cho admin.
		
				[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (ModelState.IsValid)
			{
				// Nếu RatingModel không phải là một entity, hãy chuyển nó sang entity tương ứng
				var ratingEntity = new RatingModel
				{
					//các đánh giá ng dùng nhập vào sẽ đc gửi vào csdl 
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star
					// Other properties as needed
				};
				_dataContext.Ratings.Add(ratingEntity);  // thêm vào DBSet của entity
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Thêm đánh giá thành công!";

				return Redirect(Request.Headers["Referer"]);
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

				return RedirectToAction("Details",new {id=rating.ProductId});
			}
			return Redirect(Request.Headers["Referer"]);  //nếu đánh giá ko thành công quay lại trang trc đó 
		}
	}
}
