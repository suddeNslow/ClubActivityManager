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
    }
}
