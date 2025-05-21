using Xunit;
using Moq;
using ClubActivityManager.Controllers;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class EventControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var context = new AppDbContext(options);

        // Seed with some dummy data
        var user = new ApplicationUser { Id = "user1", UserName = "testuser", Role = "Member" };
        context.Users.Add(user);

        var evt = new Event
        {
            EventId = 1,
            Title = "Test Event",
            Description = "A sample test event.", 
            Location = "Room A",                  
            DateTime = DateTime.Now.AddDays(1),
            CreatedBy = user.Id,
            Creator = user
        };

        context.Events.Add(evt);
        context.SaveChanges();

        return context;
    }

    private Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Index_ReturnsViewWithEvents()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();

        var controller = new EventController(context, mockUserManager.Object);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Event>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("Test Event", model.First().Title);
    }
}
