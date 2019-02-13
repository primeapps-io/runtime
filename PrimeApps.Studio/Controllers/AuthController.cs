using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PrimeApps.Studio.Controllers
{
    public class AuthController : Controller
    {
        public async Task<ActionResult> SignOut()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            return RedirectToAction("Index", "Home");
        }
    }
}