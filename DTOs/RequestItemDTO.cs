using BorrowingSystemAPI.Models;

namespace BorrowingSystemAPI.DTOs
{
    public class RequestItemDTO
    {
        public Guid RequestId { get; set; }
        public Guid ItemId { get; set; }
        public string? Description { get; set; }
    }
}
