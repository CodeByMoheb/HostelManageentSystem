using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        public required string StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public required string University { get; set; }

        [Required]
        [StringLength(50)]
        public required string Department { get; set; }

        [Required]
        public int HostelId { get; set; }

        [Required]
        public int RoomId { get; set; }

        public bool HasMealPlan { get; set; }

        public DateTime CheckInDate { get; set; } = DateTime.Now;
        public DateTime? CheckOutDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Hostel? Hostel { get; set; }
        public virtual Room? Room { get; set; }
    }
} 