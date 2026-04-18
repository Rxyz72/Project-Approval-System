using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Supervisor/Dashboard
        public async Task<IActionResult> Dashboard(int? researchAreaId)
        {
            var userId = _userManager.GetUserId(User);

            // Get all available proposals (Pending or UnderReview) - BLIND (no student info)
            var query = _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview)
                .AsQueryable();

            // Filter by research area if selected
            if (researchAreaId.HasValue)
            {
                query = query.Where(p => p.ResearchAreaId == researchAreaId.Value);
            }

            var availableProposals = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get supervisor's existing matches
            var myMatches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.ResearchArea)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.Student)
                .Where(m => m.SupervisorId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Get all research areas for filter
            var researchAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            var viewModel = new SupervisorDashboardViewModel
            {
                AvailableProposals = availableProposals,
                MyMatches = myMatches,
                ResearchAreas = researchAreas,
                SelectedResearchAreaId = researchAreaId
            };

            return View(viewModel);
        }

        // POST: Supervisor/ExpressInterest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null)
            {
                TempData["Error"] = "Proposal not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Check if already matched
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectProposalId == proposalId);

            if (existingMatch != null)
            {
                TempData["Error"] = "This proposal is already matched!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Create match (not confirmed yet - awaiting student approval if needed)
            var match = new Match
            {
                ProjectProposalId = proposalId,
                SupervisorId = userId!,
                IsConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Matches.Add(match);
            proposal.Status = ProposalStatus.UnderReview;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Interest expressed! Waiting for confirmation...";
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Supervisor/ConfirmMatch/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var userId = _userManager.GetUserId(User);
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId && m.SupervisorId == userId);

            if (match == null)
            {
                TempData["Error"] = "Match not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Confirm the match - IDENTITY REVEAL happens here
            match.IsConfirmed = true;
            match.ConfirmedAt = DateTime.UtcNow;
            match.ProjectProposal!.Status = ProposalStatus.Matched;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Match confirmed! Identity revealed. You can now contact the student.";
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Supervisor/WithdrawInterest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawInterest(int matchId)
        {
            var userId = _userManager.GetUserId(User);
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId && m.SupervisorId == userId);

            if (match == null)
            {
                TempData["Error"] = "Match not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            if (match.IsConfirmed)
            {
                TempData["Error"] = "Cannot withdraw from a confirmed match!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Remove match and reset proposal status
            match.ProjectProposal!.Status = ProposalStatus.Pending;
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Interest withdrawn successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    // ViewModel
    public class SupervisorDashboardViewModel
    {
        public List<ProjectProposal> AvailableProposals { get; set; } = new();
        public List<Match> MyMatches { get; set; } = new();
        public List<ResearchArea> ResearchAreas { get; set; } = new();
        public int? SelectedResearchAreaId { get; set; }
    }
}