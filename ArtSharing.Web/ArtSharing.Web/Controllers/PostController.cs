using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtSharing.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;

        public PostController(IPostService postService, UserManager<ArtSharing.Data.Models.Models.User> userManager)
        {
            _postService = postService;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var categories = await _postService.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (model.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "You must upload an image.");
            }

            if (!ModelState.IsValid)
            {
                var categories = await _postService.GetAllCategoriesAsync();
                ViewBag.CategoryId = new SelectList(categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _postService.CreatePostAsync(model, user.Id);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _postService.GetPostDetailsAsync(id.Value);
            if (post == null) return NotFound();

            ViewBag.Comments = post.Comments.Where(c => c.ParentCommentId == null).ToList();
            return View(post);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _postService.GetPostForEditAsync(id.Value, user.Id);
            if (model == null) return Forbid();

            var categories = await _postService.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", model.CategoryId);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _postService.UpdatePostAsync(id, model, user.Id);
            if (!success) return Forbid();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var post = await _postService.GetPostForDeleteAsync(id.Value, user.Id);
            if (post == null) return Forbid();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _postService.DeletePostConfirmedAsync(id, user.Id);
            if (!success) return Forbid();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreComments(int postId, int skip = 0)
        {
            var comments = await _postService.LoadMoreCommentsAsync(postId, skip);
            return PartialView("_CommentsPartial", comments);
        }
    }
}
