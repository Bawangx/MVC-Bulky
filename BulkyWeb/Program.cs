// ─────────────────────────────────────────────────────────────────────────────
// 📦 Using directive (import library)
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

#region 1️⃣ KONFIGURASI SERVICE

// ✅ 1. Konfigurasi koneksi database (Entity Framework)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Konfigurasi Identity (untuk login/register user)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Tidak perlu konfirmasi email
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Gunakan database EF Core
.AddDefaultTokenProviders();                      // Untuk reset password, dll

// ✅ 3. Konfigurasi path login/logout/access denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ✅ 4. Konfigurasi Facebook Login
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "1141841511058275";               // Ganti dengan App ID Facebook kamu
    options.AppSecret = "c25b1216a10ac9d308ec5523b50aba35"; // Ganti dengan App Secret
});

// ✅ 5. Konfigurasi Session (menyimpan data di server sementara)
builder.Services.AddDistributedMemoryCache(); // Cache memory lokal
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Expired setelah 100 menit
    options.Cookie.HttpOnly = true;                  // Cookie hanya bisa dibaca server
    options.Cookie.IsEssential = true;               // Cookie tetap aktif meskipun user menolak tracking
});

// ✅ 6. Register Service untuk Dependency Injection
builder.Services.AddScoped<IDbInitializer, DbInitializer>(); // Inisialisasi data awal DB
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();       // Pattern repository gabungan
builder.Services.AddScoped<IEmailSender, EmailSender>();     // Pengirim email (reset password, dll)

// ✅ 7. MVC dan Razor Pages
builder.Services.AddControllersWithViews(); // Untuk routing controller
builder.Services.AddRazorPages();           // Untuk halaman Razor Pages

#endregion

// Bangun app ASP.NET Core
var app = builder.Build();

#region 2️⃣ KONFIGURASI MIDDLEWARE

// ✅ 8. Error handling & security HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redirect ke halaman error friendly
    app.UseHsts();                          // Tambahkan HTTP Strict Transport Security
}

// ✅ 9. Middleware dasar
app.UseHttpsRedirection();  // Paksa HTTPS
app.UseStaticFiles();       // Load file CSS, JS, gambar, dll

// ✅ 10. Stripe Configuration dari appsettings.json
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

// ✅ 11. Middleware Routing
app.UseRouting();           // Aktifkan sistem routing

// ✅ 12. Middleware Autentikasi dan Otorisasi
app.UseAuthentication();    // Proses login & validasi identitas
app.UseAuthorization();     // Cek apakah user punya akses

// ✅ 13. Aktifkan Session Server
app.UseSession();           // Simpan data user sementara (cart, userId, dll)

// ✅ 14. Seed Database (masukkan data awal ke DB)
SeedDatabase();

#endregion

#region 3️⃣ KONFIGURASI ENDPOINT

// ✅ 15. Routing Razor Page dan MVC Controller
app.MapRazorPages(); // Untuk halaman-halaman /Identity
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"); // Default route

#endregion

// ✅ 16. Jalankan Aplikasi
app.Run();

#region 🔁 FUNGSI SEED DATABASE

// Fungsi untuk inisialisasi database
void SeedDatabase()
{
    using var scope = app.Services.CreateScope(); // Buat scope untuk akses service
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>(); // Ambil service
    dbInitializer.Initialize(); // Jalankan proses seeding
}

#endregion