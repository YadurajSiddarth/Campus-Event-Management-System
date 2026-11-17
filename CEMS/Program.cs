using Microsoft.EntityFrameworkCore;
using CEMS.Data;
using CEMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Microsoft SQL Server (LocalDB by default - see ConnectionStrings:Default in appsettings.json)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Session control (30 minute idle timeout)
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

// Application services
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<EventResourceService>();

var app = builder.Build();

// Create the database (if it does not exist yet) and load demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
