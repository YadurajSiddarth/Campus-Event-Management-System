# Campus Event Management System (CEMS)

**PRG3204 Web Application Development - Assessment 3 (Group Project)**

A web-based system that lets a university manage campus events (seminars, workshops, sports and club
activities): event creation, online registration with seat limits, registration change logs, search and
enquiries, notifications, attendance/feedback reports and role-based account management.

## Technology stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core Razor Pages, C# (.NET 10) |
| Database | Microsoft SQL Server (Entity Framework Core) |
| Frontend | HTML5, CSS3, JavaScript, Bootstrap 5 |
| Session control | ASP.NET Core session (30 min idle timeout) + role filter (`[SessionAuthorize]`) |
| Notifications | In-app notifications + optional e-mail through SMTP |
| Hosting | Local (Kestrel) |

## Repository structure

```
.
├── CEMS/                      ASP.NET Core project (open CEMS.csproj in Visual Studio, or use the CLI)
│   ├── Data/                  AppDbContext (EF Core) and demo SeedData
│   ├── Infrastructure/        SessionAuthorizeAttribute (login + role check for pages)
│   ├── Models/                Entity classes and form models
│   ├── Pages/                 Razor Pages
│   │   ├── Account/           Login, Register, Profile (+ change password), Logout
│   │   ├── Admin/             User and role management
│   │   ├── Events/            List, Calendar, Create, Edit/Cancel, Detail, History, Logs, Attendance
│   │   ├── Shared/            _Layout
│   │   └── ...                Home, Enquiries, Notifications, Reports, AccessDenied, Error
│   ├── Services/              Registration rules, notifications, e-mail, file upload, password hashing
│   ├── wwwroot/               CSS, JavaScript, Bootstrap, uploaded event files
│   ├── Program.cs             Start-up and configuration
│   └── appsettings.json       Database connection string and e-mail settings
├── database/CEMS_schema.sql   SQL Server schema (reference - the app creates it automatically)
├── docs/                      Project report (PDF / DOCX)
└── README.md
```

## How to run (Windows)

**Requirements**

1. [.NET 10 SDK](https://dotnet.microsoft.com/download)
2. Microsoft SQL Server - one of:
   * **SQL Server LocalDB** (installed with Visual Studio's *ASP.NET and web development* / *Data storage* workloads) - works with the default settings, nothing to configure; or
   * **SQL Server Express / Developer** - change `ConnectionStrings:Default` in `CEMS/appsettings.json` to
     `Server=localhost\\SQLEXPRESS;Database=CEMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

**Steps**

```bash
git clone 
cd 
dotnet run
```

Open the address printed in the terminal (normally `http://localhost:5258`).
On the first start the application creates the **CEMS** database, its tables and demo data automatically.
To start again from scratch, drop the `CEMS` database and run the application again.

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@cems.edu.my | Admin@123 |
| Staff / Organiser | ahmad@cems.edu.my | Staff@123 |
| Student | siti@student.cems.edu.my | Student@123 |
| Student | raj@student.cems.edu.my | Student@123 |
| Student | wei@student.cems.edu.my | Student@123 |

## Features (mapped to the assignment)

| Requirement | Where in the system |
|-------------|--------------------|
| **Event management** - create, update, cancel; calendar; upload brochures | `Events/Create`, `Events/Edit` (update, cancel event, upload/remove files), `Events/Calendar` (month calendar + upcoming list), `Events/Detail` (downloads) |
| **Event registration** - online registration, history, seat limit | `Events/Detail` (register, seat check, duplicate prevention), `Events/History` |
| **Event modification** - update/cancel registration, logs | `Events/History` and `Events/Detail` (update remarks, cancel with reason), `Events/Logs` (audit trail of every change) |
| **Enquiry and notifications** - search, contact organiser, email/SMS | `Events/Index` (keyword, category, date range, organiser), `Events/Detail` (contact organiser), `Enquiries` (organiser replies), `Notifications`, optional SMTP e-mail |
| **Reports** - attendance, popular events, feedback | `Events/Attendance` (organiser marks attendance), `Reports` (attendance summary, popular events, categories), feedback form on `Events/Detail` |
| **Account management** - password, profile, roles | `Account/Profile` (profile + change password), `Account/Register`, `Admin` (roles), role checks on every protected page |

### Roles

| Role | Can do |
|------|--------|
| Student | Browse/search events, register, update/cancel own registration, give feedback after attending, send enquiries, manage own profile |
| Staff (organiser) | Everything a student can, plus create/edit/cancel **own** events, upload resources, mark attendance, answer enquiries, view reports |
| Admin | Everything, on all events, plus user and role management |

### Enabling e-mail notifications (optional)

Every notification is always stored in the system. To also send e-mail, fill in the `Email` section of
`CEMS/appsettings.json` (for Gmail use an *App Password*) and set `"Enabled": true`. While disabled, the
e-mail text is written to the application log instead.

## Team

| Member | Module |
|--------|--------|
| Member 1 | Event creation, calendar view, resource upload |
| Member 2 | Event registration and registration history |
| Member 3 | Event modification and registration logs |
| Member 4 | Reports and feedback |
| Member 5 | Notifications, e-mail integration and account management |

_(Replace "Member 1-5" with the real names and student IDs before submitting.)_

## Submission checklist

- [ ] Report saved as `GroupName.pdf` (20-40 pages), Turnitin similarity under 20%
- [ ] This repository / project folder with all source code
- [ ] Pre-recorded presentation and demo video - link: _add link here_
- [ ] Peer evaluation forms (Appendix A) compiled in the report
