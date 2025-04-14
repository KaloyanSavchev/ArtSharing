using ArtSharing.Data;
using ArtSharing.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArtSharing.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Начална страница
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(posts);
        }

        // Зареждане на още постове при скрол
        [HttpGet]
        public async Task<IActionResult> LoadMorePosts(int skip)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(6)
                .ToListAsync();

            return PartialView("_PostCardListPartial", posts); // трябва да съществува
        }

        // Филтриране по категория + търсене
        [HttpGet]
        public async Task<IActionResult> FilterPosts(string? searchTerm, int? categoryId)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p =>
                    (string.IsNullOrEmpty(searchTerm) ||
                     p.Title.Contains(searchTerm) ||
                     p.Description.Contains(searchTerm)) &&
                    (!categoryId.HasValue || p.CategoryId == categoryId.Value))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return PartialView("_PostCardListPartial", posts); // трябва да съществува
        }

        // Грешка
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
