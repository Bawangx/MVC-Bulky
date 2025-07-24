using Bulky.DataAccess.DbInitializer;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// 1. Tambahkan DbContext untuk akses database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Setup ASP.NET Core Identity untuk user management dan autentikasi
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = false) // Tidak pakai konfirmasi email dulu
    .AddEntityFrameworkStores<ApplicationDbContext>() // Simpan data user di EF Core
    .AddDefaultTokenProviders(); // Token untuk reset password, email confirmation, dll.

// 3. Konfigurasi path login, logout, dan akses ditolak
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 4. Tambahkan Facebook Authentication
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "1141841511058275";
    option.AppSecret = "c25b1216a10ac9d308ec5523b50aba35";
});

// 5. Tambahkan session dan cache untuk menyimpan data sementara di server
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Waktu timeout session
    options.Cookie.HttpOnly = true;                   // Cookie hanya bisa diakses via HTTP (bukan JS)
    options.Cookie.IsEssential = true;                 // Cookie wajib walau user menolak tracking
});

// 6. Daftarkan service custom (dependency injection)
builder.Services.AddScoped<IDbInitializer, DbInitializer>(); // Service untuk inisialisasi database
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();       // Unit of Work pattern untuk repository
builder.Services.AddScoped<IEmailSender, EmailSender>();     // Service pengiriman email

// 7. Tambahkan dukungan MVC dan Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 8. Konfigurasi middleware pipeline

// Jika bukan development, pakai halaman error umum dan HSTS (keamanan HTTPS)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();  // Redirect HTTP ke HTTPS
app.UseStaticFiles();       // Serve file statis seperti CSS, JS, gambar

// Konfigurasi Stripe API Key dari appsettings.json
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe.SecretKey").Get<string>();

app.UseRouting();           // Aktifkan routing URL

app.UseAuthentication();    // Aktifkan sistem autentikasi
app.UseAuthorization();     // Aktifkan sistem otorisasi (akses)
app.UseSession();           // Aktifkan session server

SeedDatabase();             // Jalankan inisialisasi database (seed data)

// Mapping Razor Pages dan route default MVC
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();  // Jalankan aplikasi

// Fungsi untuk menjalankan seeding database menggunakan service IDbInitializer
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}