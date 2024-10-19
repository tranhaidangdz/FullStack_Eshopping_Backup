using System.ComponentModel.DataAnnotations;

namespace Eshopping.Models
{
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage="Yêu cầu nhập tên Coupon")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập mô tả")]
		public string Description { get; set; }

		//ngày bắt đầu khuyến mãi
		public DateTime DateStart { get; set; }
		//ngày kết thúc khuyến mãi
		public DateTime DateExpired { get; set; }
		[Required(ErrorMessage = "Yêu cầu giá")]
		public decimal Price { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập số lượng coupon")]

		public int Quantity {  get; set; }
		public int Status {  get; set; }


	}
}
