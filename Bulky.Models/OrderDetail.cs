using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }  // Primary key

        [Required]
        public int OrderHeaderId { get; set; }  // Foreign key ke OrderHeader (header order)

        [ForeignKey("OrderHeaderId")]
        [ValidateNever]  // Tidak divalidasi saat form submit, properti navigasi ke OrderHeader
        public OrderHeader OrderHeader { get; set; }  // Navigasi ke entitas OrderHeader

        [Required]
        public int ProductId { get; set; }  // Foreign key ke Product

        [ForeignKey("ProductId")]
        [ValidateNever]  // Tidak divalidasi saat form submit, properti navigasi ke Product
        public Product Product { get; set; }  // Navigasi ke entitas Product

        public int Count { get; set; }  // Jumlah produk yang dipesan

        public double Price { get; set; }  // Harga produk pada saat order (bisa berbeda dari harga produk saat ini)
    }
}