using CareerPilot.Data;
using CareerPilot.Models;
using CareerPilot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Pages;

[Authorize(Roles = "Candidate")]
public class JobsModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public int MinimumMatch { get; set; }
    public List<string> Locations { get; private set; } = [];
    public List<JobMatch> Matches { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var jobs = await db.JobListings.Where(j => j.IsActive).OrderByDescending(j => j.PostedAt).ToListAsync();
        Locations = jobs.Select(j => j.Location).Distinct().Order().ToList();

        if (!string.IsNullOrWhiteSpace(Keyword))
            jobs = jobs.Where(j => $"{j.Title} {j.Company} {j.RequiredSkills}".Contains(Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(Location)) jobs = jobs.Where(j => j.Location == Location).ToList();

        var userId = users.GetUserId(User)!;
        var tracked = await db.JobApplications.Where(a => a.UserId == userId).ToDictionaryAsync(a => a.JobListingId, a => a.Status);
        Matches = jobs.Select(j =>
        {
            var score = MatchService.Score(j.RequiredSkills);
            return tracked.TryGetValue(j.Id, out var status) ? new JobMatch(j, score, status) : new JobMatch(j, score);
        }).Where(m => m.Score >= MinimumMatch).OrderByDescending(m => m.Score).ThenByDescending(m => m.Job.PostedAt).ToList();
    }

    public async Task<IActionResult> OnPostAsync(int jobId, ApplicationStatus status)
    {
        if (status is not (ApplicationStatus.Saved or ApplicationStatus.Applied) || !await db.JobListings.AnyAsync(j => j.Id == jobId && j.IsActive)) return BadRequest();

        var userId = users.GetUserId(User)!;
        var application = await db.JobApplications.SingleOrDefaultAsync(a => a.JobListingId == jobId && a.UserId == userId);
        if (application is null)
            db.JobApplications.Add(new JobApplication { JobListingId = jobId, UserId = userId, Status = status, UpdatedAt = DateTime.UtcNow });
        else if (application.Status == ApplicationStatus.Saved && status == ApplicationStatus.Applied)
        {
            application.Status = status;
            application.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
