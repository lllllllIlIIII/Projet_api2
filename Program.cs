using Microsoft.AspNetCore.Authentication.Cookies;
using Projet_api2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/Login"; 
        options.AccessDeniedPath = "/Error";  
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<UserService>(provider => new UserService(connectionString!));
builder.Services.AddScoped<ProjetService>(provider => new ProjetService(connectionString!)); 
builder.Services.AddScoped<TacheService>(provider => new TacheService(connectionString!));
builder.Services.AddScoped<PdfExportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapRazorPages();

app.Run();