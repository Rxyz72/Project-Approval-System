using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Student/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var proposals = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.StudentId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var matches = await _context.Matches
                .Include(m => m.ProjectProposal)
                .Include(m => m.Supervisor)
                .Where(m => m.ProjectProposal.StudentId == userId)
                .ToListAsync();

            var viewModel = new StudentDashboardViewModel
            {
                Proposals = proposals,
                Matches = matches
            };

            return View(viewModel);
        }

        // GET: Student/CreateProposal
        public async Task<IActionResult> CreateProposal()
        {
            ViewBag.ResearchAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .ToListAsync();
            return View();
        }

        // POST: Student/CreateProposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProposal(CreateProposalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var proposal = new ProjectProposal
                {
                    Title = model.Title,
                    Abstract = model.Abstract,
                    TechStack = model.TechStack,
                    ResearchAreaId = model.ResearchAreaId,
                    StudentId = userId!,
                    Status = ProposalStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProjectProposals.Add(proposal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Project proposal submitted successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.ResearchAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .ToListAsync();
            return View(model);
        }

        // GET: Student/EditProposal/5
        public async Task<IActionResult> EditProposal(int id)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (proposal == null)
                return NotFound();

            // Can't edit if already matched
            if (proposal.Status == ProposalStatus.Matched)
            {
                TempData["Error"] = "Cannot edit a matched proposal!";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.ResearchAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .ToListAsync();

            var viewModel = new CreateProposalViewModel
            {
                Title = proposal.Title,
                Abstract = proposal.Abstract,
                TechStack = proposal.TechStack,
                ResearchAreaId = proposal.ResearchAreaId
            };

            ViewBag.ProposalId = id;
            return View("CreateProposal", viewModel);
        }

        // POST: Student/EditProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProposal(int id, CreateProposalViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (proposal == null)
                return NotFound();

            if (proposal.Status == ProposalStatus.Matched)
            {
                TempData["Error"] = "Cannot edit a matched proposal!";
                return RedirectToAction(nameof(Dashboard));
            }

            if (ModelState.IsValid)
            {
                proposal.Title = model.Title;
                proposal.Abstract = model.Abstract;
                proposal.TechStack = model.TechStack;
                proposal.ResearchAreaId = model.ResearchAreaId;
                proposal.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Proposal updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.ResearchAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .ToListAsync();
            ViewBag.ProposalId = id;
            return View("CreateProposal", model);
        }

        // POST: Student/WithdrawProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawProposal(int id)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (proposal == null)
                return NotFound();

            if (proposal.Status == ProposalStatus.Matched)
            {
                TempData["Error"] = "Cannot withdraw a matched proposal!";
                return RedirectToAction(nameof(Dashboard));
            }

            proposal.Status = ProposalStatus.Withdrawn;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proposal withdrawn successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Student/DeleteProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProposal(int id)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);

            if (proposal == null)
                return NotFound();

            if (proposal.Status == ProposalStatus.Matched)
            {
                TempData["Error"] = "Cannot delete a matched proposal!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Check if there are any pending matches
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => m.ProjectProposalId == id);

            if (existingMatch != null)
            {
                _context.Matches.Remove(existingMatch);
            }

            _context.ProjectProposals.Remove(proposal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proposal deleted successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
    }

        // ViewModels
        public class StudentDashboardViewModel
        {
            public List<ProjectProposal> Proposals { get; set; } = new();
            public List<Match> Matches { get; set; } = new();
        }

        public class CreateProposalViewModel
        {
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
        }


}