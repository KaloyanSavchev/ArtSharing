using ArtSharing.Data;
using ArtSharing.Data.Models.Models;
using ArtSharing.Services.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ArtSharing.Tests.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        private ApplicationDbContext _context;
        private ReportService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ReportServiceTests")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new ReportService(_context);
        }

        [Test]
        public async Task ReportPostAsync_ShouldAddReport()
        {
            await _service.ReportPostAsync("user1", 1, "spam");

            var exists = await _context.Reports.AnyAsync(r => r.TargetPostId == 1);
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task ReportUserAsync_ShouldAddReport()
        {
            await _service.ReportUserAsync("user2", "target1", "abuse");

            var exists = await _context.Reports.AnyAsync(r => r.TargetUserId == "target1");
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task ReportCommentAsync_ShouldAddReport()
        {
            await _service.ReportCommentAsync("user3", 3, "offensive");

            var exists = await _context.Reports.AnyAsync(r => r.TargetCommentId == 3);
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task HasAlreadyReportedPostAsync_ShouldReturnTrue_WhenReportExists()
        {
            await _context.Reports.AddAsync(new Report
            {
                ReporterId = "user4",
                TargetPostId = 4,
                TargetType = "Post",
                Reason = "test"
            });
            await _context.SaveChangesAsync();

            var result = await _service.HasAlreadyReportedPostAsync("user4", 4);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task HasAlreadyReportedCommentAsync_ShouldReturnTrue_WhenReportExists()
        {
            await _context.Reports.AddAsync(new Report
            {
                ReporterId = "user5",
                TargetCommentId = 5,
                TargetType = "Comment",
                Reason = "test"
            });
            await _context.SaveChangesAsync();

            var result = await _service.HasAlreadyReportedCommentAsync("user5", 5);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task HasAlreadyReportedUserAsync_ShouldReturnTrue_WhenReportExists()
        {
            await _context.Reports.AddAsync(new Report
            {
                ReporterId = "user6",
                TargetUserId = "target6",
                TargetType = "User",
                Reason = "test"
            });
            await _context.SaveChangesAsync();

            var result = await _service.HasAlreadyReportedUserAsync("user6", "target6");
            Assert.IsTrue(result);
        }
    }
}
