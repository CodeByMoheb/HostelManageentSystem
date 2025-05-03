using System.Collections.Generic;

namespace HostelManageentSystem.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHostels { get; set; }
        public int TotalBookings { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
    }
} 