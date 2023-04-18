using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thrift_E;
using Infrastructure_Layer.Models;
using Infrastructure_Layer.Repository;
using System.Linq;
using Infrastructure_Layer;
using Microsoft.AspNetCore.Mvc;
using Data_Layer;
using Microsoft.CodeAnalysis;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var query = from c in _context.Categorys
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
                            Total = Int32.Parse(p.Price) * ca.Qty,
                            Qty = ca.Qty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed
                        };
            return View(query.ToList<object>());
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
            var entity = _unitOfWork.Carts.Upsert(id);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(int? id,Cart cart)
        {
            {
                var entity = _unitOfWork.Carts.Upsert(id, cart);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
        }







    }
}
