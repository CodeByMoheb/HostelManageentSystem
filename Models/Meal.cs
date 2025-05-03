using System.ComponentModel.DataAnnotations;

namespace HostelManageentSystem.Models
{
    public class Meal
    {
        public int Id { get; set; }

        [Required]
        public int HostelId { get; set; }

        [Required]
        [StringLength(50)]
        public required string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner

        [Required]
        [StringLength(200)]
        public required string Menu { get; set; } = string.Empty;

        [Required]
        public DateTime MealDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Hostel? Hostel { get; set; }
    }
} 