using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyWeb.Areas.Admin.Controllers
{
    // Controller untuk mengelola user, khusus untuk Admin
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;       // Untuk operasi Identity User
        private readonly RoleManager<IdentityRole> _roleManager;       // Untuk operasi Role Identity
        private readonly IUnitOfWork _unitOfWork;                      // Unit of Work untuk akses database

        // Konstruktor, dependency injection UserManager, RoleManager, dan UnitOfWork
        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // Halaman utama daftar user
        public IActionResult Index()
        {
            return View();
        }

        // Menampilkan halaman manajemen role user (GET)
        public async Task<IActionResult> RoleManagment(string userId)
        {
            // Ambil user lengkap dengan data company-nya
            ApplicationUser appUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company");

            if (appUser == null)
            {
                return NotFound(); // Jika user tidak ditemukan, tampilkan halaman 404
            }

            // Membuat ViewModel untuk role management
            RoleManagmentVM roleVM = new RoleManagmentVM()
            {
                ApplicationUser = appUser,

                // Ambil semua role yang ada di sistem, untuk dropdown
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),

                // Ambil semua company, untuk dropdown (jika role Company)
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            // Ambil role user (asumsi user hanya memiliki satu role)
            var roles = await _userManager.GetRolesAsync(appUser);
            roleVM.ApplicationUser.Role = roles.FirstOrDefault();

            return View(roleVM);
        }

        // Meng-handle update role user (POST)
        [HttpPost]
        public async Task<IActionResult> RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            // Ambil user dari database berdasarkan Id
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            // Ambil role lama user
            var oldRoles = await _userManager.GetRolesAsync(applicationUser);
            string oldRole = oldRoles.FirstOrDefault();

            // Jika role diubah
            if (roleManagmentVM.ApplicationUser.Role != oldRole)
            {
                // Jika role baru adalah Company, update CompanyId user
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }

                // Jika role lama adalah Company, kosongkan CompanyId
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                // Update data user di database
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                // Hapus role lama dan assign role baru
                if (!string.IsNullOrEmpty(oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(applicationUser, oldRole);
                }
                await _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role);
            }
            else
            {
                // Jika role sama tapi company diubah (khusus role Company)
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            // Redirect ke halaman daftar user setelah berhasil update
            return RedirectToAction("Index");
        }

        #region API CALLS

        // API untuk mendapatkan semua user beserta company dan role-nya
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

            // Loop untuk isi property Role dan company dummy jika null
            foreach (var user in objUserList)
            {
                // Ambil role user secara async (berjalan sinkron di foreach)
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault();

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = "" // Dummy supaya tidak null di view/frontend
                    };
                }
            }

            // Return JSON agar bisa dipakai di JS atau frontend
            return Json(new { data = objUserList });
        }

        // API untuk mengunci/membuka kunci user berdasarkan Id
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "User tidak ditemukan" });
            }

            // Jika user terkunci (LockoutEnd masih di masa depan), maka buka kunci
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                // Jika user tidak terkunci, set lockout selama 1000 tahun (prakteknya terkunci permanen)
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            // Simpan perubahan ke database
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Operasi berhasil" });
        }

        #endregion
    }
}