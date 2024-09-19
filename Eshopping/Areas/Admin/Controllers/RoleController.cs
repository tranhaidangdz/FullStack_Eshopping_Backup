using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    //[Authorize(Roles ="Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUserModel> _userManager;
        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(p=>p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }  
        //==================================================================
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);  //tim kiem role theo id
            return View(role);
        }
        
        //edit:
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            //ktra role có tồn tại ko:
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = model.Name; //update role name với gtri lấy từ model dât(gtri ng dùng nhập)
            try
            {
                await _roleManager.UpdateAsync(role);
                TempData["success"] = "Cập nhật role thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                    ModelState.AddModelError("", "An error occurred while deleting the role.");
            }
            }
            // nếu model ko tồn tại or idenntityrole ko tồn tại=> trả về view với id của role hiện tại 
            return View(model ?? new IdentityRole { Id=id});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            //avoid duplicate role:nếu cái role chưa tồn tại thì ta tao role mới 
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        //delete:
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete (string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role =await _roleManager.FindByIdAsync(id);  //tim kiem role theo id
            if(role==null)
            {
                return NotFound();
            }
            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa role thành công!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the role.");

            }
            return Redirect("Index");
        }


    }
}
