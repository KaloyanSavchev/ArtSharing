using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using ArtSharing.Tests.Helpers;
using Moq;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class AdminServiceTests
    {
        private ApplicationDbContext _context;
        private AdminService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "AdminTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new AdminService(_context, userManager: null);
        }

        [Test]
        public async Task CreateCategoryAsync_ShouldAddCategoryToDatabase()
        {
            // Act
            await _service.CreateCategoryAsync("Test Category", "Test Description");

            // Assert
            var categories = await _context.Categories.ToListAsync();
            Assert.That(categories.Count, Is.EqualTo(1));
            Assert.That(categories[0].Name, Is.EqualTo("Test Category"));
        }

        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var category1 = new Category { Name = "Cat1", Description = "Desc1" };
            var category2 = new Category { Name = "Cat2", Description = "Desc2" };

            await _context.Categories.AddRangeAsync(category1, category2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Exists(c => c.Name == "Cat1"));
            Assert.That(result.Exists(c => c.Name == "Cat2"));
        }
        [Test]
        public async Task GetCategoryByIdAsync_ShouldReturnCorrectCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test", Description = "Desc" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCategoryByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo("Test"));
        }

        [Test]
        public async Task EditCategoryAsync_ShouldUpdateCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Old", Description = "OldDesc" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var updated = new Category { Id = 1, Name = "New", Description = "NewDesc" };

            // Act
            await _service.EditCategoryAsync(updated);

            // Assert
            var result = await _context.Categories.FindAsync(1);
            Assert.That(result.Name, Is.EqualTo("New"));
            Assert.That(result.Description, Is.EqualTo("NewDesc"));
        }

        [Test]
        public async Task DeleteCategoryAsync_ShouldRemoveCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "ToDelete", Description = "DeleteMe" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteCategoryAsync(1);

            // Assert
            var result = await _context.Categories.FindAsync(1);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllReportsAsync_ShouldReturnReportsWithIncludes()
        {
            // Arrange
            var reporter = new User { Id = "user1", UserName = "Reporter" };
            var targetUser = new User { Id = "target", UserName = "Target" };

            await _context.Users.AddRangeAsync(reporter, targetUser);
            await _context.SaveChangesAsync();

            var report = new Report
            {
                Id = 1,
                Reporter = reporter,
                ReporterId = reporter.Id,
                TargetUser = targetUser,
                TargetUserId = targetUser.Id,
                Reason = "Test reason",
                TargetType = "User"
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User>());

            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            var result = await service.GetAllReportsAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Reason, Is.EqualTo("Test reason"));
            Assert.That(result[0].Reporter.UserName, Is.EqualTo("Reporter"));
            Assert.That(result[0].TargetUser.UserName, Is.EqualTo("Target"));
        }


        [Test]
        public async Task DismissReportAsync_ShouldDeleteReport()
        {
            // Arrange
            var reporter = new User { Id = "reporter1", UserName = "Reporter" };
            await _context.Users.AddAsync(reporter);
            await _context.SaveChangesAsync();

            var report = new Report
            {
                Id = 1,
                ReporterId = "reporter1",
                Reason = "Test reason",
                TargetType = "Post"
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User>());

            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            await service.DismissReportAsync(1);

            // Assert
            var result = await _context.Reports.FindAsync(1);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllUsersWithRolesAsync_ShouldReturnUsersWithCorrectRoles()
        {
            // Arrange
            var users = new List<User>
            {
        new User { Id = "1", UserName = "user1", Email = "user1@email.com", IsBanned = false },
        new User { Id = "2", UserName = "mod", Email = "mod@email.com", IsBanned = true }
            };

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(users);
            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            var result = await service.GetAllUsersWithRolesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(u => u.UserName == "mod" && u.Roles.Contains("Moderator")));
        }

        [Test]
        public async Task PromoteToModeratorAsync_ShouldAddRole_WhenUserIsNotModerator()
        {
            // Arrange
            var user = new User { Id = "1", UserName = "newUser" };
            var users = new List<User> { user };

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(users);
            mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            await service.PromoteToModeratorAsync("1");

            // Assert
            mockUserManager.Verify(x => x.AddToRoleAsync(user, "Moderator"), Times.Once);
        }

        [Test]
        public async Task RemoveModeratorRoleAsync_ShouldRemoveRole_WhenUserIsModerator()
        {
            // Arrange
            var user = new User { Id = "2", UserName = "mod" };
            var users = new List<User> { user };

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(users);
            mockUserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(true);

            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            await service.RemoveModeratorRoleAsync("2");

            // Assert
            mockUserManager.Verify(x => x.RemoveFromRoleAsync(user, "Moderator"), Times.Once);
        }

        [Test]
        public async Task BanUserAsync_ShouldSetIsBannedToTrue_AndRemoveReports()
        {
            // Arrange
            var user = new User { Id = "user1", UserName = "target", IsBanned = false };
            var reporter = new User { Id = "reporter1", UserName = "Reporter" };

            await _context.Users.AddRangeAsync(user, reporter);
            await _context.SaveChangesAsync();

            var report1 = new Report
            {
                Id = 1,
                ReporterId = "reporter1",
                Reason = "Spam",
                TargetUserId = "user1",
                TargetType = "User"
            };

            var report2 = new Report
            {
                Id = 2,
                ReporterId = "reporter1",
                Reason = "Offensive",
                TargetUserId = "user1",
                TargetType = "User"
            };

            await _context.Reports.AddRangeAsync(report1, report2);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { user });

            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            await service.BanUserAsync("user1");

            // Assert
            Assert.IsTrue(user.IsBanned);
            Assert.That(await _context.Reports.CountAsync(), Is.EqualTo(0));
            mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        }


        [Test]
        public async Task UnbanUserAsync_ShouldSetIsBannedToFalse()
        {
            // Arrange
            var user = new User { Id = "3", UserName = "bannedUser", IsBanned = true };
            var users = new List<User> { user };

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(users);
            var service = new AdminService(_context, mockUserManager.Object);

            // Act
            await service.UnbanUserAsync("3");

            // Assert
            Assert.IsFalse(user.IsBanned);
            mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        }



    }
}
