using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Admin,ModuleLeader")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");

            var matches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.Student)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.ResearchArea)
                .Include(m => m.Supervisor)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var proposals = await _context.ProjectProposals
                .Include(p => p.Student)
                .Include(p => p.ResearchArea)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var researchAreas = await _context.ResearchAreas.ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = students.Count,
                TotalSupervisors = supervisors.Count,
                TotalProposals = proposals.Count,
                TotalMatches = matches.Count(m => m.IsConfirmed),
                PendingMatches = matches.Count(m => !m.IsConfirmed),
                Matches = matches,
                Proposals = proposals,
                ResearchAreas = researchAreas
            };

            return View(viewModel);
        }

        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRoleViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Role = roles.FirstOrDefault() ?? "None"
                });
            }

            return View(usersWithRoles);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Department = model.Department,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["Success"] = $"User {model.FullName} created successfully!";
                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Prevent deleting yourself
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete user!";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // GET: Admin/ManageResearchAreas
        // GET: Admin/ManageResearchAreas
        [Authorize(Roles = "ModuleLeader")]
        public async Task<IActionResult> ManageResearchAreas()
        {
            var areas = await _context.ResearchAreas
                .OrderBy(r => r.Name)
                .ToListAsync();
            return View(areas);
        }

        // POST: Admin/CreateResearchArea
        [Authorize(Roles = "ModuleLeader")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearchArea(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Research area name is required!";
                return RedirectToAction(nameof(ManageResearchAreas));
            }

            var area = new ResearchArea
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? "",
                IsActive = true
            };

            _context.ResearchAreas.Add(area);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Research area created successfully!";
            return RedirectToAction(nameof(ManageResearchAreas));
        }

        // POST: Admin/ToggleResearchArea
        [Authorize(Roles = "ModuleLeader")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleResearchArea(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null)
            {
                TempData["Error"] = "Research area not found!";
                return RedirectToAction(nameof(ManageResearchAreas));
            }

            area.IsActive = !area.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Research area {(area.IsActive ? "activated" : "deactivated")} successfully!";
            return RedirectToAction(nameof(ManageResearchAreas));
        }

        // POST: Admin/ReassignMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignMatch(int matchId, string newSupervisorId)
        {
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                TempData["Error"] = "Match not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            var newSupervisor = await _userManager.FindByIdAsync(newSupervisorId);
            if (newSupervisor == null)
            {
                TempData["Error"] = "Supervisor not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            match.SupervisorId = newSupervisorId;
            match.IsConfirmed = false; // Reset confirmation
            match.ConfirmedAt = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Match reassigned successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Admin/DeleteMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMatch(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                TempData["Error"] = "Match not found!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Reset proposal status
            if (match.ProjectProposal != null)
            {
                match.ProjectProposal.Status = ProposalStatus.Pending;
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Match deleted successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalSupervisors { get; set; }
        public int TotalProposals { get; set; }
        public int TotalMatches { get; set; }
        public int PendingMatches { get; set; }
        public List<Match> Matches { get; set; } = new();
        public List<ProjectProposal> Proposals { get; set; } = new();
        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }

    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
    }

    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}