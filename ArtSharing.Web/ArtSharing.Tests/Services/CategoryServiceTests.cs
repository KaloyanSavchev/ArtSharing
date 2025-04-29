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
    public class CategoryServiceTests
    {
        private ApplicationDbContext _context;
        private CategoryService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CategoryTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _service = new CategoryService(_context);
        }

        [Test]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Art", Description = "Art desc" },
                new Category { Name = "Photography", Description = "Photo desc" }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Exists(c => c.Name == "Art"));
            Assert.That(result.Exists(c => c.Name == "Photography"));
        }
    }
}
