using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Hanya Admin yang bisa akses
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // ========== GET: /Admin/Product ==========
        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }

        // ========== GET: /Admin/Product/Upsert/{id?} ==========
        public IActionResult Upsert(int? id)
        {
            var productVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category
                    .GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
            };

            if (id == null || id == 0)
            {
                // Tambah produk baru
                return View(productVM);
            }

            // Edit produk
            productVM.Product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "ProductImages");
            if (productVM.Product == null)
                return NotFound();

            return View(productVM);
        }

        // ========== POST: /Admin/Product/Upsert ==========
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                // Jika validasi gagal, isi ulang CategoryList
                productVM.CategoryList = _unitOfWork.Category
                    .GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });
                return View(productVM);
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (productVM.Product.Id == 0)
            {
                // Tambah produk baru
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save(); // Simpan dulu agar ID terisi
            }
            else
            {
                // Update produk lama
                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
            }

            // Proses upload gambar
            if (files != null && files.Count > 0)
            {
                string productPath = Path.Combine("images", "products", "product" + productVM.Product.Id);
                string fullPath = Path.Combine(wwwRootPath, productPath);

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(fullPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    var productImage = new ProductImage
                    {
                        ImageUrl = Path.Combine("\\", productPath, fileName).Replace("\\", "/"),
                        ProductId = productVM.Product.Id
                    };

                    _unitOfWork.ProductImage.Add(productImage);
                }

                _unitOfWork.Save();
            }

            TempData["success"] = productVM.Product.Id == 0 ? "Product created successfully" : "Product updated successfully";
            return RedirectToAction("Index");
        }

        // ========== DELETE Image ==========
        public IActionResult DeleteImage(int imageId)
        {
            var image = _unitOfWork.ProductImage.Get(i => i.Id == imageId);

            if (image == null)
            {
                TempData["error"] = "Image not found";
                return RedirectToAction("Index");
            }

            // Hapus file fisik
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            int productId = image.ProductId;
            _unitOfWork.ProductImage.Remove(image);
            _unitOfWork.Save();
            TempData["success"] = "Image deleted successfully";

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        // =====================================
        // ==========        API         =======
        // =====================================

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return Json(new { success = false, message = "Invalid ID" });

            var product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            // Hapus folder dan gambar produk
            string productFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", "product" + product.Id);
            if (Directory.Exists(productFolder))
            {
                foreach (var file in Directory.GetFiles(productFolder))
                {
                    System.IO.File.Delete(file);
                }
                Directory.Delete(productFolder);
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted successfully" });
        }

        #endregion
    }
}