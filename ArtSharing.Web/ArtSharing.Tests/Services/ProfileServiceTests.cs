using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IWebHostEnvironment> _environmentMock;
        private ProfileService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ProfileServiceTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            _service = new ProfileService(_context, _userManagerMock.Object, _environmentMock.Object);
        }

        [Test]
        public async Task ToggleFollowAsync_ShouldUnfollow_WhenAlreadyFollowing()
        {
            // Arrange
            var currentUser = new User { Id = "user1", UserName = "User1" };
            var targetUser = new User { Id = "user2", UserName = "User2" };

            await _context.Users.AddRangeAsync(currentUser, targetUser);
            await _context.UserFollows.AddAsync(new UserFollow
            {
                FollowerId = currentUser.Id,
                FollowingId = targetUser.Id
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleFollowAsync(currentUser, targetUser);

            // Assert
            Assert.IsFalse(result);
            Assert.That(await _context.UserFollows.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task ToggleFollowAsync_ShouldFollow_WhenNotAlreadyFollowing()
        {
            // Arrange
            var currentUser = new User { Id = "user3", UserName = "User3" };
            var targetUser = new User { Id = "user4", UserName = "User4" };

            await _context.Users.AddRangeAsync(currentUser, targetUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleFollowAsync(currentUser, targetUser);

            // Assert
            Assert.IsTrue(result);
            Assert.That(await _context.UserFollows.CountAsync(), Is.EqualTo(1));
            var follow = await _context.UserFollows.FirstOrDefaultAsync();
            Assert.That(follow.FollowerId, Is.EqualTo("user3"));
            Assert.That(follow.FollowingId, Is.EqualTo("user4"));
        }

        [Test]
        public async Task GetFollowListAsync_ShouldReturnFollowers_WhenTypeIsFollowers()
        {
            var user = new User { Id = "userA", UserName = "UserA" };
            var follower1 = new User { Id = "f1", UserName = "Follower1" };
            var follower2 = new User { Id = "f2", UserName = "Follower2" };

            await _context.Users.AddRangeAsync(user, follower1, follower2);
            await _context.UserFollows.AddRangeAsync(
                new UserFollow { FollowerId = follower1.Id, FollowingId = user.Id },
                new UserFollow { FollowerId = follower2.Id, FollowingId = user.Id }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetFollowListAsync(user, "followers");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(u => u.Id == "f1"), Is.True);
            Assert.That(result.Any(u => u.Id == "f2"), Is.True);
        }

        [Test]
        public async Task GetFollowListAsync_ShouldReturnFollowing_WhenTypeIsFollowing()
        {
            var user = new User { Id = "userB", UserName = "UserB" };
            var following1 = new User { Id = "x1", UserName = "X1" };
            var following2 = new User { Id = "x2", UserName = "X2" };

            await _context.Users.AddRangeAsync(user, following1, following2);
            await _context.UserFollows.AddRangeAsync(
                new UserFollow { FollowerId = user.Id, FollowingId = following1.Id },
                new UserFollow { FollowerId = user.Id, FollowingId = following2.Id }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetFollowListAsync(user, "following");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(u => u.Id == "x1"), Is.True);
            Assert.That(result.Any(u => u.Id == "x2"), Is.True);
        }

        [Test]
        public async Task GetUserPostsAsync_ShouldReturnPosts_WhenUserIsOwner()
        {
            var user = new User { Id = "userX", UserName = "userX" };
            var category = new Category { Id = 1, Name = "TestCat" };
            var post = new Post
            {
                Id = 1,
                Title = "My Post",
                Description = "Description for test",
                UserId = user.Id,
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.Categories.AddAsync(category);
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserPostsAsync("userX", "userX");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Title, Is.EqualTo("My Post"));
        }

        [Test]
        public async Task GetUserPostsAsync_ShouldReturnPosts_WhenFollowing()
        {
            var currentUser = new User { Id = "me1", UserName = "me1" };
            var otherUser = new User { Id = "target1", UserName = "target1" };
            var category = new Category { Id = 2, Name = "Cat" };

            await _context.Users.AddRangeAsync(currentUser, otherUser);
            await _context.Categories.AddAsync(category);
            await _context.Posts.AddAsync(new Post
            {
                Title = "Their Post",
                Description = "Description for test",
                UserId = otherUser.Id,
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow
            });

            await _context.UserFollows.AddAsync(new UserFollow
            {
                FollowerId = currentUser.Id,
                FollowingId = otherUser.Id
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetUserPostsAsync("target1", "me1");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Title, Is.EqualTo("Their Post"));
        }

        [Test]
        public async Task GetUserPostsAsync_ShouldReturnEmpty_WhenNotFollowing()
        {
            var currentUser = new User { Id = "userN", UserName = "userN" };
            var targetUser = new User { Id = "userT", UserName = "userT" };
            var category = new Category { Id = 3, Name = "BlockedCat" };

            await _context.Users.AddRangeAsync(currentUser, targetUser);
            await _context.Categories.AddAsync(category);
            await _context.Posts.AddAsync(new Post
            {
                Title = "Hidden Post",
                Description = "This is a test post",
                UserId = targetUser.Id,
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetUserPostsAsync("userT", "userN");

            Assert.That(result.Count, Is.EqualTo(0));
        }


        [Test]
        public async Task GetOwnProfileAsync_ShouldReturnCompleteProfileViewModel()
        {
            var user = new User
            {
                Id = "u101",
                UserName = "TestUser",
                Email = "test@example.com",
                PhoneNumber = "0888123456",
                ProfilePicture = "/images/profiles/pic.png",
                ProfileDescription = "Hello there!"
            };

            var category = new Category { Id = 1, Name = "TestCat" };

            var post = new Post
            {
                Id = 1,
                Title = "MyPost",
                Description = "Post description",
                UserId = user.Id,
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow,
                PostImages = new List<PostImage>
        {
            new PostImage { ImagePath = "/uploads/img1.png" }
        }
            };

            var likedPost = new Post
            {
                Id = 2,
                Title = "LikedPost",
                Description = "Another post",
                UserId = "u999",
                User = new User { Id = "u999", UserName = "OtherUser" },
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow,
                PostImages = new List<PostImage>
        {
            new PostImage { ImagePath = "/uploads/img2.png" }
        }
            };

            var like = new Like
            {
                UserId = user.Id,
                PostId = likedPost.Id,
                Post = likedPost,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.Users.AddAsync(likedPost.User);
            await _context.Categories.AddAsync(category);
            await _context.Posts.AddRangeAsync(post, likedPost);
            await _context.PostImages.AddRangeAsync(post.PostImages.First(), likedPost.PostImages.First());
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();

            var result = await _service.GetOwnProfileAsync(user, "myposts");

            Assert.IsNotNull(result);
            Assert.That(result.UserName, Is.EqualTo("TestUser"));
            Assert.That(result.Email, Is.EqualTo("test@example.com"));
            Assert.That(result.PhoneNumber, Is.EqualTo("0888123456"));
            Assert.That(result.ProfilePicture, Is.EqualTo("/images/profiles/pic.png"));
            Assert.That(result.ProfileDescription, Is.EqualTo("Hello there!"));
            Assert.That(result.FollowersCount, Is.EqualTo(0));
            Assert.That(result.FollowingCount, Is.EqualTo(0));
            Assert.That(result.SelectedTab, Is.EqualTo("myposts"));
            Assert.IsTrue(result.IsOwnProfile);
            Assert.That(result.UserPosts.Count, Is.EqualTo(1));
            Assert.That(result.LikedPosts.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserProfileAsync_ShouldReturnOwnProfile_WhenViewingSelf()
        {
            var user = new User
            {
                Id = "u301",
                UserName = "SelfUser",
                Email = "self@example.com",
                PhoneNumber = "123456",
                ProfilePicture = "/images/profiles/self.png",
                ProfileDescription = "My description",
                IsBanned = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserProfileAsync("SelfUser", user);

            Assert.IsNotNull(result);
            Assert.That(result.UserName, Is.EqualTo("SelfUser"));
            Assert.That(result.IsOwnProfile, Is.True);
            Assert.That(result.IsFollowing, Is.False);
            Assert.That(result.IsBanned, Is.False);
        }

        [Test]
        public async Task GetUserProfileAsync_ShouldReturnProfile_WhenFollowingOtherUser()
        {
            var currentUser = new User { Id = "viewer1", UserName = "Viewer" };
            var targetUser = new User
            {
                Id = "creator1",
                UserName = "Artist",
                Email = "artist@example.com",
                ProfileDescription = "Art profile"
            };

            var category = new Category { Id = 55, Name = "ArtCat" };

            var post = new Post
            {
                Title = "Art Post",
                Description = "Art Desc",
                UserId = targetUser.Id,
                CategoryId = category.Id,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddRangeAsync(currentUser, targetUser);
            await _context.Categories.AddAsync(category);
            await _context.Posts.AddAsync(post);
            await _context.UserFollows.AddAsync(new UserFollow
            {
                FollowerId = currentUser.Id,
                FollowingId = targetUser.Id
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetUserProfileAsync("Artist", currentUser);

            Assert.IsNotNull(result);
            Assert.That(result.UserName, Is.EqualTo("Artist"));
            Assert.That(result.IsOwnProfile, Is.False);
            Assert.That(result.IsFollowing, Is.True);
            Assert.That(result.UserPosts.Count, Is.EqualTo(1));
        }

    }
}
