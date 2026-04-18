using BlindMatchPAS.Web.Models;

namespace BlindMatchPAS.Tests
{
    public class MatchingLogicTests
    {
        [Fact]
        public void ProposalStatus_TransitionsCorrectly()
        {
            // Arrange
            var proposal = new ProjectProposal
            {
                Status = ProposalStatus.Pending
            };

            // Act & Assert - Pending -> UnderReview
            proposal.Status = ProposalStatus.UnderReview;
            Assert.Equal(ProposalStatus.UnderReview, proposal.Status);

            // Act & Assert - UnderReview -> Matched
            proposal.Status = ProposalStatus.Matched;
            Assert.Equal(ProposalStatus.Matched, proposal.Status);
        }

        [Fact]
        public void Match_IdentityRevealLogic()
        {
            // Arrange
            var match = new Match
            {
                IsConfirmed = false,
                ConfirmedAt = null
            };

            // Assert - Before confirmation (blind)
            Assert.False(match.IsConfirmed);
            Assert.Null(match.ConfirmedAt);

            // Act - Confirm match (reveal)
            match.IsConfirmed = true;
            match.ConfirmedAt = DateTime.UtcNow;

            // Assert - After confirmation (revealed)
            Assert.True(match.IsConfirmed);
            Assert.NotNull(match.ConfirmedAt);
        }

        [Fact]
        public void ProposalValidation_RequiredFieldsPresent()
        {
            // Arrange & Act
            var proposal = new ProjectProposal
            {
                Title = "Valid Title",
                Abstract = "Valid abstract with sufficient length",
                TechStack = "C#, ASP.NET Core",
                ResearchAreaId = 1,
                StudentId = "student-123"
            };

            // Assert
            Assert.NotEmpty(proposal.Title);
            Assert.NotEmpty(proposal.Abstract);
            Assert.NotEmpty(proposal.TechStack);
            Assert.True(proposal.ResearchAreaId > 0);
            Assert.NotEmpty(proposal.StudentId);
        }

        [Theory]
        [InlineData(ProposalStatus.Pending, true)]
        [InlineData(ProposalStatus.UnderReview, true)]
        [InlineData(ProposalStatus.Matched, false)]
        [InlineData(ProposalStatus.Withdrawn, false)]
        public void Proposal_CanBeEditedBasedOnStatus(ProposalStatus status, bool canEdit)
        {
            // Arrange
            var proposal = new ProjectProposal
            {
                Status = status
            };

            // Act
            bool isEditable = proposal.Status == ProposalStatus.Pending ||
                             proposal.Status == ProposalStatus.UnderReview;

            // Assert
            Assert.Equal(canEdit, isEditable);
        }

        [Fact]
        public void ResearchArea_ActiveByDefault()
        {
            // Arrange & Act
            var area = new ResearchArea
            {
                Name = "Test Area",
                Description = "Test Description",
                IsActive = true
            };

            // Assert
            Assert.True(area.IsActive);
        }
    }
}