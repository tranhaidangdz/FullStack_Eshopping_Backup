using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        private readonly IWebHostEnvironment _webHostEnvironment; //IWebHostEnvironment: để load file ảnh thì phải có dòng này 
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ProductModel> product = _dataContext.Products.OrderBy(P => P.Id).Include(p => p.Category).Include(p => p.Brand).ToList(); // 33 datas

            const int pageSize = 10; // 10 items/ 1 trang

            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = product.Count(); // 33 items
            //Console.WriteLine(recsCount);

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; // (1-1) * 10

            /*
             trang 1:  category.Skip(0).Take(10).ToList(); lấy item từ  0 -> 9
             trang 2:  category.Skip(10).Take(10).ToList(); lấy item từ 10 -> 19
             */


            var data = product.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager; // truyền pager qua trang index thông qua viewbag

            return View(data);
            //return View(await _dataContext.Products.OrderByDescending(P => P.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }
        [HttpGet]
        public IActionResult Create()  //lấy ra ds danh mục và thương hiệu sp
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                //code them du lieu san pham:
                //TempData["success"] = "Model ok hết rồi";
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "This product is already in the Database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Added product successfully!";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "The model has a few things that are wrong!";
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
            return View(product);
        }
        [HttpGet]

        public async Task<IActionResult> Edit(int Id)
        {
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			if (product == null)
			{
				// Thêm thông báo lỗi hoặc xử lý khi không tìm thấy sản phẩm
				return NotFound();
			}

			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        //FORM edit:
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(ProductModel product)  //lấy ds và thương hiệu của form (nhận từ ng dùng ) sau đó so sánh với các sp đã có trong csdl 
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
			var exitsted_product = _dataContext.Products.Find(product.Id); // sử dụng FindAsync:tìm sp theo ID 
			if (exitsted_product == null)
			{
				TempData["error"] = "\r\nProduct does not exist!";
				return RedirectToAction("Index");
			}
			if (ModelState.IsValid)
            {
                //code them du lieu san pham:
                //TempData["success"] = "Model ok hết rồi";
                product.Slug = product.Name.Replace(" ", "-");


                if (product.ImageUpload != null)
                {

                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    // Kiểm tra nếu sản phẩm đã có ảnh cũ
                    if (!string.IsNullOrEmpty(exitsted_product.Image))  //ktra nếu CSDL chưa có bức ảnh cũ => cột Image đang null .do  Path.Combine ko đc chứa gtri null nên ta phải ktra trc 
					{
                        //cập nhật và xóa ảnh sp cũ: tức là khi muốn cập nhật bức ảnh mới ta sẽ xóa ảnh cũ đi và thay mới 
                        string oldfilePath = Path.Combine(uploadsDir, exitsted_product.Image);
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
                            ModelState.AddModelError("", "Error deleting product image! ");
                        }
                    }
					// Lưu ảnh mới:Sử dụng using cho FileStream: Đảm bảo FileStream được đóng tự động sau khi ghi file bằng cách sử dụng using.
					using (var fs = new FileStream(filePath, FileMode.Create))
					{
						await product.ImageUpload.CopyToAsync(fs);
					}
					
                        exitsted_product.Image = imageName;

                    
                }
                //update other product properties: VD ta chỉ muốn thay đổi 1 trong các thuộc tính của sp thôi, ta sẽ chỉ thay đổi những thuộc tính t/ứng đc truyền vào 
                //cái hay ở đây: khi ta ko thay đổi ảnh thì nó vẫn giữ nguyên cái ảnh cũ, ko bị xóa mất 
                exitsted_product.Name = product.Name;
                exitsted_product.Description = product.Description;
                exitsted_product.Price = product.Price;
                exitsted_product.CategoryId = product.CategoryId;
                exitsted_product.BrandId = product.BrandId;


                _dataContext.Update(exitsted_product);

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Product update successful!";
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
            return View(product);
        }

        //delete sp: gửi id sp về trang edit :
        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldfileImage = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Product deleted";
            return RedirectToAction("Index");
        }

        // Add more quantity to products
        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.Id = Id;
            return View();
        }

        [Route("StoreProductQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            // Tim san pham dua vao product id
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            // Cộng dồn số lượng sản phẩm nếu tìm thấy
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();

            TempData["success"] = "Add product quantity successfully!";
            return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });


        }
    }
}
