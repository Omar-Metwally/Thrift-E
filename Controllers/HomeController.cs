using Data_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Thrift_E.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly MaindbContext _context;

        public HomeController(ILogger<HomeController> logger, MaindbContext context)
        {
            _logger = logger;

            _context = context;
        }

        public IActionResult Index()
        {

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.MeasureOfScale)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Price = p.Price,
                    Image1 = p.Image1,
                    CategoryName = p.Category.CategoryName,
                    MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale
                })
                .ToList();

            return View(products);
        }


        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}