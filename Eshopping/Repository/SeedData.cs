using Eshopping.Models;
using Microsoft.EntityFrameworkCore;

namespace Eshopping.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel macbook = new CategoryModel { Name = "macbook", Slug = "macbook", Description = "macbook is Large product in the world ", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "pc", Slug = "pc", Description = "samsung  is Large product in the world ", Status = 1 };  
                BrandModel apple = new  BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is Large Brand in the world ", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "samsung  is Large Brand in the world ", Status = 1 };
        _context.Products.AddRange(
                            new ProductModel { Name="Macbook",Slug="Macbook",Description="Macbook id the best",Image="product1.jpg",Category=macbook,Brand=samsung,Price=1234},
                            new ProductModel { Name="Pc",Slug="Pc",Description="Pc id the best",Image="product2.jpg",Category=pc,Brand=samsung,Price=1234}
                        );
                _context.SaveChanges();
            }
        }

    }
}
