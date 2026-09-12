# CareerPilot

Skill-based job matching and application tracking built as a portfolio project for an IT Support / ERP Developer profile.

![CareerPilot home](docs/screenshots/careerpilot-home.png)

## What it demonstrates

- Ranks fictional job listings against a technical skill profile
- Filters opportunities by keyword, location, and match score
- Saves jobs and tracks Applied, Interview, Offer, and Rejected stages
- Stores follow-up dates and notes per signed-in user
- Uses ASP.NET Core Razor Pages, Identity, EF Core, and SQLite

## Demo account

- Email: `candidate@careerpilot.local`
- Password: `Demo123!`

## Run locally

```powershell
dotnet restore
dotnet run
```

Open the localhost URL shown in the terminal. Job listings and companies are synthetic demo data.

## Check

```powershell
dotnet build --configuration Release
dotnet run --no-build --configuration Release -- --self-check
```
