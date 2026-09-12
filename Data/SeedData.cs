using CareerPilot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        const string email = "candidate@careerpilot.local";
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(user, "Demo123!");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await db.JobListings.AnyAsync())
        {
            db.JobListings.AddRange(
                Job("ERP Application Support Specialist", "Siam Manufacturing", "Chonburi", "On-site", "Full-time", "฿28,000–38,000", "Support ERP users, troubleshoot incidents, and coordinate fixes with business teams.", "ERP, IT Support, SQL, Troubleshooting", 1),
                Job("Junior .NET Developer", "Blue Orbit Solutions", "Bangkok", "Hybrid", "Full-time", "฿30,000–42,000", "Build and maintain internal business applications with the Microsoft stack.", "C#, ASP.NET Core, SQL, Git", 2),
                Job("IT Support Engineer", "Eastern Logistics", "Chonburi", "On-site", "Full-time", "฿25,000–34,000", "Resolve hardware, software, network, and user-account issues across branch offices.", "IT Support, Troubleshooting, SQL", 3),
                Job("Business Systems Analyst", "Rayong Parts Group", "Rayong", "Hybrid", "Full-time", "฿35,000–48,000", "Gather requirements and improve workflows between ERP and operations teams.", "ERP, SQL, Troubleshooting, Power BI", 4),
                Job("Web Application Developer", "RemoteCraft", "Remote", "Remote", "Contract", "฿35,000–50,000", "Deliver customer-facing features and maintain modern web applications.", "JavaScript, Next.js, Git, React, TypeScript", 5));
            await db.SaveChangesAsync();
        }

        if (!await db.JobApplications.AnyAsync(a => a.UserId == user.Id))
        {
            var jobs = await db.JobListings.OrderBy(j => j.Id).Take(2).ToListAsync();
            db.JobApplications.AddRange(
                new JobApplication { JobListingId = jobs[0].Id, UserId = user.Id, Status = ApplicationStatus.Applied, Notes = "Submitted through company career page.", FollowUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)), UpdatedAt = DateTime.UtcNow },
                new JobApplication { JobListingId = jobs[1].Id, UserId = user.Id, Status = ApplicationStatus.Saved, Notes = "Review the job description before applying.", UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    private static JobListing Job(string title, string company, string location, string mode, string type, string salary, string description, string skills, int daysAgo) =>
        new() { Title = title, Company = company, Location = location, WorkMode = mode, EmploymentType = type, SalaryRange = salary, Description = description, RequiredSkills = skills, PostedAt = DateTime.UtcNow.AddDays(-daysAgo) };
}
