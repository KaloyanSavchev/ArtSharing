using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                ImagePath = "/images/" + fileName,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            }; 

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Likes)
            .ThenInclude(l => l.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // Извличаме само root comments
            var rootComments = post.Comments
                .Where(c => c.ParentCommentId == null)
                .ToList();

            ViewBag.Comments = rootComments;
            return View(post);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (post.UserId != user.Id && !isAdmin)
                return Forbid();

            var viewModel = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                CategoryId = post.CategoryId,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                ImagePath = post.ImagePath
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.UserId != user.Id && !isAdmin)
                return Forbid();

            if (ModelState.IsValid)
            {
                post.Title = model.Title;
                post.Description = model.Description;
                post.CategoryId = model.CategoryId;

                _context.Update(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (post.UserId != user.Id && !isAdmin)
                return Forbid();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (post == null)
                return NotFound();

            if (post.UserId != user.Id && !isAdmin)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }     
    }
}
