using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Web.Models
{
    public class ProjectProposal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string TechStack { get; set; } = string.Empty;

        [Required]
        public int ResearchAreaId { get; set; }
        public ResearchArea? ResearchArea { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }

        public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum ProposalStatus
    {
        Pending,
        UnderReview,
        Matched,
        Withdrawn
    }
}