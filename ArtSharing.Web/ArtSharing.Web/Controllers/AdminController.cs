using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult ManageModerators()
        {
            return View();
        }

        public IActionResult ManageCategories()
        {
            return View();
        }

        public IActionResult Reported()
        {
            return View();
        }
    }
}
