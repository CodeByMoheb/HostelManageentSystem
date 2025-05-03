using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public required string RoomNumber { get; set; }

        [Required]
        [StringLength(50)]
        public required string RoomType { get; set; } // Master, Single, Double, Triple, Quad

        [Required]
        public int HostelId { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        public decimal RentPerSeat { get; set; }

        [Required]
        public decimal SecurityDeposit { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Hostel? Hostel { get; set; }
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
} 