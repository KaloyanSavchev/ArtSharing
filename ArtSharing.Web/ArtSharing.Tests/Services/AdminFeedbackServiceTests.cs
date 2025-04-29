using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using ArtSharing.Data.Models.Models;


namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class AdminFeedbackServiceTests
    {
        private ApplicationDbContext _context;
        private AdminFeedbackService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted(); 
            _context.Database.EnsureCreated();

            _service = new AdminFeedbackService(_context);
        }

        [Test]
        public async Task GetAllFeedbacksAsync_ShouldReturnAllFeedbacksSorted()
        {
            // Arrange
            await _context.Users.AddRangeAsync(new List<User>
    {
        new User { Id = "user1", UserName = "User1" },
        new User { Id = "user2", UserName = "User2" },
        new User { Id = "user3", UserName = "User3" }
    });
            await _context.SaveChangesAsync();

            var feedbacks = new List<Feedback>
    {
        new Feedback
        {
            Id = 1,
            Status = "Resolved",
            Message = "Test Message 1",
            Subject = "Test Subject 1",
            Type = "Bug Report",
            UserId = "user1",
            SubmittedAt = new DateTime(2024, 1, 2)
        },
        new Feedback
        {
            Id = 2,
            Status = "Pending",
            Message = "Test Message 2",
            Subject = "Test Subject 2",
            Type = "Feature Request",
            UserId = "user2",
            SubmittedAt = new DateTime(2024, 1, 5)
        },
        new Feedback
        {
            Id = 3,
            Status = "Resolved",
            Message = "Test Message 3",
            Subject = "Test Subject 3",
            Type = "Bug Report",
            UserId = "user3",
            SubmittedAt = new DateTime(2024, 1, 1)
        }
    };

            await _context.Feedbacks.AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllFeedbacksAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.First().Id, Is.EqualTo(2)); // Най-новият feedback трябва да е с ID 2
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldUpdateStatus_WhenFeedbackExists()
        {
            // Arrange
            var feedback = new Feedback
            {
                Id = 1,
                Status = "Pending",
                Message = "Test Message",
                Subject = "Test Subject",
                Type = "Bug Report",
                UserId = "user1"
            };

            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.UpdateStatusAsync(1, "Resolved");
            var updatedFeedback = await _context.Feedbacks.FindAsync(1);

            // Assert
            Assert.IsTrue(result);
            Assert.That(updatedFeedback.Status, Is.EqualTo("Resolved"));
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldReturnFalse_WhenFeedbackDoesNotExist()
        {
            // Act
            var result = await _service.UpdateStatusAsync(999, "Resolved");

            // Assert
            Assert.IsFalse(result);
        }
        [Test]
        public async Task DeleteFeedbackAsync_ShouldDeleteFeedback_WhenFeedbackExists()
        {
            // Arrange
            var feedback = new Feedback
            {
                Id = 1,
                Status = "Pending",
                Message = "Test Message",
                Subject = "Test Subject",
                Type = "Bug Report",
                UserId = "user1"
            };

            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteFeedbackAsync(1);
            var deletedFeedback = await _context.Feedbacks.FindAsync(1);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(deletedFeedback);
        }

        [Test]
        public async Task DeleteFeedbackAsync_ShouldReturnFalse_WhenFeedbackDoesNotExist()
        {
            // Act
            var result = await _service.DeleteFeedbackAsync(999);

            // Assert
            Assert.IsFalse(result);
        }
        [Test]
public async Task ClearResolvedFeedbacksAsync_ShouldDeleteAllResolvedFeedbacks()
{
            // Arrange
            var feedbacks = new List<Feedback>
{
    new Feedback
    {
        Id = 1,
        Status = "Resolved",
        Message = "Test Message 1",
        Subject = "Test Subject 1",
        Type = "Bug Report",
        UserId = "user1"
    },
    new Feedback
    {
        Id = 2,
        Status = "Pending",
        Message = "Test Message 2",
        Subject = "Test Subject 2",
        Type = "Feature Request",
        UserId = "user2"
    },
    new Feedback
    {
        Id = 3,
        Status = "Resolved",
        Message = "Test Message 3",
        Subject = "Test Subject 3",
        Type = "Bug Report",
        UserId = "user3"
    }
};


            await _context.Feedbacks.AddRangeAsync(feedbacks);
    await _context.SaveChangesAsync();

    // Act
    var result = await _service.ClearResolvedFeedbacksAsync();
    var remaining = await _context.Feedbacks.ToListAsync();

    // Assert
    Assert.IsTrue(result);
    Assert.That(remaining.Count, Is.EqualTo(1));
    Assert.That(remaining[0].Status, Is.EqualTo("Pending"));
}

[Test]
public async Task ClearResolvedFeedbacksAsync_ShouldReturnFalse_WhenNoResolvedFeedbacksExist()
{
            // Arrange
            var feedback = new Feedback
            {
                Id = 1,
                Status = "Pending",
                Message = "Test Message",
                Subject = "Test Subject",
                Type = "Bug Report",
                UserId = "user1"
            };

            await _context.Feedbacks.AddAsync(feedback);
    await _context.SaveChangesAsync();

    // Act
    var result = await _service.ClearResolvedFeedbacksAsync();

    // Assert
    Assert.IsFalse(result);
}


    }
}
