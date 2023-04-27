using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var products = _context.Carts.Include(c => c.Product).ThenInclude(p => p.Category).Include(x => x.Product.MeasureOfScale).Select(c => new CartViewModel
            {
                CustomerId = c.CustomerId,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Qty = c.Qty,
                Price = c.Product.Price,
                NewOrUsed = c.Product.NewOrUsed,
                Image1 = c.Product.Image1,
                CategoryName = c.Product.Category.CategoryName,
                MeasureOfScaleName = c.Product.MeasureOfScale.MeasureOfScale
            }).ToList();

            return View(products);

        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult Delete(int? id)
        {
            _unitOfWork.Carts.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Upsert(int Id)
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);
            Cart cart = new Cart();

            cart = _context.Carts.FirstOrDefault(x => x.CustomerId == person.CustomerId && x.ProductId == Id);

            if (cart == null)
            {
                cart = new Cart();
                cart.CustomerId = (int)person.CustomerId;
                cart.ProductId = (int)Id;
                cart.Qty = 1;
                _context.Add(cart);
            }
            else
            {
                cart.Qty = cart.Qty + 1;
                _context.Update(cart);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

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
        [HttpPost]
        public IActionResult Plus(int ProductId, int CustomerId)
        {
            Cart cart = new Cart();
            cart = _context.Carts.FirstOrDefault(x => x.CustomerId == CustomerId && x.ProductId == ProductId);
            cart.Qty += 1;
            _context.Update(cart);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult Minus(int ProductId, int CustomerId)
        {
            Cart cart = new Cart();
            cart = _context.Carts.FirstOrDefault(x => x.CustomerId == CustomerId && x.ProductId == ProductId);
            if (cart.Qty - 1 > 0)
            {
                cart.Qty -= 1;
                _context.Update(cart);
            }
            else
            {
                _context.Remove(cart);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
