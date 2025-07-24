using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }
        public string? OrderStatus { get; set; } // e.g., "Pending", "Shipped", "Delivered"
        public string? PaymentStatus { get; set; } // e.g., "Pending", "Completed", "Failed"
        public string? TrackingNumber { get; set; } // Optional, for tracking shipments
        public string? Carrier { get; set; } // Optional, for shipping carrier information
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueData { get; set; } // For payment processing integration
        public string? SessionId { get; set; } // For payment processing integration
        public string? PaymentIntentId { get; set; } // For payment processing integration
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
        public string Name { get; set; }

    }
}
