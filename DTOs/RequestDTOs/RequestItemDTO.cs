using System.ComponentModel.DataAnnotations;

namespace BorrowingSystemAPI.DTOs.RequestDTOs
{
    public class RequestItemDTO
    {
        [Required]
        public Guid ItemId { get; set; }

        public string? Description { get; set; }

        public int? Quantity { get; set; } = 1;


    }
}
