using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Shipping")]
	public class ShippingController : Controller
	{
        private readonly DataContext _dataContext;
        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }
		[Route("Index")]
        public IActionResult Index()
		{
			return View();
		}
	}
}
