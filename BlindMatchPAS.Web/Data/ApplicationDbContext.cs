using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Web.Models;

namespace BlindMatchPAS.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();
        public DbSet<ProjectProposal> ProjectProposals => Set<ProjectProposal>();
        public DbSet<Match> Matches => Set<Match>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relationships
            builder.Entity<ProjectProposal>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Match>()
                .HasOne(m => m.Supervisor)
                .WithMany()
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Match>()
                .HasOne(m => m.ProjectProposal)
                .WithMany()
                .HasForeignKey(m => m.ProjectProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Research Areas
            builder.Entity<ResearchArea>().HasData(
                new ResearchArea { Id = 1, Name = "Artificial Intelligence", Description = "AI, deep learning, neural networks", IsActive = true },
                new ResearchArea { Id = 2, Name = "Web Development", Description = "Frontend, backend, full-stack development", IsActive = true },
                new ResearchArea { Id = 3, Name = "Cybersecurity", Description = "Network security, ethical hacking, cryptography", IsActive = true },
                new ResearchArea { Id = 4, Name = "Cloud Computing", Description = "AWS, Azure, GCP, distributed systems", IsActive = true },
                new ResearchArea { Id = 5, Name = "Machine Learning", Description = "ML algorithms, data science, model training", IsActive = true },
                new ResearchArea { Id = 6, Name = "Mobile Development", Description = "iOS, Android, cross-platform apps", IsActive = true },
                new ResearchArea { Id = 7, Name = "Data Science", Description = "Data analysis, visualization, big data", IsActive = true }
            );
        }
    }
}