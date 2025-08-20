using ElasoftCommunityManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElasoftCommunityManagementSystem.Dtos;
using ElasoftCommunityManagementSystem.DTOs;
namespace ElasoftCommunityManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventReservationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventReservationController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("all-reservations")]
        public async Task<ActionResult> GetAllReservations()
        {
            var reservations = await _context.EventReservations
                .Include(r => r.Event).ThenInclude(e => e.Club)
                .Include(r => r.Location)
                .Include(r => r.TimeSlot)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            var dtoList = reservations.Select(r => new ReservationDto
            {
                LocationName = r.Location?.Name ?? "Bilinmeyen Yer",
                StartHour = r.TimeSlot != null ? r.TimeSlot.StartHour.ToString(@"hh\:mm") : "",
                EndHour = r.TimeSlot != null ? r.TimeSlot.EndHour.ToString(@"hh\:mm") : "",
                ClubName = r.Event?.Club?.Name ?? "Bilinmeyen Kulüp",
                EventDate = r.ReservationDate.ToString("yyyy-MM-dd")

            }).ToList();

            return Ok(dtoList);
        }



        // 🔹 1. Öğrenciye yer + saat ver (disable olanlar da işaretli)
        [HttpGet("locations-with-timeslots")]
        public async Task<ActionResult> GetLocationsWithTimeSlots()
        {
            var result = await _context.Locations
                .Include(l => l.TimeSlots)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    TimeSlots = l.TimeSlots.Select(t => new
                    {
                        t.Id,
                        t.StartHour,
                        t.EndHour,
                        IsReserved = _context.EventReservations
                            .Any(r => r.LocationId == l.Id && r.TimeSlotId == t.Id)
                    }).ToList()
                }).ToListAsync();

            return Ok(result);
        }

        // 🔹 2. Admin: Yeni yer eklesin
        [HttpPost("add-location")]
        public async Task<ActionResult> AddLocation([FromBody] string name)
        {
            var location = new Location { Name = name };
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return Ok(location);
        }

        // 🔹 3. Admin: Seçilen yere saat aralığı eklesin
        [HttpPost("add-timeslot")]
        public async Task<ActionResult> AddTimeSlot([FromBody] TimeSlot slot)
        {
            var exists = await _context.Locations.AnyAsync(l => l.Id == slot.LocationId);
            if (!exists)
                return BadRequest("Location not found.");

            _context.TimeSlots.Add(slot);
            await _context.SaveChangesAsync();
            return Ok(slot);
        }

        [HttpGet("locations-with-details")]
        public async Task<ActionResult> GetLocationsWithDetails()
        {
            var locations = await _context.Locations
                .Include(l => l.DailySlots)
                .ToListAsync();

            var result = locations.Select(l => new
            {
                l.Id,
                l.Name,
                ValidDays = l.ValidDays,
                DisabledDates = l.DisabledDates.Select(d => d.ToString("yyyy-MM-dd")),
                DailySlots = l.DailySlots.Select(d => new
                {
                    d.Id,
                    d.DayOfWeek,
                    StartHour = d.StartHour.ToString(@"hh\:mm"),
                    EndHour = d.EndHour.ToString(@"hh\:mm")
                }).ToList()
            });

            return Ok(result);
        }
        [HttpPost("update-location-name")]
        public async Task<IActionResult> UpdateLocationName([FromBody] UpdateLocationNameDto dto)
        {
            var location = await _context.Locations.FindAsync(dto.Id);
            if (location == null) return NotFound();

            location.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok();
        }



        [HttpPost("update-valid-days")]
        public async Task<IActionResult> UpdateValidDays(int locationId, [FromBody] List<int> days)
        {
            var location = await _context.Locations.FindAsync(locationId);
            if (location == null) return NotFound("Tesis bulunamadı.");

            location.ValidDays = days;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update-disabled-dates")]
        public async Task<IActionResult> UpdateDisabledDates(int locationId, [FromBody] List<DateTime> dates)
        {
            var location = await _context.Locations.FindAsync(locationId);
            if (location == null) return NotFound("Tesis bulunamadı.");

            location.DisabledDates = dates;
            await _context.SaveChangesAsync();
            return Ok();
        }


        // 🔹 4. Öğrenci: Rezervasyon yapsın
        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve([FromBody] ReserveRequestDto dto)
        {
            if (!DateTime.TryParse(dto.ReservationDate, out DateTime parsedDate))
            {
                return BadRequest("Geçersiz tarih formatı.");
            }

            // Aynı etkinlik + tesis + saat + tarih için rezervasyon var mı?
            bool alreadyReserved = await _context.EventReservations.AnyAsync(r =>
                r.LocationId == dto.LocationId &&
                r.TimeSlotId == dto.TimeSlotId &&
                r.ReservationDate.Date == parsedDate.Date
            );

            if (alreadyReserved)
            {
                return BadRequest("Bu saat zaten rezerve edilmiş.");
            }

            var reservation = new EventReservation
            {
                EventId = dto.EventId,
                LocationId = dto.LocationId,
                TimeSlotId = dto.TimeSlotId,
                ReservationDate = parsedDate
            };

            _context.EventReservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;

            var locations = await _context.Locations
                .Include(l => l.DailySlots)
                .ToListAsync();

            var disabledSlotIds = await _context.DisabledSlots
                .Where(d => d.Date.Date == date.Date)
                .Select(d => d.SlotId)
                .ToListAsync();

            var reservedSlots = await _context.EventReservations
                .Where(r => r.Event.StartDate.Date == date.Date)
                .Select(r => r.TimeSlotId)
                .ToListAsync();

            var result = locations.Select(loc => new
            {
                loc.Id,
                loc.Name,
                Slots = loc.DailySlots
                    .Where(slot =>
                        slot.DayOfWeek == dayOfWeek &&
                        !disabledSlotIds.Contains(slot.Id) &&
                        !_context.EventReservations.Any(r =>
                            r.LocationId == loc.Id &&
                            r.TimeSlot.StartHour == slot.StartHour &&
                            r.TimeSlot.EndHour == slot.EndHour &&
                            r.Event.StartDate.Date == date.Date)
                    )
                    .Select(slot => new
                    {
                        slot.Id,
                        Start = slot.StartHour.ToString(@"hh\:mm"),
                        End = slot.EndHour.ToString(@"hh\:mm")
                    })
            });

            return Ok(result);
        }


        // 🔹 5. Etkinlik bazlı rezervasyonları listele (gerekirse)
        [HttpGet("by-event/{eventId}")]
        public async Task<ActionResult> GetReservationsForEvent(int eventId)
        {
            var reservations = await _context.EventReservations
                .Where(r => r.EventId == eventId)
                .Include(r => r.Location)
                .Include(r => r.TimeSlot)
                .ToListAsync();

            return Ok(reservations);
        }
        //günlük rezervasyon işlemleri

        [HttpGet("daily-slots/{locationId}")]
        public async Task<ActionResult> GetDailySlots(int locationId)
        {
            var slots = await _context.DailySlots
                .Where(ds => ds.LocationId == locationId)
                .OrderBy(ds => ds.DayOfWeek)
                .ThenBy(ds => ds.StartHour)
                .ToListAsync();

            var grouped = slots
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new
                {
                    DayOfWeek = g.Key,
                    Slots = g.Select(s => new
                    {
                        s.Id,
                        Start = s.StartHour.ToString(@"hh\:mm"),
                        End = s.EndHour.ToString(@"hh\:mm")
                    }).ToList()
                });

            return Ok(grouped);
        }
        [HttpPost("daily-slots")]
        public async Task<ActionResult> AddDailySlot([FromBody] CreateDailySlotDto dto)
        {
            var slot = new DailySlot
            {
                LocationId = dto.LocationId,
                DayOfWeek = dto.DayOfWeek,
                StartHour = TimeSpan.Parse(dto.StartHour),
                EndHour = TimeSpan.Parse(dto.EndHour)
            };

            _context.DailySlots.Add(slot);
            await _context.SaveChangesAsync();

            return Ok(slot);
        }
        [HttpGet("daily-slots/{locationId}/{dayOfWeek}")]
        public async Task<ActionResult> GetDailySlotsForDate(int locationId, int dayOfWeek, [FromQuery] string? date)
        {
            var baseSlots = await _context.DailySlots
                .Where(s => s.LocationId == locationId && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var result = new List<object>();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime selectedDate))
            {
                var disabledSlotIds = await _context.DisabledSlots
                    .Where(d => d.LocationId == locationId && d.Date.Date == selectedDate.Date)
                    .Select(d => d.SlotId)
                    .ToListAsync();

                var reservedSlotIds = await _context.EventReservations
                    .Where(r => r.LocationId == locationId && r.ReservationDate.Date == selectedDate.Date)
                    .Select(r => r.TimeSlotId)
                    .ToListAsync();

                result = baseSlots.Select(s => new
                {
                    s.Id,
                    StartHour = s.StartHour.ToString(@"hh\:mm"),
                    EndHour = s.EndHour.ToString(@"hh\:mm"),
                    Enabled = !disabledSlotIds.Contains(s.Id) && !reservedSlotIds.Contains(s.Id)
                }).ToList<object>();
            }
            else
            {
                result = baseSlots.Select(s => new
                {
                    s.Id,
                    StartHour = s.StartHour.ToString(@"hh\:mm"),
                    EndHour = s.EndHour.ToString(@"hh\:mm"),
                    Enabled = true
                }).ToList<object>();
            }

            return Ok(result);
        }



        [HttpPost("disable-daily-slots")]
        public async Task<ActionResult> DisableDailySlots([FromBody] DisableDailySlotsDto dto)
        {
            var parsedDate = DateTime.Parse(dto.Date);

            foreach (var slotId in dto.DisabledSlotIds)
            {
                var disabled = new DisabledSlot
                {
                    LocationId = dto.LocationId,
                    SlotId = slotId,
                    Date = parsedDate
                };
                _context.DisabledSlots.Add(disabled);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("update-daily-slots")]
        public async Task<IActionResult> UpdateDailySlots([FromBody] UpdateDailySlotDto dto)
        {
            var existing = await _context.DailySlots
                .Where(s => s.LocationId == dto.LocationId && s.DayOfWeek == dto.DayOfWeek)
                .ToListAsync();

            _context.DailySlots.RemoveRange(existing);

            foreach (var slot in dto.Slots)
            {
                var start = TimeSpan.Parse(slot.StartHour);
                var end = TimeSpan.Parse(slot.EndHour);

                _context.DailySlots.Add(new DailySlot
                {
                    LocationId = dto.LocationId,
                    DayOfWeek = dto.DayOfWeek,
                    StartHour = start,
                    EndHour = end
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }



    }
}
