using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using MyRazorApp.Data;
using Microsoft.AspNetCore.Identity;
using MyRazorApp.Models; // AppIdentityUser için gerekli

var builder = WebApplication.CreateBuilder(args);

// Razor Pages hizmetini ekle
builder.Services.AddRazorPages();

// EF Core SQL Server bağlantısı
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnection")));

// ❗ Identity sistemine AppIdentityUser sınıfını tanıt
builder.Services.AddIdentity<AppIdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<SchoolDbContext>()
    .AddDefaultTokenProviders();


// Oturum yönetimi
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Üretim ortamı için hata sayfası ve HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Orta katmanlar
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();     // Session middleware
app.UseAuthentication(); // ❗ Kimlik doğrulama middleware
app.UseAuthorization();

app.MapRazorPages();

app.Run();
