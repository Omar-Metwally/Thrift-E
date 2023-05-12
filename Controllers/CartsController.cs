using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;

namespace Thrift_E.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        public CartsController(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        [HttpGet]
        public IActionResult pay()
        {
            int LastId = _context.Orders.OrderByDescending(x => x.OrderId).FirstOrDefault().OrderId;
            List<double> Amounts = _context.OrderdProducts.Where(x => x.OrderId == LastId).Select(x => x.Total).ToList();
            double Amount = Amounts.Sum();
            ViewBag.Amount = Amount;
            return View();
        }
        [HttpPost]
        public IActionResult pay(string cardnum, string name, string cvc, string exp, double amount)
        {
            int LastId = _context.Orders.OrderByDescending(x => x.OrderId).FirstOrDefault().OrderId;
            Payment pay = new Payment();
            pay.OrderId = LastId;
            pay.CardNumber = cardnum;
            pay.CardName = name;
            pay.ExpireDate = exp;
            pay.Cvc = cvc;
            pay.PaymentAmount = amount;
            _context.Payments.Add(pay);
            _context.SaveChanges();
            return RedirectToAction("Index", "Orders");

        }

            public IActionResult Index()
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);

            var products = _context.Carts.Where(z => z.CustomerId == person.CustomerId).Include(c => c.Product).ThenInclude(p => p.Category).Include(x => x.Product.MeasureOfScale).Select(c => new CartViewModel
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

        [HttpPost]
        public IActionResult Upsert(int ProductId , int? qty)
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);
            Cart cart = new Cart();

            cart = _context.Carts.FirstOrDefault(x => x.CustomerId == person.CustomerId && x.ProductId == ProductId);

            if (cart == null)
            {
                cart = new Cart();
                cart.CustomerId = (int)person.CustomerId;
                cart.ProductId = (int)ProductId;
                if(qty == null) cart.Qty = 1;
                else cart.Qty = qty;
                _context.Add(cart);
            }
            else
            {
                if (qty == null) cart.Qty = 1;
                else cart.Qty = cart.Qty + qty;
                _context.Update(cart);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        
        [ValidateAntiForgeryToken]
        [HttpGet]
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
            Product product = _context.Products.FirstOrDefault(x => x.ProductId == ProductId);
            if (cart.Qty < product.InstockQty)
            {
                cart.Qty += 1;
                _context.Update(cart);
                _context.SaveChanges();
            }
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
