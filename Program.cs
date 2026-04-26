using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Data.Seed;
using ThirteenthAvenue.Models;
using ThirteenthAvenue.Services.Email;
using ThirteenthAvenue.Services.Pdf;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TicketPdfService>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<AuthMessageSenderOptions>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await DbSeeder.SeedRolesAndAdminAsync(services);

    var context = services.GetRequiredService<ApplicationDbContext>();
    await CategorySeeder.SeedAsync(context);
}
catch (Exception ex)
{
    Console.WriteLine($"Seeding skipped: {ex.Message}");
}
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    //await DbSeeder.SeedRolesAndAdminAsync(services);
//}


//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<ApplicationDbContext>();

//    await CategorySeeder.SeedAsync(context);
//}

app.Run();