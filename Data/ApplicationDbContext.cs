using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareerPilot.Models;

namespace CareerPilot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<JobListing> JobListings => Set<JobListing>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationDocument>().HasOne(d => d.JobApplication).WithOne(a => a.Document)
            .HasForeignKey<ApplicationDocument>(d => d.JobApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}
