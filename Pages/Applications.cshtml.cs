using CareerPilot.Data;
using CareerPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Pages;

[Authorize]
public class ApplicationsModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    public List<JobApplication> Applications { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = users.GetUserId(User)!;
        Applications = await db.JobApplications.Include(a => a.JobListing)
            .Where(a => a.UserId == userId).OrderByDescending(a => a.UpdatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(int id, ApplicationStatus status, DateOnly? followUpDate, string? notes)
    {
        if (!Enum.IsDefined(status) || notes?.Length > 500) return BadRequest();

        var userId = users.GetUserId(User)!;
        var application = await db.JobApplications.SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (application is null) return NotFound();

        application.Status = status;
        application.FollowUpDate = followUpDate;
        application.Notes = notes?.Trim() ?? "";
        application.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
