using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
		[Area("Admin")]
		[Route("Admin/Slider")]
		//[Authorize(Roles = "Publisher,Author,Admin")]
	public class SliderController:Controller
	{
		private readonly DataContext _dataContext;
		public SliderController(DataContext context)
		{
			_dataContext = context;
		}

		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Sliders.OrderBy(p => p.Id).ToListAsync());
		}
	}
}
