using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public required string UserId { get; set; }

        [Required]
        public int HostelId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [StringLength(50)]
        public required string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public virtual Room? Room { get; set; }
    }
} 