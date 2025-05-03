using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int HostelId { get; set; }
        public Hostel? Hostel { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty; // Room Rent, Security Deposit, Meal Plan, etc.

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        [Required]
        public DateTime PaymentDate { get; set; }

        public string? TransactionId { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
} 