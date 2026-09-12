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
public class ApplicationsModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    [TempData] public string? Message { get; set; }
    [TempData] public string? ErrorMessage { get; set; }
    public List<JobApplication> Applications { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = users.GetUserId(User)!;
        Applications = await db.JobApplications.Include(a => a.JobListing).Include(a => a.Document)
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

    public async Task<IActionResult> OnPostUploadAsync(int id, IFormFile? resume)
    {
        var userId = users.GetUserId(User)!;
        var application = await db.JobApplications.Include(a => a.Document)
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (application is null) return NotFound();

        if (resume is null || resume.Length is <= 0 or > ResumeFileValidator.MaxBytes)
        {
            ErrorMessage = "กรุณาเลือกไฟล์ PDF หรือ DOCX ที่มีขนาดไม่เกิน 5 MB";
            return RedirectToPage();
        }

        var fileName = Path.GetFileName(resume.FileName);
        if (fileName.Length > 255)
        {
            ErrorMessage = "ชื่อไฟล์ยาวเกิน 255 ตัวอักษร";
            return RedirectToPage();
        }

        await using var stream = new MemoryStream((int)resume.Length);
        await resume.CopyToAsync(stream);
        var data = stream.ToArray();
        var contentType = ResumeFileValidator.GetContentType(fileName, data);
        if (data.Length > ResumeFileValidator.MaxBytes || contentType is null)
        {
            ErrorMessage = "ไฟล์ไม่ถูกต้อง ระบบรองรับเฉพาะ PDF และ DOCX เท่านั้น";
            return RedirectToPage();
        }

        if (application.Document is null)
            db.ApplicationDocuments.Add(new ApplicationDocument { JobApplicationId = id, FileName = fileName, ContentType = contentType, Data = data, UploadedAt = DateTime.UtcNow });
        else
        {
            application.Document.FileName = fileName;
            application.Document.ContentType = contentType;
            application.Document.Data = data;
            application.Document.UploadedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        Message = "แนบเอกสารสมัครงานเรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetResumeAsync(int id)
    {
        var userId = users.GetUserId(User)!;
        var document = await db.ApplicationDocuments.SingleOrDefaultAsync(d => d.JobApplicationId == id && d.JobApplication.UserId == userId);
        return document is null ? NotFound() : File(document.Data, document.ContentType, document.FileName);
    }
}
