using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using ArtSharing.Tests.Helpers;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class CommentServiceTests
    {
        private ApplicationDbContext _context;
        private CommentService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CommentTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new CommentService(_context, null);
        }

        [Test]
        public async Task CreateCommentAsync_ShouldAddComment()
        {
            // Act
            await _service.CreateCommentAsync(1, "Test comment", "user1", null);

            // Assert
            var comments = await _context.Comments.ToListAsync();
            Assert.That(comments.Count, Is.EqualTo(1));
            Assert.That(comments[0].Content, Is.EqualTo("Test comment"));
        }

        [Test]
        public async Task GetCommentByIdAsync_ShouldReturnCommentWithIncludes()
        {
            // Arrange
            var user = new User { Id = "user1", UserName = "User1" };
            var post = new Post
            {
                Id = 1,
                Title = "Test Post",
                Description = "Test description",
                UserId = "user1"
            };
            var comment = new Comment
            {
                Id = 1,
                Content = "Test Comment",
                UserId = user.Id,
                PostId = post.Id,
                User = user,
                Post = post
            };

            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(post);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCommentByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Content, Is.EqualTo("Test Comment"));
            Assert.That(result.User.UserName, Is.EqualTo("User1"));
            Assert.That(result.Post.Title, Is.EqualTo("Test Post"));
        }

        [Test]
        public async Task UpdateCommentAsync_ShouldUpdate_WhenUserIsOwner()
        {
            // Arrange
            var user = new User { Id = "user1", UserName = "User1" };
            var comment = new Comment { Id = 1, Content = "Old Content", UserId = "user1" };

            await _context.Users.AddAsync(user);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { user });

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.UpdateCommentAsync(1, "New Content", "user1");

            // Assert
            Assert.IsTrue(result);
            var updatedComment = await _context.Comments.FindAsync(1);
            Assert.That(updatedComment.Content, Is.EqualTo("New Content"));
        }

        [Test]
        public async Task UpdateCommentAsync_ShouldUpdate_WhenUserIsAdmin()
        {
            // Arrange
            var owner = new User { Id = "user1", UserName = "Owner" };
            var admin = new User { Id = "admin1", UserName = "AdminUser" };

            await _context.Users.AddRangeAsync(owner, admin);
            await _context.SaveChangesAsync();

            var comment = new Comment { Id = 1, Content = "Old Content", UserId = "user1" };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { owner, admin });
            mockUserManager.Setup(x => x.IsInRoleAsync(admin, "Admin")).ReturnsAsync(true);

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.UpdateCommentAsync(1, "Admin Edit", "admin1");

            // Assert
            Assert.IsTrue(result);
            var updatedComment = await _context.Comments.FindAsync(1);
            Assert.That(updatedComment.Content, Is.EqualTo("Admin Edit"));
        }

        [Test]
        public async Task UpdateCommentAsync_ShouldNotUpdate_WhenUserIsNotOwnerNorAdminNorModerator()
        {
            // Arrange
            var owner = new User { Id = "user1", UserName = "Owner" };
            var stranger = new User { Id = "user2", UserName = "Stranger" };

            await _context.Users.AddRangeAsync(owner, stranger);
            await _context.SaveChangesAsync();

            var comment = new Comment { Id = 1, Content = "Original Content", UserId = "user1" };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { owner, stranger });
            mockUserManager.Setup(x => x.IsInRoleAsync(stranger, "Admin")).ReturnsAsync(false);
            mockUserManager.Setup(x => x.IsInRoleAsync(stranger, "Moderator")).ReturnsAsync(false);

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.UpdateCommentAsync(1, "Stranger Edit", "user2");

            // Assert
            Assert.IsFalse(result);
            var updatedComment = await _context.Comments.FindAsync(1);
            Assert.That(updatedComment.Content, Is.EqualTo("Original Content"));
        }

        [Test]
        public async Task DeleteCommentAsync_ShouldDelete_WhenUserIsOwner()
        {
            // Arrange
            var user = new User { Id = "user1", UserName = "User1" };
            var comment = new Comment { Id = 1, Content = "Comment to delete", UserId = "user1" };

            await _context.Users.AddAsync(user);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { user });

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.DeleteCommentAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);
            var deletedComment = await _context.Comments.FindAsync(1);
            Assert.IsNull(deletedComment);
        }


        [Test]
        public async Task DeleteCommentAsync_ShouldDelete_WhenUserIsAdmin()
        {
            // Arrange
            var owner = new User { Id = "user1", UserName = "Owner" };
            var admin = new User { Id = "admin1", UserName = "AdminUser" };

            await _context.Users.AddRangeAsync(owner, admin);
            await _context.SaveChangesAsync();

            var comment = new Comment { Id = 1, Content = "Admin delete", UserId = "user1" };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { owner, admin });
            mockUserManager.Setup(x => x.IsInRoleAsync(admin, "Admin")).ReturnsAsync(true);

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.DeleteCommentAsync(1, "admin1");

            // Assert
            Assert.IsTrue(result);
            var deletedComment = await _context.Comments.FindAsync(1);
            Assert.IsNull(deletedComment);
        }

        [Test]
        public async Task DeleteCommentAsync_ShouldNotDelete_WhenUserIsNotOwnerNorAdminNorModerator()
        {
            // Arrange
            var owner = new User { Id = "user1", UserName = "Owner" };
            var stranger = new User { Id = "user2", UserName = "Stranger" };

            await _context.Users.AddRangeAsync(owner, stranger);
            await _context.SaveChangesAsync();

            var comment = new Comment { Id = 1, Content = "Can't touch this", UserId = "user1" };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var mockUserManager = MockUserManagerHelper.CreateMockUserManager(new List<User> { owner, stranger });
            mockUserManager.Setup(x => x.IsInRoleAsync(stranger, "Admin")).ReturnsAsync(false);
            mockUserManager.Setup(x => x.IsInRoleAsync(stranger, "Moderator")).ReturnsAsync(false);

            var service = new CommentService(_context, mockUserManager.Object);

            // Act
            var result = await service.DeleteCommentAsync(1, "user2");

            // Assert
            Assert.IsFalse(result);
            var existingComment = await _context.Comments.FindAsync(1);
            Assert.IsNotNull(existingComment);
        }

    }
}
