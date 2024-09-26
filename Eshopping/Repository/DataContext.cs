using Eshopping.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Repository
{
    public class DataContext:IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext>options):base(options) 
        {
        }
        //để tạo csdl ta sẽ thêm các phương thức dataset, lấy thuộc tính từ model, sau đó chạy add-migration trong nugnet packet console là tạo đc csdl  
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
		public DbSet<OrderDetails> OrderDetails { get; set; }
		public DbSet<ContactModel> Contact { get; set; }
		public DbSet<WishlishModel> Wishlishs { get; set; }
		public DbSet<CompareModel> Compares { get; set; }
	}
}
