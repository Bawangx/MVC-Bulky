using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class OrderHeader
    {
        public int Id { get; set; } // Primary key

        public string ApplicationUserId { get; set; } // FK ke user yang membuat order

        [ForeignKey("ApplicationUserId")]
        [ValidateNever] // Jangan divalidasi saat submit form, karena ini properti navigasi
        public ApplicationUser ApplicationUser { get; set; } // Navigasi ke user

        public DateTime OrderDate { get; set; } // Tanggal order dibuat
        public DateTime ShippingDate { get; set; } // Tanggal pengiriman barang

        public double OrderTotal { get; set; } // Total harga order

        public string? OrderStatus { get; set; } // Status order: Pending, Shipped, Delivered, dll
        public string? PaymentStatus { get; set; } // Status pembayaran: Pending, Completed, Failed, dll

        public string? TrackingNumber { get; set; } // Nomor pelacakan pengiriman (optional)
        public string? Carrier { get; set; } // Nama jasa pengiriman (optional)

        public DateTime PaymentDate { get; set; } // Tanggal pembayaran dilakukan
        public DateTime PaymentDueData { get; set; } // Tanggal batas pembayaran

        // Untuk integrasi payment gateway (seperti Stripe)
        public string? SessionId { get; set; } // ID sesi pembayaran
        public string? PaymentIntentId { get; set; } // ID intent pembayaran

        // Data pengiriman dan kontak wajib diisi
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string StreetAddress { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string Name { get; set; } // Nama penerima / pemesan
    }
}