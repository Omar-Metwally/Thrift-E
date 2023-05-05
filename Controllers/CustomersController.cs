using Infrastructure_Layer;
using Infrastructure_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Thrift_E.Controllers
{

    public class CustomersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize(policy: "MustBeAdmin")]
        // GET: Customers
        public IActionResult Index()
        {
            //return _context.Customers != null ? 
            var list = _unitOfWork.Customers.GetAll();
            return View(list);

        }
        [Authorize(policy: "MustBeAdmin")]
        public IActionResult Delete(int? id)
        {
            _unitOfWork.Customers.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        // POST: Create/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(int? id, Customer customer)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Customers.Upsert(id, customer);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View("Signup");
        }

        // Get: Create/Edit
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            var entity = _unitOfWork.Customers.Upsert(id);
            return View(entity);

        }
        public IActionResult Signup()
        {
            return View();
        }

    }
}
