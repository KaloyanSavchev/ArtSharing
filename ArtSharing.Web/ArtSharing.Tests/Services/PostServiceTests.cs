using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.AspNetCore.Hosting;
using ArtSharing.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class PostServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IWebHostEnvironment> _environmentMock;
        private PostService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("PostServiceTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            _service = new PostService(_context, _userManagerMock.Object, _environmentMock.Object);
        }

        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            await _context.Categories.AddRangeAsync(
                new Category { Name = "Photography" },
                new Category { Name = "Painting" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }
        [Test]
        public async Task CreatePostAsync_ShouldSavePostAndImages()
        {
            // Arrange
            var uploadsPath = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath); // създаваме фалшив uploads

            var userId = "u1";
            var model = new PostCreateViewModel
            {
                Title = "Test Post",
                Description = "Test Description",
                CategoryId = 1,
                ImageFiles = new List<IFormFile>()
            };

            var fakeImage = new Mock<IFormFile>();
            var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Fake image content"));
            fakeImage.Setup(f => f.Length).Returns(content.Length);
            fakeImage.Setup(f => f.FileName).Returns("test.png");
            fakeImage.Setup(f => f.OpenReadStream()).Returns(content);
            fakeImage.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                     .Returns<Stream, CancellationToken>((stream, token) => content.CopyToAsync(stream, token));

            model.ImageFiles.Add(fakeImage.Object);

            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            await _context.Categories.AddAsync(new Category { Id = 1, Name = "TestCat" });
            await _context.SaveChangesAsync();

            // Act
            await _service.CreatePostAsync(model, userId);

            // Assert
            var posts = await _context.Posts.Include(p => p.PostImages).ToListAsync();
            Assert.That(posts.Count, Is.EqualTo(1));
            Assert.That(posts[0].Title, Is.EqualTo("Test Post"));
            Assert.That(posts[0].PostImages.Count, Is.EqualTo(1));
        }
        [TearDown]
        public void Cleanup()
        {
            var uploadsPath = Path.Combine("wwwroot", "uploads");
            if (Directory.Exists(uploadsPath))
                Directory.Delete(uploadsPath, true);
        }

        [Test]
        public async Task GetPostForEditAsync_ShouldReturnViewModel_WhenUserIsOwner()
        {
            // Arrange
            var user = new User { Id = "u1", UserName = "owner" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 1,
                Title = "My Post",
                Description = "desc",
                CategoryId = 1,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            // Act
            var result = await _service.GetPostForEditAsync(1, "u1");

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("My Post"));
        }

        [Test]
        public async Task GetPostForEditAsync_ShouldReturnViewModel_WhenUserIsAdmin()
        {
            var user = new User { Id = "admin", UserName = "admin" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 2,
                Title = "Admin Post",
                Description = "desc",
                CategoryId = 1,
                UserId = "someoneelse",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("admin")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var result = await _service.GetPostForEditAsync(2, "admin");

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Admin Post"));
        }

        [Test]
        public async Task GetPostForEditAsync_ShouldReturnNull_WhenUserNotOwnerOrAdmin()
        {
            var user = new User { Id = "u2", UserName = "user" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 3,
                Title = "Not Your Post",
                Description = "desc",
                CategoryId = 1,
                UserId = "someoneelse",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("u2")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _service.GetPostForEditAsync(3, "u2");

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdatePostAsync_ShouldUpdatePost_WhenUserIsOwner()
        {
            var user = new User { Id = "owner1", UserName = "owner" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 10,
                Title = "Original",
                Description = "Original desc",
                CategoryId = 1,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("owner1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var model = new EditPostViewModel
            {
                Id = 10,
                Title = "Updated Title",
                Description = "Updated Description",
                CategoryId = 2
            };

            var result = await _service.UpdatePostAsync(10, model, "owner1");

            Assert.IsTrue(result);
            var updated = await _context.Posts.FindAsync(10);
            Assert.That(updated.Title, Is.EqualTo("Updated Title"));
            Assert.That(updated.Description, Is.EqualTo("Updated Description"));
            Assert.That(updated.CategoryId, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdatePostAsync_ShouldUpdatePost_WhenUserIsAdmin()
        {
            var user = new User { Id = "admin1", UserName = "admin" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 11,
                Title = "Before",
                Description = "desc",
                CategoryId = 1,
                UserId = "otheruser",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("admin1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var model = new EditPostViewModel
            {
                Id = 11,
                Title = "Admin Updated",
                Description = "By admin",
                CategoryId = 3
            };

            var result = await _service.UpdatePostAsync(11, model, "admin1");

            Assert.IsTrue(result);
            var post = await _context.Posts.FindAsync(11);
            Assert.That(post.Title, Is.EqualTo("Admin Updated"));
            Assert.That(post.Description, Is.EqualTo("By admin"));
            Assert.That(post.CategoryId, Is.EqualTo(3));
        }

        [Test]
        public async Task UpdatePostAsync_ShouldReturnFalse_WhenUserIsNotOwnerOrAdmin()
        {
            var user = new User { Id = "user1", UserName = "regular" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 12,
                Title = "No touch",
                Description = "desc",
                CategoryId = 1,
                UserId = "someone_else",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var model = new EditPostViewModel
            {
                Id = 12,
                Title = "Should not update",
                Description = "fail",
                CategoryId = 4
            };

            var result = await _service.UpdatePostAsync(12, model, "user1");

            Assert.IsFalse(result);
            var post = await _context.Posts.FindAsync(12);
            Assert.That(post.Title, Is.EqualTo("No touch")); // no change
        }

        [Test]
        public async Task GetPostForDeleteAsync_ShouldReturnPost_WhenUserIsOwner()
        {
            var user = new User { Id = "ownerDel", UserName = "owner" };
            var category = new Category { Id = 1, Name = "TestCat" };

            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Id = 100,
                Title = "Delete me",
                Description = "desc",
                CategoryId = category.Id,
                Category = category,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("ownerDel")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _service.GetPostForDeleteAsync(100, "ownerDel");

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Delete me"));
        }

        [Test]
        public async Task GetPostForDeleteAsync_ShouldReturnPost_WhenUserIsAdmin()
        {
            var admin = new User { Id = "adminDel", UserName = "admin" };
            var postOwner = new User { Id = "other", UserName = "postowner" };
            var category = new Category { Id = 1, Name = "TestCat" };

            await _context.Users.AddRangeAsync(admin, postOwner);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            await _context.Posts.AddAsync(new Post
            {
                Id = 101,
                Title = "Admin deletes this",
                Description = "desc",
                CategoryId = category.Id,
                Category = category,
                UserId = postOwner.Id,
                User = postOwner,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("adminDel")).ReturnsAsync(admin);
            _userManagerMock.Setup(m => m.IsInRoleAsync(admin, "Admin")).ReturnsAsync(true);

            var result = await _service.GetPostForDeleteAsync(101, "adminDel");

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Admin deletes this"));
        }

        [Test]
        public async Task GetPostForDeleteAsync_ShouldReturnNull_WhenUnauthorizedUser()
        {
            var user = new User { Id = "unauthUser", UserName = "unauth" };
            await _context.Users.AddAsync(user);
            await _context.Posts.AddAsync(new Post
            {
                Id = 102,
                Title = "Protected",
                Description = "desc",
                CategoryId = 1,
                UserId = "someone_else",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("unauthUser")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _service.GetPostForDeleteAsync(102, "unauthUser");

            Assert.IsNull(result);
        }

        [Test]
        public async Task DeletePostConfirmedAsync_ShouldDeletePost_WhenUserIsOwner()
        {
            var user = new User { Id = "ownerDelConfirm", UserName = "owner" };
            var category = new Category { Id = 5, Name = "TestCat" };

            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Id = 201,
                Title = "Delete me",
                Description = "desc",
                CategoryId = category.Id,
                Category = category,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("ownerDelConfirm")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _service.DeletePostConfirmedAsync(201, "ownerDelConfirm");

            Assert.IsTrue(result);
            var deleted = await _context.Posts.FindAsync(201);
            Assert.IsNull(deleted);
        }

        [Test]
        public async Task DeletePostConfirmedAsync_ShouldReturnFalse_WhenUserIsNotAuthorized()
        {
            var user = new User { Id = "intruder", UserName = "hacker" };
            var owner = new User { Id = "realowner", UserName = "owner" };
            var category = new Category { Id = 6, Name = "TestCat2" };

            await _context.Users.AddRangeAsync(user, owner);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Id = 202,
                Title = "Protected Post",
                Description = "desc",
                CategoryId = category.Id,
                Category = category,
                UserId = owner.Id,
                User = owner,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("intruder")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _service.DeletePostConfirmedAsync(202, "intruder");

            Assert.IsFalse(result);
            var existing = await _context.Posts.FindAsync(202);
            Assert.IsNotNull(existing);
        }

        [Test]
        public async Task GetPostDetailsAsync_ShouldReturnPostWithAllIncludes()
        {
            var user = new User { Id = "detailsUser", UserName = "Poster" };
            var commenter = new User { Id = "commenter1", UserName = "Commenter" };
            var replier = new User { Id = "replier1", UserName = "Replier" };
            var category = new Category { Id = 10, Name = "TestCat" };

            await _context.Users.AddRangeAsync(user, commenter, replier);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Id = 301,
                Title = "Full Post",
                Description = "With all includes",
                CategoryId = category.Id,
                Category = category,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow,
                PostImages = new List<PostImage>
        {
            new PostImage { ImagePath = "/uploads/image1.png" }
        },
                Likes = new List<Like>
        {
            new Like { UserId = user.Id, CreatedAt = DateTime.UtcNow }
        },
                Comments = new List<Comment>
        {
            new Comment
            {
                Id = 401,
                Content = "Main Comment",
                UserId = commenter.Id,
                User = commenter,
                CreatedAt = DateTime.UtcNow,
                Replies = new List<Comment>
                {
                    new Comment
                    {
                        Content = "Reply Comment",
                        UserId = replier.Id,
                        User = replier,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            }
        }
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var result = await _service.GetPostDetailsAsync(301);

            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo("Full Post"));
            Assert.That(result.PostImages.Count, Is.EqualTo(1));
            Assert.That(result.Likes.Count, Is.EqualTo(1));
            Assert.That(result.Comments.Count, Is.EqualTo(1));
            Assert.That(result.Comments.First().Replies.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task LoadMoreCommentsAsync_ShouldReturnTopThreeParentComments_WithReplies()
        {
            var user = new User { Id = "cuser", UserName = "Commenter" };
            var replier = new User { Id = "ruser", UserName = "Replier" };
            var category = new Category { Id = 123, Name = "CommentCat" };

            await _context.Users.AddRangeAsync(user, replier);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var post = new Post
            {
                Id = 999,
                Title = "Post with Comments",
                Description = "Test description",
                CategoryId = category.Id,
                UserId = user.Id,
                Category = category,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var comments = new List<Comment>
    {
        new Comment
        {
            Content = "Comment 1",
            PostId = post.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-3),
            UserId = user.Id,
            User = user
        },
        new Comment
        {
            Content = "Comment 2",
            PostId = post.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-2),
            UserId = user.Id,
            User = user,
            Replies = new List<Comment>
            {
                new Comment
                {
                    Content = "Reply 1",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                    UserId = replier.Id,
                    User = replier
                }
            }
        },
        new Comment
        {
            Content = "Comment 3",
            PostId = post.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            UserId = user.Id,
            User = user
        },
        new Comment
        {
            Content = "Comment 4",
            PostId = post.Id,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            User = user
        }
    };

            await _context.Comments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            var result = await _service.LoadMoreCommentsAsync(post.Id, skip: 0);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Any(c => c.Replies.Count > 0), Is.True);
        }

    }
}

