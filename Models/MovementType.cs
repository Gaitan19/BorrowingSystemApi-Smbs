namespace BorrowingSystemAPI.Models
{
    public class MovementType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }  

        public string Description { get; set; }
        public bool IsInput { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
