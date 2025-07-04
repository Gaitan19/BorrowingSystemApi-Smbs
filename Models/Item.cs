﻿namespace BorrowingSystemAPI.Models
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        
        public string? Brand { get; set; }

        public string? Color { get; set; }

        public string? Code { get; set; }
        public DateTime? DeletedAt { get; set; }


        public List<Movement> ItemMovements { get; set; } = new List<Movement>();
    }
}
