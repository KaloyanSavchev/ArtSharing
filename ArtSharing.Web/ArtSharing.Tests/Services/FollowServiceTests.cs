using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class FollowServiceTests
    {
        private ApplicationDbContext _context;
        private FollowService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FollowTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new FollowService(_context);
        }

        [Test]
        public async Task ToggleFollowAsync_ShouldReturnFalse_WhenUserTriesToFollowSelf()
        {
            // Act
            var result = await _service.ToggleFollowAsync("user1", "user1");

            // Assert
            Assert.IsFalse(result);
            Assert.That(await _context.UserFollows.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task ToggleFollowAsync_ShouldAddFollow_WhenNotFollowing()
        {
            // Act
            var result = await _service.ToggleFollowAsync("user1", "user2");

            // Assert
            Assert.IsTrue(result);
            var follow = await _context.UserFollows.FirstOrDefaultAsync();
            Assert.IsNotNull(follow);
            Assert.That(follow.FollowerId, Is.EqualTo("user1"));
            Assert.That(follow.FollowingId, Is.EqualTo("user2"));
        }

        [Test]
        public async Task ToggleFollowAsync_ShouldRemoveFollow_WhenAlreadyFollowing()
        {
            // Arrange
            var existing = new UserFollow { FollowerId = "user1", FollowingId = "user2" };
            await _context.UserFollows.AddAsync(existing);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleFollowAsync("user1", "user2");

            // Assert
            Assert.IsTrue(result);
            Assert.That(await _context.UserFollows.CountAsync(), Is.EqualTo(0));
        }
    }
}
