using ArtSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtSharing.Web.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminFeedbackController : Controller
    {
        private readonly IAdminFeedbackService _feedbackService;

        public AdminFeedbackController(IAdminFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public async Task<IActionResult> Index()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _feedbackService.UpdateStatusAsync(id, status);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _feedbackService.DeleteFeedbackAsync(id);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearResolved()
        {
            await _feedbackService.ClearResolvedFeedbacksAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
