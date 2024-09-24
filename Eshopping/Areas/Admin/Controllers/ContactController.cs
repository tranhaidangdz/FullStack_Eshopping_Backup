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
	[Route("Admin/Contact")]
	//[Authorize(Roles = "Admin")] 
	public class ContactController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment; //IWebHostEnvironment: để load file ảnh thì phải có dòng này 
		public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}

		[Route("Index")]
		public IActionResult Index()
		{
			var contact = _dataContext.Contact.ToList();
			return View(contact);
		}

		[Route("Edit")]
		public async Task<IActionResult> Edit()
		{
			ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
			return View(contact);
		}


		//FORM edit:
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Edit")]
		public async Task<IActionResult> Edit(ContactModel contact)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
		{
			var exitsted_contact = _dataContext.Contact.FirstOrDefault(); // lấy dữ liệu đầu tiên
			if (ModelState.IsValid)
			{
				
				if (contact.ImageUpload != null)
				{

					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");
					string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName; // làm cho hình ảnh không bị trùng tên
					string filePath = Path.Combine(uploadsDir, imageName);

					//cập nhật và xóa ảnh sp cũ: tức là khi muốn cập nhật bức ảnh mới ta sẽ xóa ảnh cũ đi và thay mới 
					string oldfilePath = Path.Combine(uploadsDir, exitsted_contact.LogoImg);
					try
					{
						//khi ta đã thay bức ảnh mới nó sẽ ktra và xóa ảnh cũ khỏi csdl 
						if (System.IO.File.Exists(oldfilePath))
						{
							System.IO.File.Delete(oldfilePath);
						}
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "Lỗi khi xóa ảnh sản phẩm ");
					}

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await contact.ImageUpload.CopyToAsync(fs);
					fs.Close();
					exitsted_contact.LogoImg = imageName;
				}
				//update other contact properties: VD ta chỉ muốn thay đổi 1 trong các thuộc tính của sp thôi, ta sẽ chỉ thay đổi những thuộc tính t/ứng đc truyền vào 
				//cái hay ở đây: khi ta ko thay đổi ảnh thì nó vẫn giữ nguyên cái ảnh cũ, ko bị xóa mất 
				exitsted_contact.Name = contact.Name;
				exitsted_contact.Email = contact.Email;
				exitsted_contact.Description = contact.Description;
				exitsted_contact.Phone = contact.Phone;
				exitsted_contact.Map = contact.Map;


				_dataContext.Update(exitsted_contact);

				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thông tin web thành công";
				return RedirectToAction("Index");

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
				return BadRequest(errorMessage);
			}
			return View(contact);
		}
	}
}
