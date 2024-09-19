using System.ComponentModel.DataAnnotations;

namespace Eshopping.Repository.Validation
{
    public class FileExtensionAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension=Path.GetExtension(file.FileName);  //đg dẫn hình ảnh
                string[] extensions = {"jpg","png","jpeg" };  //các đuôi hình ảnh hay gặp

                bool result = extensions.Any(x=>extension.EndsWith(x));  //ta ktra hình ảnh đầu vào có phải 1 trong 3 loại đuôi hay gặp ko 
                if (!result)  //nếu khác các loại đuôi trên:
                {
                    return new ValidationResult("Chỉ cho phép loại hình ảnh là jpg,png hoặc jpeg");
                }
            }
            return ValidationResult.Success;
        }
    }
}
