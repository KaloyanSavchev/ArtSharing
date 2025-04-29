using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class PostHomeServiceTests
    {
        private ApplicationDbContext _context;
        private PostHomeService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("PostHomeTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new PostHomeService(_context);
        }

        [Test]
        public async Task GetInitialPostsAsync_ShouldReturnSortedAndLimitedPosts()
        {
            // Arrange
            var user = new User { Id = "u1", UserName = "tester" };
            var category = new Category { Id = 1, Name = "TestCat" };
            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var posts = new List<Post>
    {
        new Post
        {
            Title = "Oldest",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UserId = user.Id,
            User = user,
            CategoryId = category.Id,
            Category = category,
            Description = "desc",
            PostImages = new List<PostImage>()
        },
        new Post
        {
            Title = "Middle",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UserId = user.Id,
            User = user,
            CategoryId = category.Id,
            Category = category,
            Description = "desc",
            PostImages = new List<PostImage>()
        },
        new Post
        {
            Title = "Newest",
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            User = user,
            CategoryId = category.Id,
            Category = category,
            Description = "desc",
            PostImages = new List<PostImage>()
        }
    };

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            var service = new PostHomeService(_context);

            // Act
            var result = await service.GetInitialPostsAsync(take: 2);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo("Newest"));
            Assert.That(result[1].Title, Is.EqualTo("Middle"));
        }


        [Test]
        public async Task LoadMorePostsAsync_ShouldSkipAndTakeCorrectly()
        {
            // Arrange
            var user = new User { Id = "u1", UserName = "tester" };
            var category = new Category { Id = 1, Name = "TestCat" };
            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var posts = new List<Post>();

            for (int i = 1; i <= 5; i++)
            {
                posts.Add(new Post
                {
                    Title = $"Post {i}",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UserId = user.Id,
                    User = user,
                    CategoryId = category.Id,
                    Category = category,
                    Description = "desc",
                    PostImages = new List<PostImage>()
                });
            }

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            var service = new PostHomeService(_context);

            // Act
            var result = await service.LoadMorePostsAsync(skip: 2, take: 2);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo("Post 3"));
            Assert.That(result[1].Title, Is.EqualTo("Post 4"));
        }


        [Test]
        public async Task FilterPostsAsync_ShouldReturnPostsMatchingSearchTerm()
        {
            // Arrange
            var user = new User { Id = "u1", UserName = "tester" };
            var category = new Category { Id = 1, Name = "TestCat" };

            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var matchingPost = new Post
            {
                Title = "SpecialKeyword",
                Description = "Something here",
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                User = user,
                CategoryId = category.Id,
                Category = category,
                PostImages = new List<PostImage>()
            };

            var otherPost = new Post
            {
                Title = "Nothing here",
                Description = "Completely unrelated",
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                User = user,
                CategoryId = category.Id,
                Category = category,
                PostImages = new List<PostImage>()
            };

            await _context.Posts.AddRangeAsync(matchingPost, otherPost);
            await _context.SaveChangesAsync();

            var service = new PostHomeService(_context);

            // Act
            var result = await service.FilterPostsAsync("SpecialKeyword", null);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("SpecialKeyword"));
        }


        [Test]
        public async Task FilterPostsAsync_ShouldReturnPostsByCategoryId()
        {
            // Arrange
            var category = new Category { Name = "Painting" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var user = new User { Id = "u1", UserName = "testuser" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Title = "Test Post",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                CategoryId = category.Id,
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var service = new PostHomeService(_context);

            // Act
            var result = await service.FilterPostsAsync(null, category.Id);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Test Post"));
            Assert.That(result[0].CategoryId, Is.EqualTo(category.Id));
        }


        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            await _context.Categories.AddRangeAsync(
                new Category { Name = "1" },
                new Category { Name = "2" }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetAllCategoriesAsync();

            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}
