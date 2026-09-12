using System.ComponentModel.DataAnnotations;
using CareerPilot.Data;
using CareerPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Pages;

[Authorize(Roles = "Recruiter,Admin")]
public class RecruiterModel(ApplicationDbContext db) : PageModel
{
    [BindProperty] public JobInput Input { get; set; } = new();
    public List<JobListing> Jobs { get; private set; } = [];
    public List<JobApplication> Applications { get; private set; } = [];
    public Dictionary<string, string> CandidateEmails { get; private set; } = [];

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }
        db.JobListings.Add(new JobListing
        {
            Title = Input.Title.Trim(),
            Company = Input.Company.Trim(),
            Location = Input.Location.Trim(),
            WorkMode = Input.WorkMode.Trim(),
            EmploymentType = Input.EmploymentType.Trim(),
            SalaryRange = Input.SalaryRange.Trim(),
            Description = Input.Description.Trim(),
            RequiredSkills = Input.RequiredSkills.Trim(),
            PostedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var job = await db.JobListings.FindAsync(id);
        if (job is null) return NotFound();
        job.IsActive = !job.IsActive;
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Jobs = await db.JobListings.OrderByDescending(j => j.PostedAt).ToListAsync();
        Applications = await db.JobApplications.Include(a => a.JobListing).OrderByDescending(a => a.UpdatedAt).ToListAsync();
        CandidateEmails = await db.Users.ToDictionaryAsync(u => u.Id, u => u.Email ?? "ไม่ทราบอีเมล");
    }

    public sealed class JobInput
    {
        [Required, StringLength(100)] public string Title { get; set; } = "";
        [Required, StringLength(100)] public string Company { get; set; } = "";
        [Required, StringLength(100)] public string Location { get; set; } = "";
        [Required, StringLength(30)] public string WorkMode { get; set; } = "";
        [Required, StringLength(30)] public string EmploymentType { get; set; } = "";
        [Required, StringLength(50)] public string SalaryRange { get; set; } = "";
        [Required, StringLength(500)] public string Description { get; set; } = "";
        [Required, StringLength(300)] public string RequiredSkills { get; set; } = "";
    }
}
