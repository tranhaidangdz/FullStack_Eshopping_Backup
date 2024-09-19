using Eshopping.Models;
using Eshopping.Models.ViewModels;
using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;

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
			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
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
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity >= 1)
			{
				++cartItem.Quantity;  //nếu bấm vào nút có class decrease ta ktra số lg , nếu >1 thì tang  số lg sp đi 
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
            TempData["success"] = "Increase Item quantity to cart Successfully!" ;
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
			TempData["success"] = "Remove Item of cart Successfully!" ;
            return RedirectToAction("Index");

		}
		//hàm xóa toàn bộ giỏ hàng:
		public async Task<IActionResult> ClearCart()
		{
			HttpContext.Session.Remove("Cart");

            TempData["success"] = "Clear all Item of cart Successfully!" ;
            return RedirectToAction("Index");
		}
	}
}
