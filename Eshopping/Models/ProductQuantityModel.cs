using System.ComponentModel.DataAnnotations;

namespace Eshopping.Models
{
	public class ProductQuantityModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập số lượng sản phẩm ")]
		public int Quantity { get; set; }

		
		public int ProductId { get; set; }

		public DateTime DateCreated { get; set; }
	}
}
