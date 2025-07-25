using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BulkyWeb.Areas.Admin.Controllers
{
    // Controller untuk mengelola Category
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Hanya role Admin yang bisa akses
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // Inject UnitOfWork untuk akses repository dan database
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Menampilkan daftar semua category
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        // Menampilkan form Create category baru
        public IActionResult Create()
        {
            return View();
        }

        // Menampilkan form Edit category berdasarkan id
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound(); // Jika id kosong atau 0, tampilkan 404
            }

            // Ambil category berdasarkan id
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound(); // Jika tidak ditemukan, tampilkan 404
            }

            return View(categoryFromDb); // Kirim data category ke view edit
        }

        // POST method untuk proses Edit category
        [HttpPost]
        [ValidateAntiForgeryToken] // Mencegah CSRF attack
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid) // Cek validasi data
            {
                _unitOfWork.Category.Update(obj); // Update category di database
                _unitOfWork.Save(); // Simpan perubahan
                TempData["success"] = "Category updated successfully"; // Simpan pesan sukses
                return RedirectToAction("Index"); // Redirect ke daftar category
            }
            return View(obj); // Jika gagal validasi, kembalikan ke form edit dengan data lama
        }

        // POST method untuk proses Create category baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            // Contoh validasi custom: Name tidak boleh sama dengan DisplayOrder (string)
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order cannot exactly match the Name.");
            }

            if (ModelState.IsValid) // Jika validasi berhasil
            {
                _unitOfWork.Category.Add(obj); // Tambah category baru
                _unitOfWork.Save(); // Simpan perubahan
                TempData["success"] = "Category created successfully"; // Pesan sukses
                return RedirectToAction("Index"); // Redirect ke daftar category
            }

            return View(obj); // Jika gagal validasi, kembalikan ke form create dengan data lama
        }

        // Menampilkan konfirmasi delete berdasarkan id
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound(); // Jika id tidak valid, 404
            }

            var categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound(); // Jika category tidak ditemukan, 404
            }

            return View(categoryFromDb); // Tampilkan view konfirmasi hapus
        }

        // POST method untuk proses delete category
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.Category.Get(u => u.Id == id);

            if (obj == null)
            {
                return NotFound(); // Jika data tidak ditemukan, 404
            }

            _unitOfWork.Category.Remove(obj); // Hapus category
            _unitOfWork.Save(); // Simpan perubahan

            TempData["success"] = "Category deleted successfully"; // Pesan sukses
            return RedirectToAction("Index"); // Redirect ke daftar category
        }
    }
}