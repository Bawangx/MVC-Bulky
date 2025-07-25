using DI_Service_Lifetime.Services; // Namespace berisi definisi service

var builder = WebApplication.CreateBuilder(args);

// Menambahkan layanan (services) ke dalam container Dependency Injection
builder.Services.AddControllersWithViews();

// Registrasi service dengan berbagai lifetime:

// Singleton: Instance dibuat sekali, digunakan sepanjang aplikasi berjalan
builder.Services.AddSingleton<ISingletonGuidService, SingletonGuidService>();

// Scoped: Instance dibuat sekali per request HTTP, dibuang setelah request selesai
builder.Services.AddScoped<IScopedGuidService, ScopedGuidService>();

// Transient: Instance baru dibuat setiap kali service diminta (berulang kali)
builder.Services.AddTransient<ITransientGuidService, TransientGuidService>();

var app = builder.Build();

// Konfigurasi middleware pipeline

if (!app.Environment.IsDevelopment())
{
    // Jika bukan environment development, gunakan halaman error khusus dan HSTS (security)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();  // Redirect HTTP ke HTTPS
app.UseStaticFiles();       // Mengizinkan akses ke file statis (css, js, gambar, dll)

app.UseRouting();           // Routing untuk menghubungkan request ke controller/action

app.UseAuthorization();    // Middleware otorisasi (akses kontrol)

app.MapControllerRoute(     // Definisi routing default untuk controller dan action
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Jalankan aplikasi
app.Run();