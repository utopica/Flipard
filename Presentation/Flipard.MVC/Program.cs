using Flipard.Domain.Identity;
using Flipard.Domain.Interfaces;
using Flipard.Infrastructure;
using Flipard.MVC.Services;
using Flipard.Persistence;
using Flipard.Persistence.Concretes;
using Flipard.Persistence.Contexts;
using Flipard.Persistence.Contexts.Identity;
using Flipard.Persistence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr();

builder.Services.AddMvc().AddNToastNotifyToastr();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("app.development.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;

var tessdata = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tessdata");
builder.Services.AddPersistenceServices(configuration);
builder.Services.AddInfrastructureServices(tessdata);

builder.Services.AddScoped<INToastNotifyService, NToastNotifyService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSession();

builder.Services.AddIdentity<User,Role>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvqxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*_-!%^&/=€$@";
    options.User.RequireUniqueEmail = true;

}).AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders(); 

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Auth/Login");
    options.LogoutPath = new PathString("/Auth/Logout");
    options.Cookie = new CookieBuilder
    {
        Name = "TestCookie",
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        SecurePolicy = CookieSecurePolicy.SameAsRequest
    };

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = System.TimeSpan.FromDays(7); //remember login information for seven days.
    options.AccessDeniedPath = new PathString("/Auth/AccessDenied");
});

var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Session settings
app.UseSession();

app.UseNToastNotify();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
