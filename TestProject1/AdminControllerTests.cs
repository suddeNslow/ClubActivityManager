using Xunit;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Controllers;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AdminControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateResource_ValidResource_SavesAndRedirects()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = new AdminController(context, null); // UserManager is not needed here

        var resource = new Resource
        {
            Name = "Projector",
            Type = "Equipment",
            Availability = "available"
        };

        // Act
        var result = await controller.CreateResource(resource);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Resources", redirect.ActionName);

        var saved = await context.Resources.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("Projector", saved.Name);
    }

    // Test for invalid resource
    [Fact]
    public async Task CreateResource_InvalidModel_ReturnsView()
    {
        var context = GetInMemoryDbContext();
        var controller = new AdminController(context, null);
        controller.ModelState.AddModelError("Name", "Required");

        var resource = new Resource
        {
            Name = null,
            Type = "Equipment",
            Availability = "available"
        };

        var result = await controller.CreateResource(resource);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Resource>(view.Model);
        Assert.Null(model.Name);
    }
}
