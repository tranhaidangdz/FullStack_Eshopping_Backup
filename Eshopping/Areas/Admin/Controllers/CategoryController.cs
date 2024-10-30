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
	[Authorize(Roles = "Admin")]
	public class CategoryController:Controller
	{
		
		private readonly DataContext _dataContext;
		public CategoryController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index(int pg = 1)
		{
            List<CategoryModel> category = _dataContext.Categories.ToList(); // 33 datas

            const int pageSize = 10; // 10 items/ 1 trang

            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = category.Count(); // 33 items

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; // (1-1) * 10

            /*
             trang 1:  category.Skip(0).Take(10).ToList(); lấy item từ  0 -> 9
             trang 2:  category.Skip(10).Take(10).ToList(); lấy item từ 10 -> 19
             */


            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager; // truyền pager qua trang index thông qua viewbag

            return View(data);

		}
        
        public async Task<IActionResult> Edit(int Id)
		{
            CategoryModel category =await _dataContext.Categories.FindAsync(Id);
            return View(category);

		}
        public IActionResult Create()  //lấy ra ds danh mục và thương hiệu sp
        {
            return View();
        }

        //form create danh muc:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {

            if (ModelState.IsValid)
            {
                //code them du lieu san pham:
                //TempData["success"] = "Model ok hết rồi";
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "This category is already in the Database");
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Added category successfully";
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
            return View(category);
        }
        //EDIT CATEGORY =============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {

            if (ModelState.IsValid)
            {
                //code them du lieu san pham:
                //TempData["success"] = "Model ok hết rồi";
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "This category is already in the Database");
                    return View(category);
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Updated category successfully";
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
            return View(category);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
          
            _dataContext.Categories.Remove(category);
            //sau khi xóa đi category ta phải gọi hàm save change thì nó mới lưu sự thay đổi CSDL
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Category has been deleted";
            return RedirectToAction("Index");
        }
    }
}
