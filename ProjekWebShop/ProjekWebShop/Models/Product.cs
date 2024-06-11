namespace ProjekWebShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }

        public bool IsDiscount { get; set; }
        public float DiscountPrice { get; set; }
        public int NuberOfSoldItems { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public bool IsHot { get; set; }
        public int CategoryId { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
