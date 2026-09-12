using CareerPilot.Models;

namespace CareerPilot.Services;

public static class MatchService
{
    public static readonly string[] CandidateSkills =
    [
        "C#", "ASP.NET Core", "SQL", "MySQL", "ERP", "IT Support",
        "JavaScript", "Next.js", "Python", "Java", "Git", "Troubleshooting"
    ];

    public static int Score(string requiredSkills, IEnumerable<string>? candidateSkills = null)
    {
        var required = requiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (required.Length == 0) return 0;

        var candidate = new HashSet<string>(candidateSkills ?? CandidateSkills, StringComparer.OrdinalIgnoreCase);
        return (int)Math.Round(required.Count(candidate.Contains) * 100d / required.Length);
    }

    public static void SelfCheck()
    {
        if (Score("C#, SQL", ["C#", "SQL"]) != 100 ||
            Score("C#, SQL", ["Python"]) != 0 ||
            Score("C#, SQL", ["SQL"]) != 50)
            throw new InvalidOperationException("Job matching self-check failed.");
    }
}
