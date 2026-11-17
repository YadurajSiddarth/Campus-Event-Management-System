using CEMS.Models;
using CEMS.Services;

namespace CEMS.Data;

/// <summary>Demo data loaded the first time the application runs on an empty database.</summary>
public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User { FullName = "Admin User", Email = "admin@cems.edu.my", PasswordHash = PasswordHasher.Hash("Admin@123"), Role = "Admin", Department = "IT Department" };
        var staff = new User { FullName = "Dr. Ahmad Organiser", Email = "ahmad@cems.edu.my", PasswordHash = PasswordHasher.Hash("Staff@123"), Role = "Staff", Department = "Student Affairs", ContactNumber = "012-3456789" };
        var student1 = new User { FullName = "Siti Nurhaliza", Email = "siti@student.cems.edu.my", PasswordHash = PasswordHasher.Hash("Student@123"), Role = "Student", Department = "BCSI", ContactNumber = "011-1234567" };
        var student2 = new User { FullName = "Raj Kumar", Email = "raj@student.cems.edu.my", PasswordHash = PasswordHasher.Hash("Student@123"), Role = "Student", Department = "BCSI", ContactNumber = "012-9876543" };
        var student3 = new User { FullName = "Lee Wei Ming", Email = "wei@student.cems.edu.my", PasswordHash = PasswordHasher.Hash("Student@123"), Role = "Student", Department = "BCS", ContactNumber = "013-5556677" };

        db.Users.AddRange(admin, staff, student1, student2, student3);
        db.SaveChanges();

        var today = DateTime.Today;
        var workshop = new Event { Title = "Web Development Workshop 2026", Description = "A hands-on workshop covering ASP.NET Core, Razor Pages, and modern web development practices. Participants will build a mini project.", EventDate = today.AddDays(14).AddHours(9), Venue = "Lab 3-01, Block A", Category = "Workshop", MaxSeats = 40, OrganiserId = staff.UserId };
        var sports = new Event { Title = "INTI Annual Sports Day", Description = "Join us for the annual inter-faculty sports competition. Events include badminton, futsal, basketball, and track & field.", EventDate = today.AddDays(21).AddHours(8), Venue = "INTI Sports Complex", Category = "Sports", MaxSeats = 200, OrganiserId = staff.UserId };
        var seminar = new Event { Title = "AI & Machine Learning Seminar", Description = "Industry experts share insights on the latest trends in AI, deep learning, and their applications in Malaysia.", EventDate = today.AddDays(7).AddHours(14), Venue = "Auditorium, Block B", Category = "Seminar", MaxSeats = 100, OrganiserId = staff.UserId };
        var photo = new Event { Title = "Photography Club Exhibition", Description = "A gallery showcasing the best student photography from the semester. Open to all students and staff.", EventDate = today.AddDays(10).AddHours(10), Venue = "Gallery Hall, Student Centre", Category = "Club Activity", MaxSeats = 80, OrganiserId = staff.UserId };
        var fair = new Event { Title = "Career Fair 2026", Description = "Meet recruiters from top Malaysian tech companies. Bring your CV and dress professionally.", EventDate = today.AddDays(30).AddHours(9), Venue = "Main Hall", Category = "Seminar", MaxSeats = 300, OrganiserId = admin.UserId };
        var hack = new Event { Title = "Coding Competition - HackINTI", Description = "48-hour hackathon. Form teams of 3-5 and build an innovative solution. Cash prizes for top 3 teams.", EventDate = today.AddDays(-5).AddHours(9), Venue = "Lab 2-01, Block A", Category = "Workshop", MaxSeats = 60, OrganiserId = staff.UserId, Status = "Completed" };

        db.Events.AddRange(workshop, sports, seminar, photo, fair, hack);
        db.SaveChanges();

        db.EventResources.Add(new EventResource { EventId = workshop.EventId, FileName = "web-workshop-brochure.pdf", FilePath = "/uploads/seed/web-workshop-brochure.pdf" });

        var r1 = new Registration { UserId = student1.UserId, EventId = workshop.EventId, Remarks = "Vegetarian lunch please" };
        var r2 = new Registration { UserId = student1.UserId, EventId = seminar.EventId };
        var r3 = new Registration { UserId = student2.UserId, EventId = workshop.EventId };
        var r4 = new Registration { UserId = student2.UserId, EventId = sports.EventId };
        var r5 = new Registration { UserId = student3.UserId, EventId = seminar.EventId };
        var r6 = new Registration { UserId = student1.UserId, EventId = hack.EventId, Status = "Attended" };
        var r7 = new Registration { UserId = student2.UserId, EventId = hack.EventId, Status = "Attended" };
        var r8 = new Registration { UserId = student3.UserId, EventId = photo.EventId, Status = "Cancelled", CancelledAt = DateTime.Now.AddDays(-1), CancelReason = "Schedule conflict with another class" };
        db.Registrations.AddRange(r1, r2, r3, r4, r5, r6, r7, r8);

        db.RegistrationLogs.AddRange(
            new RegistrationLog { Registration = r1, ActorUserId = student1.UserId, Action = "Registered", Details = "Registered for " + workshop.Title },
            new RegistrationLog { Registration = r2, ActorUserId = student1.UserId, Action = "Registered", Details = "Registered for " + seminar.Title },
            new RegistrationLog { Registration = r3, ActorUserId = student2.UserId, Action = "Registered", Details = "Registered for " + workshop.Title },
            new RegistrationLog { Registration = r4, ActorUserId = student2.UserId, Action = "Registered", Details = "Registered for " + sports.Title },
            new RegistrationLog { Registration = r5, ActorUserId = student3.UserId, Action = "Registered", Details = "Registered for " + seminar.Title },
            new RegistrationLog { Registration = r6, ActorUserId = student1.UserId, Action = "Registered", Details = "Registered for " + hack.Title },
            new RegistrationLog { Registration = r6, ActorUserId = staff.UserId, Action = "Attended", Details = "Marked as attended by organiser" },
            new RegistrationLog { Registration = r7, ActorUserId = student2.UserId, Action = "Registered", Details = "Registered for " + hack.Title },
            new RegistrationLog { Registration = r7, ActorUserId = staff.UserId, Action = "Attended", Details = "Marked as attended by organiser" },
            new RegistrationLog { Registration = r8, ActorUserId = student3.UserId, Action = "Registered", Details = "Registered for " + photo.Title },
            new RegistrationLog { Registration = r8, ActorUserId = student3.UserId, Action = "Cancelled", Details = "Reason: Schedule conflict with another class" }
        );

        db.Feedbacks.AddRange(
            new Feedback { EventId = hack.EventId, UserId = student1.UserId, Rating = 5, Comments = "Amazing hackathon! Learned so much about teamwork and coding under pressure." },
            new Feedback { EventId = hack.EventId, UserId = student2.UserId, Rating = 4, Comments = "Great event, but the food could have been better. Overall a wonderful experience." }
        );

        db.Notifications.AddRange(
            new Notification { UserId = student1.UserId, Message = "Registration confirmed: Web Development Workshop 2026." },
            new Notification { UserId = student1.UserId, Message = "Registration confirmed: AI & Machine Learning Seminar." },
            new Notification { UserId = student2.UserId, Message = "Registration confirmed: INTI Annual Sports Day." },
            new Notification { UserId = student3.UserId, Message = "Your registration for Photography Club Exhibition has been cancelled." }
        );

        db.Enquiries.Add(new Enquiry { EventId = workshop.EventId, UserId = student1.UserId, Subject = "Laptop Requirements", Message = "Do we need to bring our own laptop for the workshop? What software should we install beforehand?", Reply = "Yes, please bring your laptop with Visual Studio 2022 and the .NET SDK installed.", RepliedAt = DateTime.Now.AddDays(-1) });
        db.Enquiries.Add(new Enquiry { EventId = seminar.EventId, UserId = student3.UserId, Subject = "Certificate of attendance", Message = "Will a certificate be provided to all participants?" });

        db.SaveChanges();
    }
}
