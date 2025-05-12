using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClubActivityManager.Models;
using ArtClubApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ClubActivityManager.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.ResourceReservations)
                    .ThenInclude(rr => rr.Resource)
                .ToListAsync();

            return View(events);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            var now = DateTime.Now;

            // Exclude resources with active reservations in the future
            var reservedResourceIds = _context.ResourceReservations
                .Where(r => r.EndTime > now)
                .Select(r => r.ResourceId)
                .Distinct()
                .ToList();

            var availableResources = _context.Resources
                .Where(r => !reservedResourceIds.Contains(r.ResourceId))
                .ToList();

            var viewModel = new EventWithReservationViewModel
            {
                AvailableResources = availableResources
            };

            return View(viewModel);
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventWithReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var newEvent = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    DateTime = model.DateTime,
                    Location = model.Location,
                    CreatedBy = user.Id,
                    Creator = user
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                // Add resource reservations
                foreach (var resourceId in model.ResourceIds)
                {
                    var reservation = new ResourceReservation
                    {
                        EventId = newEvent.EventId,
                        ResourceId = resourceId,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime
                    };

                    _context.ResourceReservations.Add(reservation);
                }

                await _context.SaveChangesAsync();

                // Register the creator as the first attendee
                var registration = new EventRegistration
                {
                    EventId = newEvent.EventId,
                    UserId = user.Id,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.EventRegistrations.Add(registration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.AvailableResources = _context.Resources.ToList();
            return View(model);
        }
        // GET: Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.EventRegistrations)
                .ThenInclude(r => r.User) // Include registered users
            .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            ViewBag.AlreadyRegistered = evt.EventRegistrations.Any(r => r.UserId == userId);

            return View(evt);
        }

        // POST: Event/Register/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == id);

            if (!eventExists)
            {
                return NotFound();
            }

            var alreadyRegistered = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.UserId == user.Id);

            if (!alreadyRegistered)
            {
                var registration = new EventRegistration
                {
                    EventId = id,
                    UserId = user.Id,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.EventRegistrations.Add(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        // POST: Event/Unregister
        [HttpPost]
        public async Task<IActionResult> Unregister(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == user.Id);

            if (registration != null)
            {
                _context.EventRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = id });
        }
        // POST: Event/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _context.Events
                .Include(e => e.EventRegistrations)
                .Include(e => e.ResourceReservations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            var user = await _userManager.GetUserAsync(User);
            if (evt == null || evt.CreatedBy != user.Id)
            {
                return Unauthorized();
            }

            _context.EventRegistrations.RemoveRange(evt.EventRegistrations);
            _context.ResourceReservations.RemoveRange(evt.ResourceReservations);
            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
