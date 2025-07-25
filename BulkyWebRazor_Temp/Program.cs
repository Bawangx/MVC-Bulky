using Bulky.Utility;
using BulkyWebRazor_Temp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Menambahkan layanan Razor Pages ke container dependency injection
builder.Services.AddRazorPages();

// Menambahkan konfigurasi DbContext untuk koneksi ke database SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Menghubungkan konfigurasi Stripe di appsettings.json ke kelas StripeSettings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Membangun aplikasi web
var app = builder.Build();

// Konfigurasi pipeline HTTP request
if (!app.Environment.IsDevelopment())
{
    // Jika bukan environment development, gunakan halaman error khusus
    app.UseExceptionHandler("/Error");

    // Mengaktifkan HTTP Strict Transport Security (HSTS)
    app.UseHsts();
}

// Redirect HTTP ke HTTPS secara otomatis
app.UseHttpsRedirection();

// Mengaktifkan akses ke file statis seperti CSS, JS, gambar, dll.
app.UseStaticFiles();

app.UseRouting();   // Mengaktifkan routing endpoint

app.UseAuthorization();  // Mengaktifkan middleware authorization (autentikasi)

app.MapRazorPages(); // Memetakan Razor Pages ke routing aplikasi

app.Run();  // Menjalankan aplikasi