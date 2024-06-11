using ProjekWebShop.Models;

namespace ProjekWebShop.ViewModels
{
    public class ProductsViewModel
    {
        public List<Product> TopSellingproducts { get; set; }
        public List<Product> Newestproudcts { get; set; }
        public List<Product> Discountedprouducts { get; set; }
        public List<Product> HotProducts { get; set; }
    }
}
