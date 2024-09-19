using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Eshopping.Repository.Componets
{
    public class CategoriesViewComponent:ViewComponent
    {
        private readonly DataContext _dataContext;
        public CategoriesViewComponent(DataContext _context)
        {
            _dataContext = _context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Categories.ToListAsync());
    }
}
