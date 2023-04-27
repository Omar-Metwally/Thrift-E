using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
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
            Order order = new Order();
            order.OrderId = LastId + 1;
            order.CustomerId = (int)person.CustomerId;
            _context.Orders.Add(order);


            foreach (Cart Item in carts)
            {
                Product product = _context.Products.FirstOrDefault(x => x.ProductId == Item.ProductId);
                if (product.InstockQty >= Item.Qty)
                {
                    OrderdProduct OrderdProduct = new OrderdProduct();
                    OrderdProduct.OrderId = order.OrderId;
                    OrderdProduct.ProductId = Item.ProductId;
                    OrderdProduct.Qty = (int)Item.Qty;
                    OrderdProduct.Total = (product.Price * OrderdProduct.Qty);
                    product.InstockQty -= OrderdProduct.Qty;
                    _context.OrderdProducts.Add(OrderdProduct);
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

            List<Order> orders = _context.Orders.Where(x => x.CustomerId == person.CustomerId).ToList();

            return View(orders);


        }

        public IActionResult Details(int OrderId)
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);

            var products = _context.OrderdProducts.Where(z => z.OrderId == OrderId).Include(c => c.Product).ThenInclude(l => l.Category).Include(p => p.Product.MeasureOfScale).Include(p => p.Order).Select(c => new OrderViewModel
            {
                CustomerId = c.Order.CustomerId,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Qty = c.Qty,
                Total = c.Product.Price * c.Qty,
                Image1 = c.Product.Image1,
                NewOrUsed = c.Product.NewOrUsed,
                CategoryName = c.Product.Category.CategoryName,
                MeasureOfScaleName = c.Product.MeasureOfScale.MeasureOfScale,
                Description = c.Product.Description,

            }).ToList();
            return View(products);
        }
    }
}
