using System.ComponentModel.DataAnnotations;

namespace Eshopping.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        
        public ProductModel ProductDetails {  get; set; }

        [Required(ErrorMessage ="Yêu cầu nhập bình luận sản phẩm")]
        public string Comment { get; set; }

        [Required(ErrorMessage ="Yêu cầu nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Yêu cầu nhập email")]
		[EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
		public string Email { get; set; }
    }
}
