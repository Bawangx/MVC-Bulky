using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bulky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                // Cek apakah ada migrasi database yang belum diterapkan
                if (_db.Database.GetPendingMigrations().Any())
                {
                    // Terapkan migrasi database secara otomatis
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Jika terjadi error saat migrasi, bisa logging atau penanganan lain
                // (Untuk sementara hanya lanjut ke pembuatan role dan user default)
            }

            // Cek apakah role "Customer" sudah ada di database
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                // Jika belum ada, buat role-role yang dibutuhkan
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                // Buat user default admin
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@dotnetmastery.com",
                    Email = "admin@dotnetmastery.com",
                    Name = "Admin",
                    PhoneNumber = "08123456789", // isi dengan nomor yang valid
                    StreetAddress = "Jl. Admin No.1",
                    State = "AdminState",
                    PostalCode = "12345",
                    City = "AdminCity"
                };

                // Simpan user admin dengan password "Admin123"
                _userManager.CreateAsync(adminUser, "Admin123").GetAwaiter().GetResult();

                // Ambil user yang baru dibuat dari database
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");

                // Beri role Admin ke user tersebut
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
        }
    }
}