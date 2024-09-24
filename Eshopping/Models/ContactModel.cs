using Eshopping.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eshopping.Models
{
	public class ContactModel
	{
		[Key]
		[Required(ErrorMessage = "Yêu cầu nhập tiêu đề website")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập địa chỉ")]
		public string Map { get; set; }


		[Required(ErrorMessage = "Yêu cầu nhập email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập thông tin liên hệ")]
		public string Description { get; set; }
		public string LogoImg { get; set; }


		[NotMapped]
		[FileExtension]
		public IFormFile ImageUpload { get; set; }
	}
}
