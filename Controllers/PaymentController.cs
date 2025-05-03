using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HostelManageentSystem.Models;
using HostelManageentSystem.Models.ViewModels;
using HostelManageentSystem.Data;

namespace HostelManageentSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var payments = await _context.Payments
                .Include(p => p.Hostel)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payment = new Payment
            {
                HostelId = model.HostelId,
                UserId = _userManager.GetUserId(User),
                Amount = model.Amount,
                PaymentType = model.PaymentType,
                Status = "Pending",
                PaymentDate = DateTime.Now,
                Description = model.Description
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Here you would typically integrate with a payment gateway
            // For now, we'll just mark it as completed
            payment.Status = "Completed";
            payment.TransactionId = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment processed successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagePayments()
        {
            var managerId = _userManager.GetUserId(User);
            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == managerId);

            if (hostel == null)
            {
                return RedirectToAction("RegisterHostel", "Manager");
            }

            var payments = await _context.Payments
                .Include(p => p.User)
                .Where(p => p.HostelId == hostel.Id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string status)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var managerId = _userManager.GetUserId(User);
            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.ManagerId == managerId);

            if (hostel == null || payment.HostelId != hostel.Id)
            {
                return Unauthorized();
            }

            payment.Status = status;
            payment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment status updated successfully!";
            return RedirectToAction(nameof(ManagePayments));
        }
    }
} 