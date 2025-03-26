using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArtSharing.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // 👉 Роли
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // 👉 Админ акаунт
            var adminEmail = "admin@artsharing.com";
            var adminPassword = "Admin@12345";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                // 🔥 Изтриваме всички постове на админа, ако има
                var postsByAdmin = context.Posts.Where(p => p.UserId == existingAdmin.Id);
                context.Posts.RemoveRange(postsByAdmin);
                await context.SaveChangesAsync();

                // ❌ След това изтриваме самия админ
                await userManager.DeleteAsync(existingAdmin);
            }

            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Admin user recreated successfully.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"❌ Error creating admin: {error.Description}");
                }
            }

            // 👉 Категории (ако липсват)
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Photography", Description = "Photos and visual captures" },
                    new Category { Name = "Digital Art", Description = "Art created using digital tools" },
                    new Category { Name = "Traditional Art", Description = "Paintings, sketches and physical media" },
                    new Category { Name = "Nature", Description = "Art featuring natural elements" }
                );
                await context.SaveChangesAsync();
            }

            // 👉 Постове
            if (!context.Posts.Any())
            {
                var category = await context.Categories.FirstOrDefaultAsync();

                if (category != null)
                {
                    context.Posts.Add(new Post
                    {
                        Title = "Test Post",
                        Description = "This is a seed test post.",
                        ImageUrl = "https://source.unsplash.com/random/800x600",
                        CreatedAt = DateTime.UtcNow,
                        UserId = adminUser.Id,
                        CategoryId = category.Id
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
