namespace ProjekWebShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int  Zip { get; set; }
        public string Phone { get; set; }
        public int Delivery { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public bool OrderCompleted { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    }
}
