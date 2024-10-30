using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Slider")]
	[Authorize(Roles = "Admin")]
	public class SliderController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment; //IWebHostEnvironment: để load file ảnh thì phải có dòng này 
		public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}

		[Route("Index")]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Sliders.OrderBy(p => p.Id).ToListAsync());
		}
		[Route("Create")]
		public IActionResult Create() //trả về index view của slider 
		{

			return View();
		}

		//create slider: thêm ảnh đề tạo slider:
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SliderModel slider)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
		{
			if (ModelState.IsValid)
			{

				if (slider.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
					string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await slider.ImageUpload.CopyToAsync(fs);
					fs.Close();
					slider.Image = imageName;
				}

				_dataContext.Add(slider);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Added slider successfully";  //slider: thanh trượt: chính là menu trượt ở giữa trang chủ mỗi trang web 
				return RedirectToAction("Index");

			}
			else
			{
				TempData["error"] = "The model has a few things that are wrong";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(slider);
		}
		//edit slider 
		[Route("Edit")]
		public async Task<IActionResult> Edit(int Id)
		{
			SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
			return View(slider);
		}

		//Edit slider: thêm ảnh đề Edit slider:
		[Route("Edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SliderModel slider)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
		{
			var slider_exited = _dataContext.Sliders.Find(slider.Id);
			if (ModelState.IsValid)
			{

				if (slider.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
					string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await slider.ImageUpload.CopyToAsync(fs);
					fs.Close();
					slider_exited.Image = imageName;
				}
				slider_exited.Name = slider.Name;
				slider_exited.Description = slider.Description;
				slider_exited.Status = slider.Status;

				_dataContext.Update(slider_exited);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Updated slider successfully";  //slider: thanh trượt: chính là menu trượt ở giữa trang chủ mỗi trang web 
				return RedirectToAction("Index");

			}
			else
			{
				TempData["error"] = "The model has a few things that are defective";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(slider);
		}

		[Route("Delete")]
		//delete slider: gửi id slider về trang edit :
		public async Task<IActionResult> Delete(int Id)
		{
			SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
			if (!string.Equals(slider.Image, "noname.jpg"))
			{
				string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
				string oldfileImage = Path.Combine(uploadsDir, slider.Image);
				if (System.IO.File.Exists(oldfileImage))
				{
					System.IO.File.Delete(oldfileImage);
				}
			}
			_dataContext.Sliders.Remove(slider);
			await _dataContext.SaveChangesAsync();
			TempData["error"] = "Product has been deleted";
			return RedirectToAction("Index");
		}
	}
}
