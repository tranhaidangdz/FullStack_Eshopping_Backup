using System.ComponentModel.DataAnnotations;

namespace Eshopping.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage="Yêu cầu nhập tên danh mục ")]
        public string Name { get; set; }

        [Required(ErrorMessage="Yêu cầu nhập Mô tả danh mục ")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
    }
}
