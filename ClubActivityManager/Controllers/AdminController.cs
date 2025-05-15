using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClubActivityManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Dashboard() => View();

        // --- Members ---
        public async Task<IActionResult> Members()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Members));
        }

        // --- Resources ---
        public async Task<IActionResult> Resources()
        {
            var resources = await _context.Resources.ToListAsync();
            return View(resources);
        }

        [HttpGet]
        public IActionResult CreateResource() => View();

        [HttpPost]
        public async Task<IActionResult> CreateResource(Resource resource)
        {
            if (ModelState.IsValid)
            {
                _context.Resources.Add(resource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Resources));
            }
            return View(resource);
        }

        [HttpGet]
        public async Task<IActionResult> EditResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            return View(resource);
        }

        [HttpPost]
        public async Task<IActionResult> EditResource(Resource resource)
        {
            if (ModelState.IsValid)
            {
                _context.Resources.Update(resource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Resources));
            }
            return View(resource);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Resources));
        }


        // --- Events ---
        public async Task<IActionResult> Events()
        {
            var events = await _context.Events.Include(e => e.Creator).ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.EventRegistrations)
                .Include(e => e.ResourceReservations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev != null)
            {
                _context.EventRegistrations.RemoveRange(ev.EventRegistrations);
                _context.ResourceReservations.RemoveRange(ev.ResourceReservations);
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Events));
        }

        // --- Payments ---
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments.Include(p => p.Member).ToListAsync();
            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> AddPayment()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Members = new SelectList(users, "Id", "UserName");
            return View(new Payment { PaymentDate = DateTime.UtcNow });
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Payments));
            }

            
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Members = new SelectList(users, "Id", "UserName", payment.MemberId);

            return View(payment);
        }


    }
}
