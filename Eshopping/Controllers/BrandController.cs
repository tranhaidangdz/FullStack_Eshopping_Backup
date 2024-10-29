using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Controllers
{
    public class BrandController:Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

		public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
		{
			// Tìm thương hiệu dựa trên Slug
			BrandModel brand = _dataContext.Brands.FirstOrDefault(c => c.Slug == Slug);

			// Nếu không tìm thấy thương hiệu, trả về trang BrandNotFound
			if (brand == null)
				return View("BrandNotFound"); // Đảm bảo View này tồn tại trong thư mục Views/Brand hoặc Shared

			// Tìm danh sách sản phẩm dựa trên BrandId
			var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);

			// Trả về View Index với danh sách sản phẩm
			return View(await productsByBrand.ToListAsync());
		}



	}
}
