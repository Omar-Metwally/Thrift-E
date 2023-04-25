using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure_Layer.Repository;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Data_Layer;
using Infrastructure_Layer;
using Microsoft.Extensions.Options;

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


            if (!ModelState.IsValid) return Page();
            var person = _context.Customers.FirstOrDefault(x => x.Email == Credential.Email);
            if (person.Admin == true && person.Password == Credential.Password)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim("Admin" , "1") };
                var identity = new ClaimsIdentity(claims, "MyCookie");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                
                await HttpContext.SignInAsync("MyCookie", claimsPrincipal);
                var cookie = HttpContext.Request.Cookies["MyCookie"];
                save();
                return RedirectToPage("../../Views/Home/Index.cshtml");
            }
            if (person.Password == Credential.Password)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "Customer"),
                    new Claim("Customer" , "1") };
                var identity = new ClaimsIdentity(claims, "MyCookie");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
                var tooken = identity.BootstrapContext = true;

                await HttpContext.SignInAsync("MyCookie", claimsPrincipal);

                //Response.Cookies.Append("MyCookie", person.CustomerId.ToString());

                return RedirectToPage("../../Views/Home/Index.cshtml");

            }
            return Page();
        } 
        public void save()
        {
            string myCookieValue = HttpContext.Request.Cookies["MyCookie"];
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
