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
    public class FeedbackServiceTests
    {
        private ApplicationDbContext _context;
        private FeedbackService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FeedbackTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new FeedbackService(_context);
        }

        [Test]
        public async Task SubmitFeedbackAsync_ShouldAddFeedbackToDatabase()
        {
            // Arrange
            var model = new FeedbackInputModel
            {
                Subject = "Test Subject",
                Message = "Test Message",
                Type = "Bug Report"
            };
            var userId = "user1";

            // Act
            await _service.SubmitFeedbackAsync(model, userId);

            // Assert
            var feedbacks = await _context.Feedbacks.ToListAsync();
            Assert.That(feedbacks.Count, Is.EqualTo(1));
            Assert.That(feedbacks[0].Subject, Is.EqualTo("Test Subject"));
            Assert.That(feedbacks[0].Message, Is.EqualTo("Test Message"));
            Assert.That(feedbacks[0].Type, Is.EqualTo("Bug Report"));
            Assert.That(feedbacks[0].UserId, Is.EqualTo("user1"));
            Assert.That(feedbacks[0].Status, Is.EqualTo("Open"));
        }
    }
}
