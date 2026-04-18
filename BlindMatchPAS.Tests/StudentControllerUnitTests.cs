using BlindMatchPAS.Web.Controllers;
using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace BlindMatchPAS.Tests
{
    public class StudentControllerUnitTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed test data
            context.ResearchAreas.Add(new ResearchArea
            {
                Id = 1,
                Name = "AI",
                Description = "Artificial Intelligence",
                IsActive = true
            });
            context.SaveChanges();

            return context;
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Dashboard_ReturnsViewWithProposals()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManagerMock = GetMockUserManager();

            var userId = "test-user-id";
            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Test Proposal",
                Abstract = "Test Abstract",
                TechStack = "C#, ASP.NET",
                ResearchAreaId = 1,
                StudentId = userId,
                Status = ProposalStatus.Pending
            };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var controller = new StudentController(context, userManagerMock.Object);

            // Act
            var result = await controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudentDashboardViewModel>(viewResult.Model);
            Assert.Single(model.Proposals);
            Assert.Equal("Test Proposal", model.Proposals[0].Title);
        }

        [Fact]
        public async Task CreateProposal_ValidModel_RedirectsToDashboard()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManagerMock = GetMockUserManager();

            var userId = "test-user-id";
            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            var controller = new StudentController(context, userManagerMock.Object);

            // Mock TempData
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            var model = new CreateProposalViewModel
            {
                Title = "New Proposal",
                Abstract = "This is a test abstract for the proposal",
                TechStack = "C#, .NET",
                ResearchAreaId = 1
            };

            // Act
            var result = await controller.CreateProposal(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Single(context.ProjectProposals);
        }

        [Fact]
        public async Task WithdrawProposal_MatchedProposal_ReturnsError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManagerMock = GetMockUserManager();

            var userId = "test-user-id";
            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Matched Proposal",
                Abstract = "Test",
                TechStack = "Test",
                ResearchAreaId = 1,
                StudentId = userId,
                Status = ProposalStatus.Matched
            };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var controller = new StudentController(context, userManagerMock.Object);

            // Mock TempData
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Act
            var result = await controller.WithdrawProposal(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteProposal_PendingProposal_DeletesSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManagerMock = GetMockUserManager();

            var userId = "test-user-id";
            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Pending Proposal",
                Abstract = "Test",
                TechStack = "Test",
                ResearchAreaId = 1,
                StudentId = userId,
                Status = ProposalStatus.Pending
            };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var controller = new StudentController(context, userManagerMock.Object);

            // Mock TempData
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Act
            var result = await controller.DeleteProposal(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Empty(context.ProjectProposals);
        }
    }
}