using Microsoft.AspNetCore.Mvc;
using ArtSharing.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> All()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
    }
}
