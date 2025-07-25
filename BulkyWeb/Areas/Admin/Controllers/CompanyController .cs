using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BulkyWeb.Areas.Admin.Controllers
{
    // Controller ini bertugas mengelola entitas Company
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Hanya admin yang bisa mengakses controller ini
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // Constructor dengan dependency injection UnitOfWork
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Menampilkan halaman utama daftar company
        public IActionResult Index()
        {
            // Ambil semua data company dari database
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            // Kirim data ke view, meskipun data nantinya dimuat via AJAX
            return View(objCompanyList);
        }

        // Method Upsert untuk menampilkan form Create atau Edit
        // Jika id null atau 0 maka buat company baru (Create)
        // Jika id ada maka ambil data company tersebut untuk diedit (Edit)
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create: Kirim objek company baru kosong ke view
                return View(new Company());
            }
            else
            {
                // Edit: Ambil data company dari database sesuai id
                Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);

                if (companyObj == null)
                {
                    // Jika company tidak ditemukan, bisa redirect ke Index atau tampilkan error
                    return NotFound();
                }

                // Kirim objek company yang sudah ada ke view untuk diedit
                return View(companyObj);
            }
        }

        // POST Upsert untuk memproses Create atau Update data company
        [HttpPost]
        [ValidateAntiForgeryToken] // Validasi agar form hanya bisa di-submit dari aplikasi ini
        public IActionResult Upsert(Company companyObj)
        {
            // Cek apakah data yang dikirim valid sesuai model (validasi Data Annotations)
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    // Jika Id = 0 maka create data baru
                    _unitOfWork.Company.Add(companyObj);
                }
                else
                {
                    // Jika Id bukan 0 maka update data lama
                    _unitOfWork.Company.Update(companyObj);
                }

                // Simpan perubahan ke database
                _unitOfWork.Save();

                // Tampilkan pesan sukses menggunakan TempData
                TempData["success"] = "Company saved successfully";

                // Redirect ke halaman daftar company
                return RedirectToAction("Index");
            }

            // Jika validasi gagal, kembalikan data ke view untuk perbaikan input
            return View(companyObj);
        }

        #region API CALLS

        // API untuk mengambil semua company dalam bentuk JSON
        // Biasanya digunakan oleh AJAX di frontend
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companyList });
        }

        // API untuk menghapus company berdasarkan Id (DELETE)
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            // Validasi parameter Id
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            // Ambil company dari database
            var company = _unitOfWork.Company.Get(u => u.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found" });
            }

            // Hapus data company
            _unitOfWork.Company.Remove(company);

            // Simpan perubahan
            _unitOfWork.Save();

            // Kirim balikan sukses ke frontend
            return Json(new { success = true, message = "Company deleted successfully" });
        }

        #endregion
    }
}