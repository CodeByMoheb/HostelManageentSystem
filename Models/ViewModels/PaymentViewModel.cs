using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int HostelId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
} 