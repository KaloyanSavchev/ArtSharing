using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using ArtSharing.ViewModels;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class LikeServiceTests
    {
        private ApplicationDbContext _context;
        private LikeService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("LikeTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new LikeService(_context);
        }

        [Test]
        public async Task ToggleLikeAsync_ShouldReturnFalseAndZero_WhenPostDoesNotExist()
        {
            // Act
            var result = await _service.ToggleLikeAsync(new LikeDto { PostId = 999 }, "user1");

            // Assert
            Assert.IsFalse(result.hasLiked);
            Assert.That(result.likeCount, Is.EqualTo(0));
        }

        [Test]
        public async Task ToggleLikeAsync_ShouldAddLike_WhenUserHasNotLiked()
        {
            // Arrange
            var post = new Post
            {
                Id = 1,
                Title = "Post 1",
                Description = "Desc",
                UserId = "author"
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleLikeAsync(new LikeDto { PostId = 1 }, "user1");

            // Assert
            Assert.IsTrue(result.hasLiked);
            Assert.That(result.likeCount, Is.EqualTo(1));

            var likeInDb = await _context.Likes.FirstOrDefaultAsync();
            Assert.IsNotNull(likeInDb);
            Assert.That(likeInDb.UserId, Is.EqualTo("user1"));
        }

        [Test]
        public async Task ToggleLikeAsync_ShouldRemoveLike_WhenUserHasAlreadyLiked()
        {
            // Arrange
            var post = new Post
            {
                Id = 1,
                Title = "Post 1",
                Description = "Desc",
                UserId = "author"
            };
            var like = new Like
            {
                UserId = "user1",
                PostId = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Posts.AddAsync(post);
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleLikeAsync(new LikeDto { PostId = 1 }, "user1");

            // Assert
            Assert.IsFalse(result.hasLiked);
            Assert.That(result.likeCount, Is.EqualTo(0));
            Assert.That(await _context.Likes.CountAsync(), Is.EqualTo(0));
        }
    }
}
