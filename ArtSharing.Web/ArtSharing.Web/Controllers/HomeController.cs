using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ArtSharing.Web.Models;

namespace ArtSharing.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostHomeService _postHomeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPostHomeService postHomeService, ILogger<HomeController> logger)
        {
            _postHomeService = postHomeService;
            _logger = logger;
        }
        public IActionResult Rules()
        {
            return View();
        }

        public async Task<IActionResult> Index(string sortOrder = "newest")
        {
            var posts = await _postHomeService.GetInitialPostsAsync(6, sortOrder);
            var categories = await _postHomeService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SortOrder = sortOrder;
            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMorePosts(int skip = 0, int take = 6, string sortOrder = "newest")
        {
            var posts = await _postHomeService.LoadMorePostsAsync(skip, take, sortOrder);
            return PartialView("_PostCardListPartial", posts);
        }

        [HttpGet]
        public async Task<IActionResult> FilterPosts(string? searchTerm, int? categoryId, string sortOrder = "newest")
        {
            var posts = await _postHomeService.FilterPostsAsync(searchTerm, categoryId, sortOrder);
            return PartialView("_PostCardListPartial", posts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
