using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Thrift_E.Controllers
{
    public class CartsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        public CartsController(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {

            var products = _context.Carts.Include(c => c.Product).ThenInclude(p => p.Category).Include(x => x.Product.MeasureOfScale)

    .Select(c => new CartViewModel
    {
        ProductId = c.ProductId,
        ProductName = c.Product.ProductName,
        Qty = c.Qty,
        Price = (double)(c.Product.Price * c.Qty),
        NewOrUsed = c.Product.NewOrUsed,
        Image1 = c.Product.Image1,
        CategoryName = c.Product.Category.CategoryName,
        MeasureOfScaleName = c.Product.MeasureOfScale.MeasureOfScale
    })
    .ToList();

            return View(products);

        }

        public IActionResult Delete(int? id)
        {
            _unitOfWork.Carts.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Upsert(int? id)
        {
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            if (id == null)
            {
                return View();
            }
            else
            {
                var entity = _context.Carts.FirstOrDefault(x => x.ProductId == id && x.CustomerId == 1);
                return View(entity);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(int? id, Cart cart)
        {
            {
                var entity = _unitOfWork.Carts.Upsert(id, cart);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
        }







    }
}
