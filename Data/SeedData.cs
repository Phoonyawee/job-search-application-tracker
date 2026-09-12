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
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Candidate", "Recruiter", "Admin" })
        {
            if (!await roles.RoleExistsAsync(role)) EnsureSucceeded(await roles.CreateAsync(new IdentityRole(role)));
        }

        var user = await EnsureUserAsync(users, "candidate@careerpilot.local", "Candidate");
        await EnsureUserAsync(users, "recruiter@careerpilot.local", "Recruiter");
        await EnsureUserAsync(users, "admin@careerpilot.local", "Admin");

        if (!await db.JobListings.AnyAsync())
        {
            db.JobListings.AddRange(
                Job("ผู้เชี่ยวชาญฝ่ายสนับสนุนระบบ ERP", "Siam Manufacturing", "ชลบุรี", "หน้างาน", "เต็มเวลา", "฿28,000–38,000", "ดูแลผู้ใช้งาน ERP แก้ไขเหตุขัดข้อง และประสานงานกับทีมธุรกิจเพื่อปรับปรุงระบบ", "ERP, IT Support, SQL, Troubleshooting", 1),
                Job("นักพัฒนา .NET ระดับ Junior", "Blue Orbit Solutions", "กรุงเทพมหานคร", "ไฮบริด", "เต็มเวลา", "฿30,000–42,000", "พัฒนาและดูแลแอปพลิเคชันภายในองค์กรด้วยเทคโนโลยี Microsoft", "C#, ASP.NET Core, SQL, Git", 2),
                Job("วิศวกร IT Support", "Eastern Logistics", "ชลบุรี", "หน้างาน", "เต็มเวลา", "฿25,000–34,000", "แก้ไขปัญหาฮาร์ดแวร์ ซอฟต์แวร์ เครือข่าย และบัญชีผู้ใช้ของสำนักงานสาขา", "IT Support, Troubleshooting, SQL", 3),
                Job("นักวิเคราะห์ระบบธุรกิจ", "Rayong Parts Group", "ระยอง", "ไฮบริด", "เต็มเวลา", "฿35,000–48,000", "เก็บความต้องการและปรับปรุงกระบวนการทำงานระหว่างระบบ ERP กับทีมปฏิบัติการ", "ERP, SQL, Troubleshooting, Power BI", 4),
                Job("นักพัฒนาเว็บแอปพลิเคชัน", "RemoteCraft", "ทำงานทางไกล", "รีโมต", "สัญญาจ้าง", "฿35,000–50,000", "พัฒนาฟีเจอร์สำหรับลูกค้าและดูแลเว็บแอปพลิเคชันสมัยใหม่", "JavaScript, Next.js, Git, React, TypeScript", 5));
            await db.SaveChangesAsync();
        }

        if (!await db.JobApplications.AnyAsync(a => a.UserId == user.Id))
        {
            var jobs = await db.JobListings.OrderBy(j => j.Id).Take(2).ToListAsync();
            db.JobApplications.AddRange(
                new JobApplication { JobListingId = jobs[0].Id, UserId = user.Id, Status = ApplicationStatus.Applied, Notes = "ส่งใบสมัครผ่านหน้าเว็บไซต์ของบริษัทแล้ว", FollowUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)), UpdatedAt = DateTime.UtcNow },
                new JobApplication { JobListingId = jobs[1].Id, UserId = user.Id, Status = ApplicationStatus.Saved, Notes = "ตรวจรายละเอียดงานอีกครั้งก่อนสมัคร", UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    private static JobListing Job(string title, string company, string location, string mode, string type, string salary, string description, string skills, int daysAgo) =>
        new() { Title = title, Company = company, Location = location, WorkMode = mode, EmploymentType = type, SalaryRange = salary, Description = description, RequiredSkills = skills, PostedAt = DateTime.UtcNow.AddDays(-daysAgo) };

    private static async Task<IdentityUser> EnsureUserAsync(UserManager<IdentityUser> users, string email, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(user, "Demo123!");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        if ((await users.GetRolesAsync(user)).Count == 0) EnsureSucceeded(await users.AddToRoleAsync(user, role));
        return user;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
