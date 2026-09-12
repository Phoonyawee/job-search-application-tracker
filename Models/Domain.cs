namespace CareerPilot.Models;

public enum ApplicationStatus
{
    Saved,
    Applied,
    Interview,
    Offer,
    Rejected
}

public class JobListing
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Company { get; set; }
    public required string Location { get; set; }
    public required string WorkMode { get; set; }
    public required string EmploymentType { get; set; }
    public required string SalaryRange { get; set; }
    public required string Description { get; set; }
    public required string RequiredSkills { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class JobApplication
{
    public int Id { get; set; }
    public int JobListingId { get; set; }
    public JobListing JobListing { get; set; } = null!;
    public required string UserId { get; set; }
    public ApplicationStatus Status { get; set; }
    public string Notes { get; set; } = "";
    public DateOnly? FollowUpDate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record JobMatch(JobListing Job, int Score, ApplicationStatus? Status = null);
