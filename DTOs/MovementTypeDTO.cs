using System.ComponentModel.DataAnnotations;

namespace BorrowingSystemAPI.DTOs
{
    public class MovementTypeDTO
    {

        [Required(ErrorMessage = "Name is required")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "IsInput is required")]

        public bool IsInput { get; set; }
    }
}
