using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

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
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new { User = u, Product = p, Compares = c})
                                .ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> DeleteCompare(int Id)
        {
            //Tìm bản ghi theo id
            CompareModel compare = _dataContext.Compares.FirstOrDefault(w => w.ProductId == Id);
            // Kiểm tra nếu không tìm thấy bản ghi
            if (compare == null)
            {
                TempData["error"] = $"The compare item id: {Id} could not be found.";
                return RedirectToAction("Compare", "Home");
            }
            //Xóa bản ghi và lưu thay đổi
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();

            //Đặt thông báo thành công và chuyển hướng
            TempData["success"] = "The comparison has been successfully deleted";
            return RedirectToAction("Compare", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            //Tìm bản ghi theo id
            WishlistModel wishlist = _dataContext.Wishlists.FirstOrDefault(w => w.ProductId == Id);


            // Kiểm tra nếu không tìm thấy bản ghi
            if (wishlist == null)
			{
				TempData["error"] = $"The wishlist item {Id} could not be found.";
				return RedirectToAction("Wishlist", "Home");
			}
			//Xóa bản ghi và lưu thay đổi
			_dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();

            //Đặt thông báo thành công và chuyển hướng
            TempData["success"] = "The favorite has been successfully deleted";
            return RedirectToAction("Wishlist", "Home");
        }
        public async Task<IActionResult> Wishlist()
        {

			var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          join u in _dataContext.Users on w.UserId equals u.Id
										  select new { Product = p, Wishlist = w })
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

        public async Task<IActionResult> AddWishlist(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _dataContext.Products.FindAsync(Id);
            if (product == null)
            {
                return NotFound("Product does not exist.");
            }

            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            var productInWishlist = _dataContext.Wishlists.Where(w => w.ProductId == Id).FirstOrDefault();
            if (productInWishlist == null)
            {
				_dataContext.Wishlists.Add(wishlistProduct);
			}
            else
            {
				return Ok(new { success = false, message = "Product already exist in wishlist!" });
			}

			try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Added favorite products!" });
            }
            catch(Exception)
            {
                return StatusCode(500, "An error occurred while updating order status!");
            }
        }  
        //THÊM SP SO SÁNH:
        [HttpPost]

        public async Task<IActionResult> AddCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(User);

            var product = await _dataContext.Products.FindAsync(Id);
            if (product == null)
            {
                return NotFound("Product does not exist.");
            }


            var compareProduct = new CompareModel
            {
                ProductId=Id,
                UserId=user.Id
            };

			var productInCompare = _dataContext.Compares.Where(w => w.ProductId == Id).FirstOrDefault();
			if (productInCompare == null)
			{
				_dataContext.Compares.Add(compareProduct);
			}
			else
			{
				return Ok(new { success = false, message = "Product already exist in compare!" });
			}

			
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Added comparison products!" });
            }
            catch(Exception)
            {
                return StatusCode(500, "An error occurred while updating order status!");
            }
        }
    }
}
