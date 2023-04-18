using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data_Layer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thrift_E;
using Infrastructure_Layer.Models;
using Infrastructure_Layer.Repository;
using Infrastructure_Layer;

namespace Thrift_E.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Customers
        public IActionResult Index()
        {
            //return _context.Customers != null ? 
            var list = _unitOfWork.Customers.GetAll();
            return View(list);
            
        }

        // GET: Customers/Details/5
        public IActionResult Details(int? id)
        {
            var entity = _unitOfWork.Customers.Upsert(id);
            return View(entity);
            
        }

        public IActionResult Delete(int? id)
        {
            _unitOfWork.Customers.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
         }


        // POST: Create/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(int? id,Customer customer)
        {
            _unitOfWork.Customers.Upsert(id, customer);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }



        // Get: Create/Edit
        [HttpGet]
        public  IActionResult Upsert(int? id)
        {
            var entity = _unitOfWork.Customers.Upsert(id);
            return View(entity);

        }

    }
}
