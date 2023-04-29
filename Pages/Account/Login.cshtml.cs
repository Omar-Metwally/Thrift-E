using Data_Layer;
using Infrastructure_Layer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Main.Views.Pages.Account
{

    public class LoginModel : PageModel
    {

        private readonly IUnitOfWork _unitOfWork;

        private readonly MaindbContext _context;

        public LoginModel(IUnitOfWork unitOfWork, MaindbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }


        [BindProperty]
        public Credential Credential { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {


            if (!ModelState.IsValid);
            var person = _context.Customers.FirstOrDefault(x => x.Email == Credential.Email);
            if(person == null)
            {
                ModelState.AddModelError("Credential.Email","Please check the entered the email");
                return Page();
            }
            else if (person.Admin == true && person.Password == Credential.Password)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim("Admin" , "1") };
                var identity = new ClaimsIdentity(claims, "MyCookie");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookie", claimsPrincipal);

                TempData["PersonId"] = person.CustomerId;
                return RedirectToPage("Redirect");
            }
            else if(person.Password == Credential.Password)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "Customer"),
                    new Claim("Customer" , "1") };
                var identity = new ClaimsIdentity(claims, "MyCookie");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookie", claimsPrincipal);

                TempData["PersonId"] = person.CustomerId;
                return RedirectToPage("Redirect");

            }
            ModelState.AddModelError("Credential.Password", "Wrong Password");
            return Page();
        }
    }

    public class Credential
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
