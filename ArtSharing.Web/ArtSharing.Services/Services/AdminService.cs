using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task CreateCategoryAsync(string name, string description)
        {
            var category = new Category
            {
                Name = name,
                Description = description
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task EditCategoryAsync(Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(updatedCategory.Id);
            if (category == null) return;

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserWithRolesViewModel>> GetAllUsersWithRolesAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsBanned = user.IsBanned,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public async Task PromoteToModeratorAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Moderator") && !roles.Contains("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Moderator");
            }
        }

        public async Task RemoveModeratorRoleAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return;

            if (await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Moderator");
            }
        }

        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.TargetUser)
                .Include(r => r.TargetPost).ThenInclude(p => p.User)
                .Include(r => r.TargetComment).ThenInclude(c => c.User)
                .ToListAsync();
        }

        public async Task BanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            user.IsBanned = true;
            await _userManager.UpdateAsync(user);

            var reports = _context.Reports.Where(r => r.TargetUserId == userId);
            _context.Reports.RemoveRange(reports);

            await _context.SaveChangesAsync();
        }

        public async Task BanUserFromListAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return;

            user.IsBanned = true;
            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync();
        }

        public async Task UnbanUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return;

            user.IsBanned = false;
            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return;

            _context.Posts.Remove(post);

            var reports = _context.Reports.Where(r => r.TargetPostId == postId);
            _context.Reports.RemoveRange(reports);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return;

            _context.Comments.Remove(comment);

            var reports = _context.Reports.Where(r => r.TargetCommentId == commentId);
            _context.Reports.RemoveRange(reports);

            await _context.SaveChangesAsync();
        }

        public async Task DismissReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return;

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}
