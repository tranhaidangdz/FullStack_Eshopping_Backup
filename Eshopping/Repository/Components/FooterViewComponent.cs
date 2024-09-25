using Eshopping.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Areas.Admin.Repository
{
	public class FooterViewComponent : ViewComponent
	{
		private readonly DataContext _dataContext;
		public FooterViewComponent(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IViewComponentResult> InvokeAsync()=>View(await _dataContext.Contact.FirstOrDefaultAsync());  //FirstOrDefaultAsync : do footer chỉ có 1 thông tin  nên ta chỉ cần lấy ra cái đầu tiên = 1 cái footer trong CSDL , chứ ko liệt kê theo kiểu danh sách footer 

	}
}
