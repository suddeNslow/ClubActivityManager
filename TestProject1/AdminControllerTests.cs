using Xunit;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Controllers;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public class AdminControllerTests
{
    // Helper method to create an in-memory DbContext
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
            .Options;

        return new AppDbContext(options);
    }

    private UserManager<ApplicationUser> GetMockUserManager(List<ApplicationUser> users)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

        userManager.Setup(x => x.Users).Returns(users.AsQueryable());

        return userManager.Object;
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

    [Fact]
    public async Task Resources_ReturnsListOfResources()
    {
        // Arrange: Setup in-memory DB and seed a resource
        var context = GetInMemoryDbContext();
        context.Resources.Add(new Resource { Name = "Speaker", Type = "Equipment", Availability = "available" });
        await context.SaveChangesAsync();

        var controller = new AdminController(context, null);

        // Act: Call the Resources() action
        var result = await controller.Resources();

        // Assert: Verify it returns a view with the correct model
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Resource>>(view.Model);
        Assert.Single(model); // Only one item was seeded
    }

    [Fact]
    public async Task DeleteResource_ValidId_RemovesResource()
    {
        // Arrange: Create DB with one resource
        var context = GetInMemoryDbContext();
        var resource = new Resource { Name = "Microphone", Type = "Equipment", Availability = "available" };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, null);

        // Act: Attempt to delete that resource
        var result = await controller.DeleteResource(resource.ResourceId);

        // Assert: Resource should be deleted and redirected
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Resources", redirect.ActionName);

        var exists = await context.Resources.FindAsync(resource.ResourceId);
        Assert.Null(exists); // Ensure it's really gone
    }

    [Fact]
    public async Task EditResource_ValidModel_UpdatesResource()
    {
        // Arrange: Add a resource to edit
        var context = GetInMemoryDbContext();
        var resource = new Resource { Name = "OldName", Type = "Tool", Availability = "available" };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, null);

        // Act: Edit it and update the name
        resource.Name = "NewName";
        var result = await controller.EditResource(resource);

        // Assert: Should redirect and update name
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Resources", redirect.ActionName);

        var updated = await context.Resources.FindAsync(resource.ResourceId);
        Assert.Equal("NewName", updated.Name);
    }

    [Fact]
    public async Task EditResource_InvalidModel_ReturnsView()
    {
        // Arrange: Add an original resource
        var context = GetInMemoryDbContext();
        var resource = new Resource { Name = "Whiteboard", Type = "Tool", Availability = "available" };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, null);
        controller.ModelState.AddModelError("Name", "Required");

        // Act: Try to update with an invalid model
        var result = await controller.EditResource(resource);

        // Assert: Should return to View with the same model
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Resource>(view.Model);
        Assert.Equal("Whiteboard", model.Name);
    }

    [Fact]
    public async Task Payments_ReturnsListOfPaymentsWithMembers()
    {
        // Arrange: Create in-memory DB and seed a user and a payment
        var context = GetInMemoryDbContext();

        var user = new ApplicationUser
        {
            Id = "user1",
            UserName = "member1@example.com",
            Role = "Member"  
        };

        var payment = new Payment
        {
            MemberId = "user1",
            Amount = 100,
            PaymentDate = DateTime.UtcNow,
            Method = "card"
        };
        context.Users.Add(user);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var controller = new AdminController(context, null); // UserManager not needed here

        // Act: Call the Payments action
        var result = await controller.Payments();

        // Assert: Verify the payment and associated member are returned
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Payment>>(view.Model);
        Assert.Single(model);
        Assert.Equal("user1", model[0].MemberId);
    }

    [Fact]
    public async Task DeleteMember_ValidId_DeletesUser()
    {
        // Arrange: Setup a user and mock UserManager
        var user = new ApplicationUser { Id = "123", UserName = "testuser@club.com" };
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

        userManagerMock.Setup(um => um.FindByIdAsync("123")).ReturnsAsync(user);
        userManagerMock.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(null, userManagerMock.Object);

        // Act
        var result = await controller.DeleteMember("123");

        // Assert: Should redirect to Members view
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Members", redirect.ActionName);

        // Verify that DeleteAsync was called
        userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
    }
}
