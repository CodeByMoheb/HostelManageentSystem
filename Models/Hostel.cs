using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Hostel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(200)]
        public required string Address { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(450)]
        public required string ManagerId { get; set; }

        [Required]
        [StringLength(100)]
        public required string ManagerName { get; set; }

        [Required]
        [Phone]
        public required string ManagerPhone { get; set; }

        [Required]
        [StringLength(50)]
        public required string City { get; set; }

        [Required]
        [StringLength(50)]
        public required string Area { get; set; }

        [Required]
        public int TotalRooms { get; set; }

        [Required]
        public int AvailableRooms { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SecurityDeposit { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerRoom { get; set; }

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        // Navigation property
        public virtual ApplicationUser? Manager { get; set; }
    }
} 