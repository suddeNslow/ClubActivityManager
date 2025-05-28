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
            .UseInMemoryDatabase(databaseName: "dbName")
            .Options;

        var context = new AppDbContext(options);

        // Seed with some dummy data
        if (!context.Users.Any())
        {
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
        }

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


    [Fact]
    public async Task Details_ExistingEvent_ReturnsViewWithEventAndRegistrationStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();
        var testUser = new ApplicationUser { Id = "user1", UserName = "testuser", Role = "Member" };

        // Add event registration for test user
        var evt = context.Events.First();
        context.EventRegistrations.Add(new EventRegistration
        {
            EventId = evt.EventId,
            UserId = testUser.Id,
            RegistrationDate = DateTime.UtcNow
        });
        context.SaveChanges();

        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns(testUser.Id);

        var controller = new EventController(context, mockUserManager.Object);

        // Act
        var result = await controller.Details(evt.EventId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Event>(viewResult.Model);
        Assert.Equal(evt.EventId, model.EventId);

        // Check that ViewBag.AlreadyRegistered is true (since user registered)
        Assert.True((bool)controller.ViewBag.AlreadyRegistered);
    }

    [Fact]
    public async Task Register_Post_UserNotRegistered_RegistersAndRedirects()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockUserManager = GetMockUserManager();
        var testUser = new ApplicationUser { Id = "user1", UserName = "testuser", Role = "Member" };

        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(testUser);

        var controller = new EventController(context, mockUserManager.Object);

        var evt = context.Events.First();

        // Act
        var result = await controller.Register(evt.EventId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EventController.Details), redirectResult.ActionName);
        Assert.Equal(evt.EventId, redirectResult.RouteValues["id"]);

        // Check that registration was saved
        var registration = context.EventRegistrations
            .FirstOrDefault(r => r.EventId == evt.EventId && r.UserId == testUser.Id);

        Assert.NotNull(registration);
    }

    [Fact]
    public async Task Delete_Post_UserIsCreator_DeletesEventAndRedirects()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var testUser = new ApplicationUser { Id = "user1", UserName = "testuser", Role = "Member" };

        // Make sure user is creator of the seeded event
        var evt = context.Events.First();
        evt.CreatedBy = testUser.Id;
        context.SaveChanges();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(testUser);

        var controller = new EventController(context, mockUserManager.Object);

        // Act
        var result = await controller.Delete(evt.EventId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EventController.Index), redirectResult.ActionName);

        // Confirm event was deleted
        var deletedEvent = await context.Events.FindAsync(evt.EventId);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task Delete_Post_UserNotCreator_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var testUser = new ApplicationUser { Id = "someotheruser", UserName = "otheruser", Role = "Member" };

        var evt = context.Events.First();
        evt.CreatedBy = "creatorUserId";
        context.SaveChanges();

        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(testUser);

        var controller = new EventController(context, mockUserManager.Object);

        // Act
        var result = await controller.Delete(evt.EventId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

}
