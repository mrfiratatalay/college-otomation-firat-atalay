using System.Linq;
using ElasoftCommunityManagementSystem.Dtos.ClubDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class ClubService : IClubService
    {
        private readonly AppDbContext _context;
        private readonly ClubAuthorizationService _authService;

        public ClubService(AppDbContext context, ClubAuthorizationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<List<object>> GetClubsAsync(string? status)
        {
            var query = _context.Club.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            return await query
                .Select(c => new
                {
                    c.ClubId,
                    c.Name,
                    c.Description,
                    c.CategoryId,
                    CategoryName = c.Category.Name,
                    c.CreatedAt,
                    c.Status,
                    c.AdvisorId,
                    AdvisorFullName = c.Advisor != null
                        ? c.Advisor.Name + " " + c.Advisor.Surname
                        : null,
                    MemberCount = _context.ClubMembership.Count(m =>
                        m.ClubId == c.ClubId && m.Status == "approved"
                    ),
                    EventCount = _context.Event.Count(e => e.ClubId == c.ClubId),
                    Image = c.Image != null ? Convert.ToBase64String(c.Image) : null,
                    Document = c.Document != null ? Convert.ToBase64String(c.Document) : null,
                })
                .ToListAsync<object>();
        }

        public async Task<object> GetClubByIdAsync(int clubId, int userId, string userRole)
        {
            var club = await _context
                .Club.Include(c => c.ClubMemberships)
                .ThenInclude(m => m.User)
                .Include(c => c.Events)
                .Include(c => c.Category)
                .Include(c => c.Advisor)
                .FirstOrDefaultAsync(c => c.ClubId == clubId);

            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            bool hasAccess = false;

            if (userRole == "admin")
            {
                hasAccess = true;
            }
            else if (userRole == "advisor")
            {
                hasAccess = club.AdvisorId == userId;
            }
            else
            {
                hasAccess = await _context.ClubMembership.AnyAsync(m =>
                    m.ClubId == clubId && m.UserId == userId && m.Status == "approved"
                );
            }

            if (!hasAccess)
            {
                if (club.Status == "active")
                {
                    return new
                    {
                        club.ClubId,
                        club.Name,
                        club.Description,
                        CategoryName = club.Category?.Name,
                        club.Status,
                        MemberCount = club.MemberCount,
                        EventCount = club.EventCount,
                    };
                }

                throw new UnauthorizedBusinessException(
                    "Bu kulübün detaylarını görüntüleme yetkiniz yok."
                );
            }

            return new
            {
                club.ClubId,
                club.Name,
                club.Description,
                club.CategoryId,
                CategoryName = club.Category?.Name,
                club.AdvisorId,
                AdvisorFullName = club.Advisor?.Name + " " + club.Advisor?.Surname,
                club.Status,
                club.CreatedAt,
                MemberCount = club.MemberCount,
                EventCount = club.EventCount,
                Image = club.Image != null ? Convert.ToBase64String(club.Image) : null,
                Document = club.Document != null ? Convert.ToBase64String(club.Document) : null,
                Members = club
                    .ClubMemberships.Where(m => m.Status == "approved")
                    .Select(m => new
                    {
                        m.MembershipId,
                        m.UserId,
                        Name = m.User?.Name,
                        m.Role,
                        m.JoinedAt,
                    })
                    .ToList(),
                Events = club
                    .Events.Select(e => new
                    {
                        e.EventId,
                        e.Name,
                        e.Description,
                        e.StartDate,
                        e.EndDate,
                        e.MaxParticipants,
                        ParticipantCount = e.ParticipantCount,
                    })
                    .ToList(),
            };
        }

        public async Task<object> CreateClubAsync(CreateClubDto dto, string userRole)
        {
            var advisor = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserId == dto.AdvisorId && u.Role == "advisor"
            );

            if (advisor == null)
                throw new ResourceNotFoundException("Danışman bulunamadı.");

            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                    throw new ResourceNotFoundException("Kategori bulunamadı.");
            }

            if (await _context.Club.AnyAsync(c => c.Name == dto.Name))
                throw new ValidationException("Bu isimde bir topluluk zaten mevcut.");

            // Başkan olacak kullanıcı başka bir kulüpte başkan mı kontrol et

            if (dto.CreatorUserId > 0)
            {
                var alreadyPresident = await _authService.IsUserClubLeaderAnywhere(
                    dto.CreatorUserId
                );

                if (alreadyPresident)
                    throw new BusinessException(
                        "Aynı anda yalnızca bir kulübün başkanı olabilirsiniz. Önce mevcut başkanlığınızı bırakmalısınız."
                    );
            }

            string status = (userRole == "admin" || userRole == "advisor") ? "active" : "pending";

            var club = new ClubModel
            {
                Name = dto.Name,
                Description = dto.Description,
                AdvisorId = advisor.UserId,
                CategoryId = dto.CategoryId,
                Status = status,
                Image = dto.Image,
                Document = dto.Document,
            };

            _context.Club.Add(club);
            await _context.SaveChangesAsync();

            // Başkan ekle
            if (dto.CreatorUserId > 0)
            {
                _context.ClubMembership.Add(
                    new ClubMembershipModel
                    {
                        ClubId = club.ClubId,
                        UserId = dto.CreatorUserId,
                        Role = "başkan",
                        Status = "approved",
                        JoinedAt = DateTime.UtcNow,
                    }
                );

                // Danışmanı da ekle
                _context.ClubMembership.Add(
                    new ClubMembershipModel
                    {
                        ClubId = club.ClubId,
                        UserId = advisor.UserId,
                        Role = "advisor",
                        Status = "approved",
                        JoinedAt = DateTime.UtcNow,
                    }
                );

                await _context.SaveChangesAsync();
                club.MemberCount = 2;
                await _context.SaveChangesAsync();
            }

            return new
            {
                club.ClubId,
                club.Name,
                club.Description,
                club.CategoryId,
                AdvisorFullName = advisor.Name + " " + advisor.Surname,
                club.Status,
                club.MemberCount,
                club.EventCount,
                Image = club.Image != null ? Convert.ToBase64String(club.Image) : null,
                Document = club.Document != null ? Convert.ToBase64String(club.Document) : null,
            };
        }

        public async Task<int> ClubMemberCountAsync(int clubId, int userId, string userRole)
        {
            await _authService.CanManageClub(userId, userRole, clubId);

            var club = await _context
                .Club.Include(c => c.ClubMemberships)
                .FirstOrDefaultAsync(c => c.ClubId == clubId);

            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            club.MemberCount = club.ClubMemberships.Count();
            await _context.SaveChangesAsync();

            return club.MemberCount;
        }

        public async Task<int> ClubEventCountAsync(int clubId, int userId, string userRole)
        {
            await _authService.CanManageClub(userId, userRole, clubId);

            var club = await _context
                .Club.Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.ClubId == clubId);

            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            club.EventCount = club.Events.Count();
            await _context.SaveChangesAsync();

            return club.EventCount;
        }

        public async Task ApproveClubAsync(int clubId)
        {
            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            if (club.Status.ToLower() == "active")
                throw new BusinessException("Topluluk zaten aktif durumda.");

            club.Status = "active";
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClubAsync(int clubId)
        {
            var club = await _context
                .Club.Include(c => c.ClubMemberships)
                .FirstOrDefaultAsync(c => c.ClubId == clubId);

            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            // Explicitly remove all memberships associated with the club first
            _context.ClubMembership.RemoveRange(club.ClubMemberships);

            // Now remove the club itself
            _context.Club.Remove(club);
            await _context.SaveChangesAsync(); // Save changes for both membership removal and club removal
        }

        public async Task<object> UpdateClubAsync(
            int clubId,
            CreateClubDto dto,
            int userId,
            string userRole
        )
        {
            await _authService.CanManageClub(userId, userRole, clubId);

            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            if (await _context.Club.AnyAsync(c => c.Name == dto.Name && c.ClubId != clubId))
                throw new ValidationException("Bu isimde başka bir topluluk zaten mevcut.");

            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                    throw new ResourceNotFoundException("Kategori bulunamadı.");
                club.CategoryId = dto.CategoryId;
            }

            // Danışman güncelleme kontrolü ve işlemi
            if (dto.AdvisorId > 0 && dto.AdvisorId != club.AdvisorId)
            {
                var newAdvisor = await _context.Users.FindAsync(dto.AdvisorId);
                if (newAdvisor == null)
                    throw new ResourceNotFoundException("Seçilen danışman bulunamadı.");

                // Yeni danışmanın gerçekten danışman rolünde olduğunu kontrol et
                if (newAdvisor.Role?.ToLower() != "advisor")
                    throw new ValidationException("Seçilen kişi danışman rolünde değil.");

                // Eski danışmanın topluluk üyeliğini kaldır (eğer varsa)
                if (club.AdvisorId.HasValue)
                {
                    var oldAdvisorMembership = await _context.ClubMembership
                        .FirstOrDefaultAsync(m => m.ClubId == clubId &&
                                                m.UserId == club.AdvisorId.Value &&
                                                m.Role.ToLower() == "advisor");

                    if (oldAdvisorMembership != null)
                    {
                        _context.ClubMembership.Remove(oldAdvisorMembership);
                    }
                }

                // Yeni danışmanı topluluğun danışmanı olarak ata
                club.AdvisorId = dto.AdvisorId;

                // Yeni danışmanı topluluk üyesi olarak da ekle
                var existingMembership = await _context.ClubMembership
                    .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == dto.AdvisorId);

                if (existingMembership == null)
                {
                    var newMembership = new ClubMembershipModel
                    {
                        ClubId = clubId,
                        UserId = dto.AdvisorId,
                        Role = "advisor",
                        Status = "approved",
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.ClubMembership.Add(newMembership);
                }
                else
                {
                    // Mevcut üyelik varsa rolünü advisor olarak güncelle
                    existingMembership.Role = "advisor";
                    existingMembership.Status = "approved";
                }
            }

            club.Name = dto.Name;
            club.Description = dto.Description;

            if (dto.Image != null)
                club.Image = dto.Image;

            if (dto.Document != null)
                club.Document = dto.Document;

            await _context.SaveChangesAsync();

            return new
            {
                club.ClubId,
                club.Name,
                club.Description,
                club.CategoryId,
                club.AdvisorId,
                club.Status,
                Image = club.Image != null ? Convert.ToBase64String(club.Image) : null,
                Document = club.Document != null ? Convert.ToBase64String(club.Document) : null,
            };
        }

        public async Task<List<object>> GetClubsByAdvisorAsync(int advisorId)
        {
            var advisorClubIds = await _context
                .ClubMembership.Where(m =>
                    m.UserId == advisorId && m.Role == "advisor" && m.Status == "approved"
                )
                .Select(m => m.ClubId)
                .ToListAsync();

            var clubs = await _context
                .Club.Include(c => c.ClubMemberships)
                .Include(c => c.Events)
                .Where(c => advisorClubIds.Contains(c.ClubId))
                .Select(c => new
                {
                    c.ClubId,
                    c.Name,
                    c.Description,
                    c.CategoryId,
                    c.CreatedAt,
                    c.Status,
                    MemberCount = c.ClubMemberships.Count(),
                    EventCount = c.Events.Count(),
                    Image = c.Image != null ? Convert.ToBase64String(c.Image) : null,
                    Document = c.Document != null ? Convert.ToBase64String(c.Document) : null,
                })
                .ToListAsync();

            return clubs.Cast<object>().ToList();
        }

        public async Task<List<object>> GetClubMemberStatsAsync()
        {
            var result = await _context
                .Club.Include(c => c.Category)
                .Select(c => new
                {
                    ClubId = c.ClubId,
                    ClubName = c.Name,
                    CreatedDate = c.CreatedAt.ToString("yyyy-MM-dd"), // düzeltildi
                    MemberCount = _context.ClubMembership.Count(m =>
                        m.ClubId == c.ClubId && m.Status == "approved"
                    ),
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category != null ? c.Category.Name : null,
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }
    }
}
