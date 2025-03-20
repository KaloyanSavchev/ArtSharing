using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        await context.Database.MigrateAsync(); // Гарантира, че миграциите са приложени

        // Seed roles
        string[] roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@artsharing.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed categories
        if (!context.Categories.Any())
        {
            var categories = new[]
 {
                new Category { Name = "Photography", Description = "All about photography" },
                new Category { Name = "Digital Art", Description = "Modern digital artworks" },
                new Category { Name = "Traditional Art", Description = "Classic painting and sketching" },
                new Category { Name = "Nature", Description = "Art inspired by nature" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed posts
        if (!context.Posts.Any())
        {
            var user = await userManager.FindByEmailAsync(adminEmail);
            var category = await context.Categories.FirstOrDefaultAsync();
            if (user != null && category != null)
            {
                var posts = new[]
                {
                    new Post
                    {
                        Title = "The Rabbit and the Bear",
                        Description = "OMG look at this cute bunny I drew, it has a carrot <3!",
                        ImageUrl = "https://source.unsplash.com/400x300/?art,drawing",
                        CreatedAt = DateTime.UtcNow,
                        UserId = user.Id,
                        CategoryId = category.Id
                    }
                };
                context.Posts.AddRange(posts);
                await context.SaveChangesAsync();
            }
        }
    }
}
