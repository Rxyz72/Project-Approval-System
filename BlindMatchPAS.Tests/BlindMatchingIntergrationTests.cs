using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Tests
{
    public class BlindMatchingIntegrationTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task BlindMatching_ProposalHiddenUntilConfirmed()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var student = new ApplicationUser
            {
                Id = "student-1",
                UserName = "student@test.com",
                Email = "student@test.com",
                FullName = "Test Student"
            };

            var supervisor = new ApplicationUser
            {
                Id = "supervisor-1",
                UserName = "supervisor@test.com",
                Email = "supervisor@test.com",
                FullName = "Test Supervisor"
            };

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "AI",
                Description = "Artificial Intelligence",
                IsActive = true
            };

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "AI Research Project",
                Abstract = "Testing blind matching",
                TechStack = "Python, TensorFlow",
                ResearchAreaId = 1,
                StudentId = student.Id,
                Status = ProposalStatus.Pending
            };

            context.ResearchAreas.Add(researchArea);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Act - Supervisor expresses interest (blind)
            var match = new Match
            {
                ProjectProposalId = proposal.Id,
                SupervisorId = supervisor.Id,
                IsConfirmed = false
            };
            context.Matches.Add(match);
            proposal.Status = ProposalStatus.UnderReview;
            await context.SaveChangesAsync();

            // Assert - Match exists but not confirmed (still blind)
            var savedMatch = await context.Matches.FirstOrDefaultAsync(m => m.Id == match.Id);
            Assert.NotNull(savedMatch);
            Assert.False(savedMatch.IsConfirmed);
            Assert.Null(savedMatch.ConfirmedAt);

            // Act - Confirm match (identity reveal)
            savedMatch.IsConfirmed = true;
            savedMatch.ConfirmedAt = DateTime.UtcNow;
            proposal.Status = ProposalStatus.Matched;
            await context.SaveChangesAsync();

            // Assert - Match confirmed, identities revealed
            var confirmedMatch = await context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == match.Id);

            Assert.NotNull(confirmedMatch);
            Assert.True(confirmedMatch.IsConfirmed);
            Assert.NotNull(confirmedMatch.ConfirmedAt);
            Assert.Equal(ProposalStatus.Matched, confirmedMatch.ProjectProposal!.Status);
        }

        [Fact]
        public async Task Database_CanSaveAndRetrieveProposal()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var researchArea = new ResearchArea
            {
                Id = 1,
                Name = "Web Development",
                Description = "Full-stack development",
                IsActive = true
            };
            context.ResearchAreas.Add(researchArea);

            var proposal = new ProjectProposal
            {
                Title = "E-Commerce Platform",
                Abstract = "Building a scalable e-commerce solution",
                TechStack = "React, Node.js, MongoDB",
                ResearchAreaId = 1,
                StudentId = "test-student",
                Status = ProposalStatus.Pending
            };

            // Act
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Assert
            var savedProposal = await context.ProjectProposals
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.Title == "E-Commerce Platform");

            Assert.NotNull(savedProposal);
            Assert.Equal("E-Commerce Platform", savedProposal.Title);
            Assert.Equal("Web Development", savedProposal.ResearchArea!.Name);
            Assert.Equal(ProposalStatus.Pending, savedProposal.Status);
        }

        [Fact]
        public async Task Match_CascadeDeleteWhenProposalDeleted()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var proposal = new ProjectProposal
            {
                Id = 1,
                Title = "Test",
                Abstract = "Test",
                TechStack = "Test",
                ResearchAreaId = 1,
                StudentId = "student-1",
                Status = ProposalStatus.UnderReview
            };

            var match = new Match
            {
                Id = 1,
                ProjectProposalId = 1,
                SupervisorId = "supervisor-1",
                IsConfirmed = false
            };

            context.ProjectProposals.Add(proposal);
            context.Matches.Add(match);
            await context.SaveChangesAsync();

            // Act - Delete proposal
            context.ProjectProposals.Remove(proposal);
            await context.SaveChangesAsync();

            // Assert - Match should also be deleted (cascade)
            var matchExists = await context.Matches.AnyAsync(m => m.Id == 1);
            Assert.False(matchExists);
        }

        [Fact]
        public async Task ResearchArea_CanBeDeactivated()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            var area = new ResearchArea
            {
                Id = 1,
                Name = "Blockchain",
                Description = "Distributed ledger technology",
                IsActive = true
            };

            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            // Act
            area.IsActive = false;
            await context.SaveChangesAsync();

            // Assert
            var updatedArea = await context.ResearchAreas.FindAsync(1);
            Assert.NotNull(updatedArea);
            Assert.False(updatedArea.IsActive);
        }
    }
}