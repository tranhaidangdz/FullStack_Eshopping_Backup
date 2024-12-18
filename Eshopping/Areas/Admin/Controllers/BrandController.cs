﻿using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//NÓI CHUNG LÀ PHẦN THÊM SỬA XÓA THƯƠNG HIỆU TA COPY NGUYÊN TỪ CATEGORY SANG, THAY MỖI CATEGORY THÀNH "BRAND"
namespace Eshopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "Admin")] //phân quyền truy cập manage branch 
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brand = _dataContext.Brands.ToList(); // 33 datas

            const int pageSize = 10; // 10 items/ 1 trang

            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = brand.Count(); // 33 items

            var pager = new Paginate(recsCount, pg, pageSize);
            
            int recSkip = (pg - 1) * pageSize; // (1-1) * 10

            /*
             trang 1:  category.Skip(0).Take(10).ToList(); lấy item từ  0 -> 9
             trang 2:  category.Skip(10).Take(10).ToList(); lấy item từ 10 -> 19
             */


            var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager; // truyền pager qua trang index thông qua viewbag

            return View(data);
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand= await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        //form create thuong hieu:
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(BrandModel brand)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {

            if (ModelState.IsValid)
            {
                //code them du lieu san pham:
                //TempData["success"] = "Model ok hết rồi";
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == brand.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "This brand is already in the Database");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Add brand successfully";
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
            return View(brand);
        }

        //DELETE:
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            _dataContext.Brands.Remove(brand);
            //sau khi xóa đi category ta phải gọi hàm save change thì nó mới lưu sự thay đổi CSDL
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Brand has been deleted";
            return RedirectToAction("Index");
        }

        //EDIT brand =============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(BrandModel brand)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {

			if (ModelState.IsValid)
			{
				// Tạo slug mới từ tên brand
				brand.Slug = brand.Name.Replace(" ", "-");

				// Kiểm tra xem có slug nào giống trong bảng Brands không (thay vì Categories)
				var slugExists = await _dataContext.Brands
									 .Where(b => b.Slug == brand.Slug && b.Id != brand.Id) // Loại trừ brand hiện tại
									 .FirstOrDefaultAsync();

				if (slugExists != null)
				{
					ModelState.AddModelError("", "This brand is already in the Database");
					return View(brand);
				}

				// Tìm brand cũ theo ID và cập nhật dữ liệu
				var brandToUpdate = await _dataContext.Brands.FindAsync(brand.Id);

				if (brandToUpdate == null)
				{
					TempData["error"] = "Brand not found";
					return RedirectToAction("Index");
				}

				// Cập nhật dữ liệu cho brand cũ
				brandToUpdate.Name = brand.Name;
				brandToUpdate.Slug = brand.Slug;
				brandToUpdate.Description = brand.Description;
				brandToUpdate.Status = brand.Status;

				// Lưu thay đổi vào CSDL
				_dataContext.Update(brandToUpdate);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Update brand successfully";
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
			return View(brand);
		}

	}
}
