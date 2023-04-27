using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static NuGet.Packaging.PackagingConstants;

namespace Thrift_E.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        public OrdersController(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public IActionResult CreateOrder()
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);

            List<Cart> carts = _context.Carts.Where(x => x.CustomerId == person.CustomerId).ToList();
            int LastId = _context.Orders.OrderByDescending(x => x.OrderId).FirstOrDefault().OrderId;
            foreach (Cart Item in carts)
            {
                Product product = _context.Products.FirstOrDefault(x => x.ProductId == Item.ProductId);
                if (product.InstockQty >= Item.Qty)
                {
                    Order order = new Order();
                    order.OrderId = LastId + 1;
                    order.ProductId = Item.ProductId;
                    order.CustomerId = Item.CustomerId;
                    order.Qty = (int)Item.Qty;
                    order.Total = (product.Price * order.Qty);
                    product.InstockQty -= order.Qty;
                    _context.Orders.Add(order);
                    _context.Carts.Remove(Item);
                }
                else 
                {
                    return RedirectToAction("Index", "Carts");
                }
                
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);

            var products = _context.Orders.Where(z => z.CustomerId == person.CustomerId).Include(c => c.Product).ThenInclude(p => p.Category).Include(x => x.Product.MeasureOfScale).Select(c => new OrderViewModel
            {
                CustomerId = c.CustomerId,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Qty = c.Qty,
                Total = c.Product.Price * c.Qty,
                Image1 = c.Product.Image1,
                NewOrUsed = c.Product.NewOrUsed,
                CategoryName = c.Product.Category.CategoryName,
                MeasureOfScaleName = c.Product.MeasureOfScale.MeasureOfScale
            }).ToList();
            return View(products);
        }
    }
}
