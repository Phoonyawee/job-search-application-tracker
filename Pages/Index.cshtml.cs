using CareerPilot.Data;
using CareerPilot.Models;
using CareerPilot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Pages;

public class IndexModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    public int OpenRoles { get; private set; }
    public int StrongMatches { get; private set; }
    public int ActiveApplications { get; private set; }
    public int Interviews { get; private set; }
    public List<JobMatch> TopMatches { get; private set; } = [];

    public async Task OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return;

        var jobs = await db.JobListings.Where(j => j.IsActive).OrderByDescending(j => j.PostedAt).ToListAsync();
        TopMatches = jobs.Select(j => new JobMatch(j, MatchService.Score(j.RequiredSkills)))
            .OrderByDescending(m => m.Score).Take(3).ToList();
        OpenRoles = jobs.Count;
        StrongMatches = jobs.Count(j => MatchService.Score(j.RequiredSkills) >= 75);

        var userId = users.GetUserId(User)!;
        ActiveApplications = await db.JobApplications.CountAsync(a => a.UserId == userId && a.Status != ApplicationStatus.Rejected);
        Interviews = await db.JobApplications.CountAsync(a => a.UserId == userId && a.Status == ApplicationStatus.Interview);
    }
}
