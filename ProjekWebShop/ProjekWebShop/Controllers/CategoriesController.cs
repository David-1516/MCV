using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjekWebShop.Data;
using ProjekWebShop.Models;

namespace ProjekWebShop.Controllers
{
    [Authorize(Policy = "ZahtjevajEmailDavid")]
    public class CategoriesController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index() // Prikazuje listu svih kategorija
        {
            var listaKategorija = _context.Categories.ToList();
            return View(listaKategorija);
        }

        public IActionResult Create()//Forama za kreiranje kateorije
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category, IFormFile image)//sprema sve nove kategorije
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                    category.ImagePath = uniqueFileName;
                }
                _context.Categories.Add(category);
                _context.SaveChanges();
             
            }
            return RedirectToAction("Index");
        }
    }
}
