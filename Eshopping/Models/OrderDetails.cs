using System.ComponentModel.DataAnnotations.Schema;

namespace Eshopping.Models
{
	public class OrderDetails
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string OrderCode { get; set; }
		public int ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá hiện tại vì khi thêm vào giỏ hàng có thể được giảm giá (khác giá ở bảng product ) 
		public int Quantity { get; set; } // Số lượng 
		[ForeignKey("ProductId")]
		public ProductModel Product { get; set; }
	}
}
