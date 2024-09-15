using AspNetCoreHero.ToastNotification;
using GameDataCollection.DbContext;
using GameDataCollection.Extension;
using GameDataCollection.Models;
using GameDataCollection.Repositories;
using GameDataCollection.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<UserDbContext>(options =>
              options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("GameDataCollection").CommandTimeout(4000)), ServiceLifetime.Transient);
builder.Services.Configure<SchedulerOptions>(
    builder.Configuration.GetSection("Scheduler"));
// Scoped services that might depend on DbContext
builder.Services.AddScoped<IEmailSetupService, EmailSetupService>();
builder.Services.AddScoped<IGameRecordService, GameRecordService>();

// Register hosted service for the scheduler
builder.Services.AddHostedService<EmailScheduler>();
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<UserDbContext>()
  .AddDefaultTokenProviders();

builder.Services.Scan(scan =>
    scan.FromAssembliesOf(typeof(IGameRecordService), typeof(IGameRecordRepository))
        .AddClasses()
        .AsMatchingInterface());

builder.Services.AddScoped<IEmailSetupService, EmailSetupService>();
builder.Services.AddScoped<IGameRecordService, GameRecordService>();

builder.Services.AddHostedService<EmailScheduler>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(a =>
{
    a.LoginPath = "/admin/Login";
});
builder.Services.AddNotyf(config => { config.DurationInSeconds = 5; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
//builder.Services.AddScoped<IEmailSetupService, EmailSetupService>();
//builder.Services.AddScoped<IGameRecordService, GameRecordService>();
//builder.Services.AddHostedService<EmailScheduler>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=GameRecord}/{action=Index}/{id?}");

app.Run();
