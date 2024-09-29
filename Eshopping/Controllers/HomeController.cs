using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Eshopping.Controllers
{
    public class HomeController : Controller
        {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger,DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext= context;
            _userManager= userManager;
        }

        public IActionResult Index()
        {
            var products =_dataContext.Products.Include("Category").Include("Brand").ToList();
            var sliders= _dataContext.Sliders.Where(s=>s.Status==1).ToList();
            ViewBag.Sliders=sliders;

            return View(products);
        }

        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         //join u in _dataContext.Users on c.UserId equals Convert.ToInt32(u.Id)
                                         select new { Product = p, Compares = c})
                                .ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> DeleteCompare(int Id)
        {
            //Tìm bản ghi theo id
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);
            
            //Xóa bản ghi và lưu thay đổi
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();

            //Đặt thông báo thành công và chuyển hướng
            TempData["success"] = "So sánh đã được xóa thành công";
            return View("Compare", "Home");
        }

        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            //Tìm bản ghi theo id
            WishlistModel wishlist = await _dataContext.Wishlishs.FindAsync(Id);

            //Xóa bản ghi và lưu thay đổi
            _dataContext.Wishlishs.Remove(wishlist);
            await _dataContext.SaveChangesAsync();

            //Đặt thông báo thành công và chuyển hướng
            TempData["success"] = "Yêu thích đã được xóa thành công";
            return View("Wishlist", "Home");
        }
        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         //join u in _dataContext.Users on c.UserId equals Convert.ToInt32(u.Id)
                                         select new {  Product = p, Compares = c })
                                .ToListAsync();
            return View(wishlist_product);
        }

        
        public IActionResult Privacy()
        {
            return View();
        }

		//public IActionResult Contact()
		//{

		//	return View();
		//}
        //chuyển hưỚNG TỚI TRANG LIÊN HỆ:
        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contact.FirstAsync();
            return View(contact);
        }
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        //THÊM SP YÊU THÍCH:
        [HttpPost]
        public async Task<IActionResult> AddWishlish(int Id, WishlistModel wishlish)
        {
            var user=await _userManager.GetUserAsync(User);
            wishlish.ProductId = Id;
			wishlish.UserId = Convert.ToInt32(user.Id);
			_dataContext.Add(wishlish);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm sản phẩm yêu thích thành công !" });
            }
            catch(Exception)
            {
                return StatusCode(500,"Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng!");
            }
        }  
        //THÊM SP SO SÁNH:
        [HttpPost]
        public async Task<IActionResult> AddWCompare(int Id)
        {
            var user=await _userManager.GetUserAsync(User);
            var compareProduct = new CompareModel
            {
                ProductId=Id,
                UserId=Convert.ToInt32(user.Id)
            };
			_dataContext.Compares.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm sản phẩm so sánh thành công !" });
            }
            catch(Exception)
            {
                return StatusCode(500,"Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng!");
            }
        }
    }
}
