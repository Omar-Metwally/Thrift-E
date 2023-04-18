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
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        public ProductsController(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {
            var query = from c in _context.Categorys
                        join p in _context.Products on c.CategoryId equals p.CategoryId
                        join m in _context.MeasuresOfScales on p.MeasureOfScaleId equals m.MeasureOfScaleId
                        join i in _context.Images on p.ProductId equals i.ProductId
                        select new
                        {
                            CategoryName = c.CategoryName,
                            ProductName = p.ProductName,
                            ProductId = p.ProductId,
                            Price = p.Price,
                            Instock = p.Instock,
                            InstockQty = p.InstockQty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed,
                            Image = i.Image1
                        };
            return View(query.ToList<object>());
        }

        public IActionResult Delete(int? id)
        {
            _unitOfWork.Products.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Upsert(int? id)
        {
            ViewBag.Categories = new SelectList(_context.Categorys.ToList(), "CategoryId", "CategoryName");
            ViewBag.MeasureOfScales = new SelectList(_context.MeasuresOfScales.ToList(), "MeasureOfScaleId", "MeasureOfScale");
            var entity = _unitOfWork.Products.Upsert(id);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(int? id,Product product)
        {
            {
                product.SignupDate = DateTime.Now;
                if (product.InstockQty > 0) { product.Instock = true; }
                var entity = _unitOfWork.Products.Upsert(id, product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Details(int? id)
        {
            var query = from c in _context.Categorys
                        join p in _context.Products on c.CategoryId equals p.CategoryId where p.ProductId == id
                        join m in _context.MeasuresOfScales on p.MeasureOfScaleId equals m.MeasureOfScaleId where p.ProductId == id
                        join i in _context.Images on p.ProductId equals i.ProductId  where p.ProductId == id
                        select new
                        {
                            CategoryName = c.CategoryName,
                            ProductName = p.ProductName,
                            ProductId = p.ProductId,
                            Price = p.Price,
                            Instock = p.Instock,
                            InstockQty = p.InstockQty,
                            MeasureOfScale = m.MeasureOfScale,
                            NewOrUsed = p.NewOrUsed,
                            Image = i.Image1
                        };
            return View(query.FirstOrDefault());

        }







    }
}
