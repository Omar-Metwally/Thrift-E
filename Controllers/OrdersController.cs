﻿using Data_Layer;
using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static NuGet.Packaging.PackagingConstants;

namespace Thrift_E.Controllers
{
    [Authorize]
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
            Order order = new Order();
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

            var products = _context.OrderdProducts.Where(z => z.OrderId == OrderId).Include(c => c.Product).ThenInclude(l => l.Category).Include(p => p.Product.MeasureOfScale).Include(x => x.Product.Brand).Include(p => p.Order).Select(c => new OrderViewModel
            {
                CustomerId = c.Order.CustomerId,
                CustomerName = person.FirstName,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Qty = c.Qty,
                Total = c.Product.Price * c.Qty,
                Image1 = c.Product.Image1,
                NewOrUsed = c.Product.NewOrUsed,
                CategoryName = c.Product.Category.CategoryName,
                MeasureOfScaleName = c.Product.MeasureOfScale.MeasureOfScale,
                Description = c.Product.Description,
                OrderStatus = c.Order.OrderStatus,
                Brand = c.Product.Brand.BrandName,
                OrderDate = c.Order.OrderDate,
                Size = c.Product.Size,
                Visa = c.Order.PayedByVisa

            }).ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult Confirm1(string Area, string Streat, string House,bool paymethod,int visanumber)
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.Cookie == myCookieValue);

            int LastId = _context.Orders.OrderByDescending(x => x.OrderId).FirstOrDefault().OrderId;
            List<Cart> carts = _context.Carts.Where(x => x.CustomerId == person.CustomerId).ToList();
            Order order = new Order();
            order.CustomerId = (int)person.CustomerId;
            order.OrderId = LastId + 1;
            order.Area = Area;
            order.Streat = Streat;
            order.House = House;
            order.PayedByVisa = paymethod;
            order.OrderStatus = "Being Processed";
            order.OrderDate = DateTime.Now;
            _context.Orders.Add(order);
            if (carts.Count != 0)
            {
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
                if (paymethod == true)
                {
                    return RedirectToAction("pay", "Carts");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index", "Carts");
            }
        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult ChangeStatus(string Status, int OrderId)
        {
            var order = _context.Orders.FirstOrDefault(x => x.OrderId == OrderId);
            order.OrderStatus = Status;
            _context.Orders.Update(order);
            _context.SaveChanges();
            return RedirectToAction("AllOrders");
        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult Delete(int OrderId)
        {
            var order = _context.Orders.FirstOrDefault(x => x.OrderId == OrderId);
            List<OrderdProduct> orderdproducts = _context.OrderdProducts.Where(x => x.OrderId == OrderId).ToList();
            _context.OrderdProducts.RemoveRange(orderdproducts);
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return RedirectToAction("AllOrders");
        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult AllOrders()
        {
            var orders = _context.Orders.Include(x => x.Customer).Select(c => new OrderViewModel
            {
                CustomerName = c.Customer.FirstName + " " + c.Customer.MiddleName + " " + c.Customer.LastName,
                Phone = c.Customer.Phone,
                OrderId = c.OrderId,
                OrderStatus = c.OrderStatus,
                OrderDate = c.OrderDate,
                Location = c.Area + " / " + c.Streat + " / " + c.House,

            }).ToList();

            return View(orders);
        }

    }
}
