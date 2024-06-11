using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjekWebShop.Data;
using ProjekWebShop.Models;

namespace ProjekWebShop.Controllers
{
    [Authorize(Policy = "ZahtjevajEmailDavid")]
    public class ProductsController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            var listaproizvoda = _context.Products.Include(x=>x.ProductImages).ToList();

            return View(listaproizvoda);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var listaKategorija = _context.Categories.ToList();
            ViewBag.Categories = listaKategorija;
            return View();
        }

        public IActionResult Create(Product product, List<IFormFile> images, int CategoryId)
        {
            if (ModelState.IsValid)
            {
                var category = _context.Categories.Where(x => x.Id == CategoryId).FirstOrDefault();
                if (category == null) { 
     
                    return NotFound();
                }
                product.AddedDate = DateTime.Now;
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        var uniqieFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqieFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }
                        ProductImage productImage = new ProductImage();
                        productImage.ImagePath = uniqieFileName;
                        product.ProductImages.Add(productImage);


                    }
                   
                }
                category.Products.Add(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
