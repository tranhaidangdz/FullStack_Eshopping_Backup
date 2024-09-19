using Eshopping.Areas.Admin.Repository;
using Eshopping.Models;
using Eshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//connet database
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
}
);
//

// đăng ký phần gửi mail:
builder.Services.AddTransient<IEmailSender,EmailSender>();
// Add services to the container.
builder.Services.AddControllersWithViews();
//dki session:
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.IsEssential = true;
}
);
//đki chức năng đăng nhập đăng kí trang web asp:
//builder.Services.AddDbContext<DbContext>();

builder.Services.AddIdentity<AppUserModel, IdentityRole>()
	.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
//thừa
builder.Services.AddAuthentication();
builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings:yêu cầu mk khi đăng nhập :mk phải chứa số, chữ thường, ký tự đặc biệt ...
	options.Password.RequireDigit = true;  //ycau số
	options.Password.RequireLowercase = true; //ycau chữ thường 
	options.Password.RequireNonAlphanumeric = false; //ycau ký tự đặc biệt 
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4; //ycau độ dài tối thiểu = 4 ký tự 

	// Lockout settings.
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  //khóa tk sau 5'
	options.Lockout.MaxFailedAccessAttempts = 5;  //số lần truy cập tối đa là 5 lần, hết 5 lần sẽ khóa 
	options.Lockout.AllowedForNewUsers = true;  //cho phép ng dùng mới

	// User settings:chỉ cho phép mk chứa các ký tự dưới dây 
	options.User.AllowedUserNameCharacters ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
	options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
	// Cookie settings
	options.Cookie.HttpOnly = true;
	options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

	options.LoginPath = "/Identity/Account/Login";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.SlidingExpiration = true;
});


var app = builder.Build();

//đường dẫn đễn trang 404 notfound : khi lỗi thường chrome sẽ hiển thị 1 trang mặc định, ta muốn khi lỗi nó sẽ đi đến trang báo lỗi do tra custom 
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");
//app.UseHttpsRedirection();  //THEO giao thức trang web (đg link đến trang web) 

app.UseSession();
app.UseStaticFiles();
//Cấu hình request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//ta khai báo quyền theo mức độ truy cập :  TỨC LÀ TRANG WEB SẼ ĐỌC FILE XÁC THỰC TRƯỚC KHI ĐỌC FILE QUYỀN NG DÙNG ...

app.UseStaticFiles();  //theo file

app.UseRouting(); //theo đg dẫn thư mục 

app.UseAuthentication();  //theo xác thực identity (đăng nhập)

app.UseAuthorization(); //theo quyền ng dùng  (trao quyền)

//Đki map này cho backend: ten map là areas, mặc định "controller=Product"
//ta sẽ để trang backend lên trên: do mặc định nó chạy frontend đầu tiên , mà ta muốn backend chạy trc 
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");
//ta sẽ đki các map cho category,brand :khi click vào đó nó sẽ chuyển từ trang index mặc định sang các trang này 
app.MapControllerRoute(
    name: "category",
    pattern: "/category/{Slug?}",
    defaults: new {controller="Category",action="Index"});
app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new {controller="Brand",action="Index"});

//map này của FE: những map route mới ta phải viết trên route mặc định này 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//seeding data:tao du lieu trong sql server
var context=app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);
app.Run();
