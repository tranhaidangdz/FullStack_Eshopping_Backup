using Eshopping.Models;
using Eshopping.Models.ViewModels;
using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Azure.Core.HttpHeader;

namespace Eshopping.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext _context)
		{
			_dataContext = _context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			//nhận shipping từ cookie (tức là ta vừa thêm giá ship vào cookie xong giờ lại lấy ra):
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			decimal shippingPrice = 0;

			//nhận mã giảm giá từ cookie : 
			var coupon_code = Request.Cookies["CouponTitle"];
			if (shippingPriceCookie != null)
			{
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);  //đổi lại về kiểu decimal: giá tiền ban đầu 
			}


			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
				ShippingCost = shippingPrice,
				CouponCode = coupon_code
			};
			return View(cartVM);
		}

		public IActionResult Checkout()
		{
			return View("~/Views/Checkout/Index.cshtml");
		}
		//phương thức bất đồng bộ:
		public async Task<IActionResult> Add(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			//check xem đã có sp trong giỏ hàng chưa: id giỏ hàng = id sp 
			CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItems == null)  //nếu chưa có sp thì thêm ID sp vào giỏ hàng
			{
				cart.Add(new CartItemModel(product));
			}
			else  //có sp đó rồi thì tăng số lg lên 1 
			{
				cartItems.Quantity += 1;
			}
			//lưu trữ dữ liệu cart vào session:
			HttpContext.Session.SetJson("Cart", cart);

			TempData["success"] = "Add Item to cart Successfully!";
			return Redirect(Request.Headers["Referer"].ToString());
		}

		public async Task<IActionResult> Decrease(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity > 1)
			{
				--cartItem.Quantity;  //nếu bấm vào nút có class decrease ta ktra số lg , nếu >1 thì giảm số lg sp đi 
			}
			else
			{
				cart.RemoveAll(p => p.ProductId == Id);
			}

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);

			}
			TempData["success"] = "Decrease Item quantity to cart Successfully!";
			return RedirectToAction("Index");
		}
		//tuong tự khi ta nhấn asp-action="Increase":nút tăng sp 
		public async Task<IActionResult> Increase(int Id)
		{
			ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			// Kiểm tra số lượng sản phẩm còn lại, nếu đã đạt số lượng tối đa thì không cho tăng
			if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
			{
				++cartItem.Quantity;  //nếu bấm vào nút có class decrease ta ktra số lg , nếu >1 thì tang  số lg sp đi 
				TempData["success"] = "Increase the number of successful products";
			}
			else
			{
				cartItem.Quantity = product.Quantity;
				TempData["error"] = "The number of products has reached the maximum";
			}

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);

			}
			TempData["success"] = "Increase Item quantity to cart Successfully!";
			return RedirectToAction("Index");
		}

		//ham remove san pham:
		public async Task<IActionResult> Remove(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			cart.RemoveAll(p => p.ProductId == Id);
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);

			}
			TempData["success"] = "Remove Item of cart Successfully!";
			return RedirectToAction("Index");

		}
		//hàm xóa toàn bộ giỏ hàng:
		public async Task<IActionResult> ClearCart()
		{
			HttpContext.Session.Remove("Cart");

			TempData["success"] = "Clear all Item of cart Successfully!";
			return RedirectToAction("Index");
		}


		//TÍNH PHÍ SHIP KHI NG DÙNG THÊM SP VÀO GIỎ HÀNG VÀ HỌ NHẬP VÀO ĐỊA CHỈ. TA SẼ ỰA VÀO ĐCHI TỈNH/THÀNH PHỐ TRONG SHIPPING BACKEND 
		//ĐỂ TÍNH PHÍ SHIP THEO CÁC TỈNH CHO KHÁCH 
		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shipping, string quan, string tinh, string phuong)
		{
			//so sanh địa chỉ ng dùng nhập vào có giông đchi lưu trong mục của admin không 
			var existingShipping = await _dataContext.Shippings.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

			decimal shippingPrice = 0; //giá ship mặc định ban đầu

			if (existingShipping != null)
			{
				//ktra nếu tìm thấy địa chỉ ship ng dùng nhập trong CSDL
				shippingPrice = existingShipping.Price;  //gán giá ship = giá ship quy định theo vùng 
			}
			else
			{
				//set giá tiền mặc định nếu ko tìm thấy tỉnh/Thành phố đó
				//VD: các tỉnh ở gần trong CSDL sẽ có phí ship 20k->40k; các tỉnh ở xa mặc định là 50k (or ship tòan quốc là 50k)
				shippingPrice = 50000;
			}
			//chuyển giá vận chuyển về kiểu json 
			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
			try
			{
				//tạo 1 cookie cho trang web, thời hạn 30'
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTime.UtcNow.AddMinutes(30),
					Secure = true,  //using HTML 
				};
				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions); // đẩy gía ship dạng JSON shippingPriceJson vào cái cookie vừa tạo "cookieOptions", voi ten la: ShippingPrice
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error adding shipping price to cookie: {ex.Message}");
			}
			return Json(new { shippingPrice });
		}
		//XÓA GIÁ VẬN CHUYỂN:
		[HttpGet]
		[Route("Cart/RemoveShippingCookie")]

		public IActionResult RemoveShippingCookie()
		{
			Response.Cookies.Delete("ShippingPrice");
			return RedirectToAction("Index", "Cart");

		}

		//THEM VOUCHER KHUYẾN MÃI:cũng giống như phần shipping ta ko dùng session mà dùng cookie , do session sẽ chết sau 1 khoảng tgian 
		[HttpPost]
		[Route("Cart/GetCoupon")]
		public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
		{
			// Kiểm tra mã giảm giá trong cơ sở dữ liệu
			var validCoupon = await _dataContext.Coupons
			.FirstOrDefaultAsync(x => x.Name == coupon_value && x.Quantity >= 1);
			// Nếu không tìm thấy mã giảm giá, trả về thông báo lỗi
			if (validCoupon == null)
			{
				return Ok(new { success = false, message = "Discount code does not exist" });
			}

			// Nếu mã giảm giá tồn tại, lấy tên và mô tả
			string couponTitle = validCoupon.Name + "|" + validCoupon.Description;

			if (couponTitle != null)  //néu lấy đc coupon 
			{
				TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;  //lấy tgian còn lại của coupon so vs hiện tại 
				int daysRemaining = remainingTime.Days;  //lấy ra ngày còn lại 
				if (daysRemaining >= 0)
				{
					try
					{
						var cookieOptions = new CookieOptions  //bắt đầu tạo 1 cookie
						{
							HttpOnly = true,  // mở http và https 
							Expires = DateTimeOffset.UtcNow.AddMinutes(30),  //tgian cookie = ngày hết hạn + 30'
							Secure = true,
							SameSite = SameSiteMode.Strict // Kiểm tra tính tương thích trình duyệt
						};
						Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
						return Ok(new { success = true, message = "Add discount code successfully" });

					}
					catch (Exception ex)
					{
						//tra ve loi:
						Console.WriteLine($"Error when adding discount code:{ex.Message}");
						return Ok(new { success = false, message = "Coupon error " });
					}
				}
				else
				{
					return Ok(new { success = false, message = "Coupon has expired" });
				}
			}
			else  //nếu couponTitle ==null 
			{
				return Ok(new { success = false, message = "Discount code does not exist" });
			}
			//return Json(new { success = true, message = "Coupon applied successfully!" });

			return Json(new { couponTitle = couponTitle });

		}


	}
}