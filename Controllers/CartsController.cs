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

           /*var query = from c in _context.Categorys
                        join p in _context.Products on c.CategoryId equals p.CategoryId
                        join m in _context.MeasuresOfScales on p.MeasureOfScaleId equals m.MeasureOfScaleId
                        join ca in _context.Carts on p.ProductId equals ca.ProductId
                        join cu in _context.Customers on ca.CustomerId equals cu.CustomerId //where ca.CustomerId == id
                        select new
                        {
                            CartId = ca.CartId,
                            CategoryName = c.CategoryName,
                            ProductName = p.ProductName,
                            ProductId = p.ProductId,
                            Price = p.Price,
                            Total = p.Price * ca.Qty,
                            Qty = ca.Qty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed
                        };*/
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


            /*var products = _context.Carts
                .Include(p => p.Product)
                    .Include(p => p.Category)
    .Include(p => p.MeasureOfScale)
    .Select(p => new CartViewModel
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        Qty = p.Qty,
        Price = p.Price,
        NewOrUsed = p.NewOrUsed,
        Image1 = p.Image1,
        CategoryName = p.Category.CategoryName,
        MeasureOfScaleName = p.MeasureOfScale.MeasureOfScale
    });*/

            //return View(products);
            /*var query = from c in _context.Categorys
                        join p in _context.Products on c.CategoryId equals p.CategoryId
                        join m in _context.MeasuresOfScales on p.MeasureOfScaleId equals m.MeasureOfScaleId
                        join ca in _context.Carts on p.ProductId equals ca.ProductId
                        select new
                        {
                            CartId = ca.CartId,
                            CategoryName = c.CategoryName,
                            ProductName = p.ProductName,
                            ProductId = p.ProductId,
                            Price = p.Price,
                            Total = p.Price * ca.Qty,
                            Qty = ca.Qty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed
                        };
            return View(query.ToList<object>());*/
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
            //var entity = _unitOfWork.Carts.Upsert(id);
            //return View(entity);
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
