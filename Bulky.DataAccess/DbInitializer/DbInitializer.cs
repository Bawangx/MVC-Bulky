using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void Initialize() {
        try {
            if (_db.Database.GetPendingMigrations().Count() > 0) { 
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "admin@dotnetmastery.com",
                        Email = "admin@dotnetmastery.com",
                        Name = "admin@dotnetmastery.com",
                        PhoneNumber = "admin@dotnetmastery.com",
                        StreetAddress = "admin@dotnetmastery.com",
                        State = "admin@dotnetmastery.com",
                        PostalCode = "admin@dotnetmastery.com",
                        City = "admin@dotnetmastery.com"
                    }, "Admin123").GetAwaiter().GetResult();
                    ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }
                return;
            }
        }
    }
}
