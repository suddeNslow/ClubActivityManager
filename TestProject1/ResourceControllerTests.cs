using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Controllers;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class ResourceControllerTests
{
    // Helper method to create a new in-memory database context for each test
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Index_ReturnsViewWithListOfResources()
    {
        // Arrange
        var context = GetInMemoryDbContext(nameof(Index_ReturnsViewWithListOfResources));
        // Seed some resources
        context.Resources.AddRange(new List<Resource>
        {
            new Resource { ResourceId = 1, Name = "Projector", Type = "Equipment", Availability = "Available" },
            new Resource { ResourceId = 2, Name = "Whiteboard", Type = "Tool", Availability = "Available" }
        });
        await context.SaveChangesAsync();

        var controller = new ResourceController(context);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Resource>>(viewResult.Model);
        Assert.Equal(2, model.Count); // We expect two resources
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Arrange
        var context = GetInMemoryDbContext(nameof(Create_Get_ReturnsView));
        var controller = new ResourceController(context);

        // Act
        var result = controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result); // Should return the Create view without a model
    }
}