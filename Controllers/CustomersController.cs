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
        private readonly MaindbContext _context;

        public CustomersController(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }


        // GET: Customers
        public async Task<IActionResult> Index()
        {
              return _context.Customers != null ? 
                          View(await _context.Customers.ToListAsync()) :
                          Problem("Entity set 'MaindbContext.Customers'  is null.");
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }


        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'MaindbContext.Customers'  is null.");
            }
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
          return (_context.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }


        // POST: Create/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(int? id,Customer customer)
        {
            _unitOfWork.Customers.Upsert(id, customer);
            return RedirectToAction(nameof(Index));
        }


        /*public IActionResult Upsert(int? id, [Bind("CustomerId,FirstName,MiddleName,LastName,Phone,Email,Password,CreateDate,CreditDebitCard")] Customer customer)
        {

            if (id == 0)
            {
                    _context.Add(customer);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
  
            }
            else
            {

                    _context.Update(customer);
                    _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

        }*/

        // Get: Create/Edit
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return View();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

    }
}
