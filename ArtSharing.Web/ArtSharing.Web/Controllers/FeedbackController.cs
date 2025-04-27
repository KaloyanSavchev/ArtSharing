using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly UserManager<ArtSharing.Data.Models.Models.User> _userManager;

        public FeedbackController(IFeedbackService feedbackService, UserManager<ArtSharing.Data.Models.Models.User> userManager)
        {
            _feedbackService = feedbackService;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _feedbackService.SubmitFeedbackAsync(model, user.Id);

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Create");
        }
    }
}