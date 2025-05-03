using System.Collections.Generic;
using HostelManageentSystem.Models;

namespace HostelManageentSystem.Models.ViewModels
{
    public class ManagerDashboardViewModel
    {
        public bool HasHostel { get; set; }
        public Hostel? CurrentHostel { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveMealPlans { get; set; }
        public int PendingBookings { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Student> RecentStudents { get; set; } = new List<Student>();
        public List<Room> AvailableRoomsList { get; set; } = new List<Room>();
        public List<Meal> UpcomingMeals { get; set; } = new List<Meal>();
    }
} 