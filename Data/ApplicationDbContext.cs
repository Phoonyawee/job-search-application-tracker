using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareerPilot.Models;

namespace CareerPilot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<JobListing> JobListings => Set<JobListing>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
}
