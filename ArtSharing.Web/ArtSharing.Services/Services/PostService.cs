using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Interfaces;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace ArtSharing.Services.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PostService(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task CreatePostAsync(PostCreateViewModel model, string userId)
        {
            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); 

            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in model.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var postImage = new PostImage
                        {
                            PostId = post.Id,
                            ImagePath = "/uploads/" + uniqueFileName
                        };

                        _context.PostImages.Add(postImage);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<Post?> GetPostDetailsAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.PostImages)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<EditPostViewModel?> GetPostForEditAsync(int id, string userId)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.FindByIdAsync(userId);

            if (post == null || user == null)
                return null;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (post.UserId != user.Id && !isAdmin)
                return null;

            return new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                CategoryId = post.CategoryId,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
            };
        }

        public async Task<bool> UpdatePostAsync(int id, EditPostViewModel model, string userId)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.FindByIdAsync(userId);

            if (post == null || user == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (post.UserId != user.Id && !isAdmin)
                return false;

            post.Title = model.Title;
            post.Description = model.Description;
            post.CategoryId = model.CategoryId;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Post?> GetPostForDeleteAsync(int id, string userId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            var user = await _userManager.FindByIdAsync(userId);

            if (post == null || user == null)
                return null;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (post.UserId != user.Id && !isAdmin)
                return null;

            return post;
        }

        public async Task<bool> DeletePostConfirmedAsync(int id, string userId)
        {
            var post = await _context.Posts.FindAsync(id);
            var user = await _userManager.FindByIdAsync(userId);

            if (post == null || user == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (post.UserId != user.Id && !isAdmin)
                return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Comment>> LoadMoreCommentsAsync(int postId, int skip = 0)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .OrderBy(c => c.CreatedAt)
                .Skip(skip)
                .Take(3)
                .ToListAsync();
        }
    }
}
