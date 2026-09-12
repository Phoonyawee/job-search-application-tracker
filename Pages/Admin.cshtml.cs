using CareerPilot.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Pages;

[Authorize(Roles = "Admin")]
public class AdminModel(ApplicationDbContext db, UserManager<IdentityUser> users) : PageModel
{
    public List<AccountRow> Accounts { get; private set; } = [];

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostAsync(string userId, string role)
    {
        string[] allowedRoles = ["Candidate", "Recruiter", "Admin"];
        if (!allowedRoles.Contains(role)) return BadRequest();
        var account = await users.FindByIdAsync(userId);
        if (account is null) return NotFound();
        if (users.GetUserId(User) == userId && role != "Admin") return BadRequest();

        var currentRoles = await users.GetRolesAsync(account);
        if (currentRoles.Count == 1 && currentRoles[0] == role) return RedirectToPage();
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (currentRoles.Count > 0 && !(await users.RemoveFromRolesAsync(account, currentRoles)).Succeeded) return BadRequest();
        if (!(await users.AddToRoleAsync(account, role)).Succeeded) return BadRequest();
        await transaction.CommitAsync();
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        foreach (var account in await db.Users.OrderBy(u => u.Email).ToListAsync())
        {
            var roles = await users.GetRolesAsync(account);
            Accounts.Add(new AccountRow(account.Id, account.Email ?? "ไม่ทราบอีเมล", roles.Count > 0 ? string.Join(", ", roles) : "ไม่มีบทบาท"));
        }
    }

    public record AccountRow(string Id, string Email, string Role);
}
