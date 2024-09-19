using System.ComponentModel.DataAnnotations;

namespace Eshopping.Models.ViewModels
{
	public class LoginViewModel
	{
		// Loại bỏ trường email khi đăng nhập
		// Nhận thông tin đăng nhập lỗi => tách ra khỏi user model
		public int Id { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập username")]
		public string Username { get; set; }

		[DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập password")]
		public string Password { get; set; }

		public string ReturnUrl { get; set; }
	}
}
