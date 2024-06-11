using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjekWebShop.Data;
using ProjekWebShop.Data.Migrations;
using ProjekWebShop.Models;
using ProjekWebShop.ViewModels;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ProjekWebShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ProductsViewModel productsViewModel = new ProductsViewModel();
            productsViewModel.TopSellingproducts = 
                _context.Products.Include(x=>x.ProductImages)
                .OrderByDescending(x=>x.NuberOfSoldItems)
                .Take(4).ToList();
            productsViewModel.Newestproudcts = _context.Products.Include(x=>x.ProductImages)
                .OrderByDescending(x=>x.AddedDate)
                .Take(4).ToList();
            productsViewModel.Discountedprouducts = _context.Products.Include(x=> x.ProductImages)
                .Where(x=>x.IsDiscount==true)
                .OrderBy(x=>Guid.NewGuid())
                .Take(4).ToList();
            productsViewModel.HotProducts =_context.Products.Include(x=>x.ProductImages)
                .Where (x=>x.IsHot==true)
                .OrderBy(x=>Guid.NewGuid())
                .Take (4).ToList();
            return View(productsViewModel);
        }
        [Authorize(Policy = "ZahtjevajEmailDavid")]
        public IActionResult ControlPanel()
        {
            return View();
        }

        public IActionResult Categories()
        {
            var listKategorije = _context.Categories.ToList();
            return View(listKategorije);
        }
        public IActionResult Products(int? categoryId, string searchTerm , string sortOrder, int? page)
        {
            var upit = _context.Products.Include(x=>x.ProductImages).AsQueryable();
            if (categoryId != null)
            {
                upit = _context.Products.Include(x=>x.ProductImages).Where(x=>x.CategoryId==categoryId);
            }
            if(!string.IsNullOrEmpty(searchTerm))
            {
                upit = upit.Where(x=>x.Name.Contains(searchTerm) || x.Description.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    upit = upit.OrderByDescending(x => x.Name);
                    break;
                case "price":
                    upit = upit.OrderBy(x => x.Price);
                    break;
                case "price_desc":
                    upit = upit.OrderByDescending(x => x.Price);
                    break;
                case "newest":
                    upit = upit.OrderByDescending(x => x.AddedDate);
                    break;
                case "oldest":
                    upit = upit.OrderBy(x => x.AddedDate);
                    break;
                default:
                    upit = upit.OrderBy(x => x.Name);
                    break;
            }
            var listaProizvoda = upit.ToList();
            int pageSize = 1;
            int pagenumber = page ?? 1;

            int numberOfItem = listaProizvoda.Count();
            var listaPoStranici = listaProizvoda.Skip((pagenumber-1)*pageSize).Take(pageSize); 

            ViewBag.CategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pagenumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItem = numberOfItem;
            ViewBag.TotalPages = (int) Math.Ceiling((double)numberOfItem / pageSize);
            return View(listaProizvoda.ToList());
        }

        public IActionResult ProductDetails(int productId)
        {
            var proizvod = _context.Products.Include(x => x.ProductImages).Where(x => x.Id == productId).FirstOrDefault();
            if(proizvod == null)
            {
                return NotFound();
            }
            return View(proizvod);
        }

        public IActionResult AddToCart(int productId)
        {
            var caetJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;
            if (!string.IsNullOrEmpty(caetJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(caetJson);
            }
            else
            {
                cart = new List<CartItem>();
            }

            var cartItem = cart.FirstOrDefault(x =>x.ProductId == productId);
            if(cartItem != null)
            {
                cartItem.Quantity++;

            }
            else
            {
                cart.Add(new CartItem { ProductId = productId, Quantity = 1 });
            }

            caetJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", caetJson);

            return RedirectToAction("Index");
        }

        public IActionResult Cart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!String.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }

            var products = _context.Products.Include(x => x.ProductImages)
                .Where(x => cart.Select(p => p.ProductId).Contains(x.Id))
                .ToList();

            var cartViewModel = cart.Select(x => new CartItemViewModel
            {
                Product = products.First(p => p.Id == x.ProductId),
                Quantity = x.Quantity
            }).ToList();

            return View(cartViewModel);
        }
        [HttpPost]
        public IActionResult UpdateCart(int productId,int quantity)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }

            var cartItem = cart.FirstOrDefault(x=>x.ProductId == productId);
            if (cartItem != null && quantity>0)
            {
                cartItem.Quantity = quantity;
            }
            else if (cartItem != null && quantity <=0) 
            {
                cart.Remove(cartItem);
            }
            else
            {
                cart.Add(cartItem);
            }

            cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }
            var cartItem = cart.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem != null)
            {
                cart.Remove(cartItem );
            }
            cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart",cartJson);

            return RedirectToAction("Cart");
        }
        [Authorize]
        public IActionResult Order()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }
            var products = _context.Products.Include(x => x.ProductImages)
             .Where(x => cart.Select(p => p.ProductId).Contains(x.Id))
             .ToList();

            var cartViewModel = cart.Select(x => new CartItemViewModel
            {
                Product = products.First(p => p.Id == x.ProductId),
                Quantity = x.Quantity
            }).ToList();
            return View(cartViewModel);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Order (string name,string lastName,string address,string city,string phone,int zip, int delivery)
        {
            var userId = _userManager.GetUserId(User);
            Order order = new Order
            {
                UserId = userId,
                Name = name,
                LastName = lastName,
                Address = address,
                City = city,
                Phone = phone,
                Zip = zip,
                Delivery = delivery
            };
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }
            foreach (var item in cart)
            {
                var product  = _context.Products.FirstOrDefault(x=>x.Id == item.ProductId);
                if (product != null)
                {
                    product.NuberOfSoldItems += item.Quantity;
                    item.Price = product.Price;
                }
            }

            order.Items = cart;
            _context.Orders.Add(order);
            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");
            return View("ThankYou");
        }
        [Authorize(Policy = "ZahtjevajEmailDavid")]
        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(x=>x.Items)
                .ThenInclude(t=>t.Product).ToList();

            return View(orders);
        }
        [Authorize]
        public IActionResult MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _context.Orders
                .Include(x => x.Items)
                .ThenInclude(t => t.Product)
                .Where(c=>c.UserId==userId)
                .ToList();

            return View(orders);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
