using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Index(String Slug = "Macbook", string sort_by = "", string startprice = "", string endprice = "")
		{

			CategoryModel category = _dataContext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();
			if (category == null)
				return RedirectToAction("Home", "Index");

			ViewBag.Slug = Slug;
			// Lấy tất cả sản phẩm 
			IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);
			var count = await productsByCategory.CountAsync();

			if (count > 0)
			{
				// Sắp xếp giá tăng dần

				if (sort_by == "price_increase")
				{
					productsByCategory = productsByCategory.OrderBy(p => p.Price);
				}

				// Sắp xếp giá giảm dần
				else if (sort_by == "price_decrease")
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
				}

				// Sắp xếp sp mới nhất dựa vào id(id càng cao thì sẽ là sp mới)
				else if (sort_by == "price_newest")
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
				}

				// Sắp xếp sp mới nhất dựa vào id(id càng thấp thì sẽ là sp mới)
				else if (sort_by == "price_oldest")
				{
					productsByCategory = productsByCategory.OrderBy(p => p.Id);
				}

				else if (startprice != "" && endprice != "")
				{
					decimal startPriceValue;
					decimal endPriceValue;

					// Đổi  giá  truyền vào sang số thực
					if (decimal.TryParse(startprice, out startPriceValue) &&
						decimal.TryParse(endprice, out endPriceValue))
					{
						productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
					}
					else
					{
						productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
					}
				}

				// Mặc định sẽ săp xếp theo id mới nhất
				else
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
				}
			}



			return View(await productsByCategory.ToListAsync());
		}
	}
}
