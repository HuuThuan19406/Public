using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class GoodsUploadHandler
    {
        [Required]
        public byte CategoryId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Note { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string DataFileBase64 { get; set; }
    }
}