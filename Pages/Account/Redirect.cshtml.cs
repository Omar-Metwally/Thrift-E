using Data_Layer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Main.Pages.Account
{
    public class RedirectModel : PageModel
    {
        private readonly MaindbContext _context;

        public RedirectModel(MaindbContext context)
        {
            _context = context;
        }
        public IActionResult OnGet()
        {
            int? PersonId = TempData["PersonId"] as int?;
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
            var person = _context.Customers.FirstOrDefault(x => x.CustomerId == PersonId);
            person.Cookie = myCookieValue;
            _context.Update(person);
            _context.SaveChanges();
            return RedirectToPage("../../Views/Home/Index.cshtml");
        }

    }
}
