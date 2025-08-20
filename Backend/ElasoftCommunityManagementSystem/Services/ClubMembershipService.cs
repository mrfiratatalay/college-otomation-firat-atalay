using ElasoftCommunityManagementSystem.Dtos.ClubMembershipoDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class ClubMembershipService : IClubMembershipService
    {
        private readonly AppDbContext _context;
        private readonly ClubAuthorizationService _authService;
        private readonly INotificationService _notificationService;

        public ClubMembershipService(
            AppDbContext context,
            ClubAuthorizationService authService,
            INotificationService notificationService
        )
        {
            _context = context;
            _authService = authService;
            _notificationService = notificationService;
        }

        public async Task ApplyToClubAsync(int userId, ClubMembershipDto dto)
        {
            await _authService.CanJoinClub(userId, dto.ClubId);

            var club = await _context.Club.FindAsync(dto.ClubId);
            if (club == null)
                throw new ResourceNotFoundException("Kulüp bulunamadı.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ResourceNotFoundException("Kullanıcı bulunamadı.");

            // Check if user is already a member of this club
            var existingMembership = await _context.ClubMembership.FirstOrDefaultAsync(m =>
                m.UserId == userId && m.ClubId == dto.ClubId
            );

            if (existingMembership != null)
            {
                if (existingMembership.Status.ToLower() == "approved")
                    throw new BusinessException("Bu kulübe zaten üyesiniz.");

                if (existingMembership.Status.ToLower() == "pending")
                    throw new BusinessException("Bu kulüp için zaten bekleyen bir başvurunuz var.");

                // Eğer daha önce reddedilmiş veya ayrılmış ise, yeni başvuru oluştur
                _context.ClubMembership.Remove(existingMembership);
                await _context.SaveChangesAsync();
            }

            var membership = new ClubMembershipModel
            {
                ClubId = dto.ClubId,
                UserId = userId,
                Role = "member", // Default role is member
                Status = "pending",
                JoinedAt = DateTime.UtcNow,
            };

            _context.ClubMembership.Add(membership);
            await _context.SaveChangesAsync();

            // --- Bildirim Gönderme Başlangıcı ---
            try
            {
                var recipients = new List<int>();

                // Danışmanı ekle (varsa)
                if (club.AdvisorId.HasValue)
                {
                    recipients.Add(club.AdvisorId.Value);
                }

                // Kulüp başkanını ekle (varsa)
                var president = await _context
                    .ClubMembership.Where(m =>
                        m.ClubId == dto.ClubId
                        && m.Role.ToLower() == "başkan"
                        && m.Status.ToLower() == "approved"
                    )
                    .Select(m => m.UserId)
                    .FirstOrDefaultAsync();

                if (president > 0 && !recipients.Contains(president)) // Danışman aynı zamanda başkan olabilir
                {
                    recipients.Add(president);
                }

                if (recipients.Any())
                {
                    await _notificationService.CreateNotificationForMultipleUsersAsync(
                        recipients.Distinct().ToList(),
                        "Yeni Üyelik Başvurusu",
                        $"{user.Name} {user.Surname}, '{club.Name}' topluluğuna üyelik başvurusunda bulundu.",
                        "membership_request",
                        membership.MembershipId
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Üyelik başvurusu bildirimi gönderirken hata oluştu: {ex.Message}"
                );
                // Loglama yapılabilir
            }
            // --- Bildirim Gönderme Sonu ---
        }

        public async Task<List<object>> GetMyClubApplicationsAsync(int userId)
        {
            return await _context
                .ClubMembership.Include(m => m.Club)
                .Where(m => m.UserId == userId && m.Status == "pending")
                .Select(m => new
                {
                    m.MembershipId,
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.Status,
                    m.JoinedAt,
                })
                .ToListAsync<object>();
        }

        public async Task ApproveMembershipAsync(int membershipId, int userId, string userRole)
        {
            var membership = await _context.ClubMembership.FindAsync(membershipId);
            if (membership == null || membership.Status != "pending")
                throw new ResourceNotFoundException("Başvuru bulunamadı.");

            await _authService.CanApproveMembers(userId, userRole, membership.ClubId);

            membership.Status = "approved";
            await _context.SaveChangesAsync();

            // Üye sayısını güncelle
            var club = await _context.Club.FindAsync(membership.ClubId);
            if (club != null)
            {
                club.MemberCount = await _context.ClubMembership.CountAsync(m =>
                    m.ClubId == club.ClubId && m.Status == "approved"
                );
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectMembershipAsync(int membershipId, int userId, string userRole)
        {
            var membership = await _context.ClubMembership.FindAsync(membershipId);
            if (membership == null || membership.Status != "pending")
                throw new ResourceNotFoundException("Başvuru bulunamadı.");

            await _authService.CanApproveMembers(userId, userRole, membership.ClubId);

            _context.ClubMembership.Remove(membership);
            await _context.SaveChangesAsync();
        }

        // Bu metot kullanıcının üye olduğu kulüpleri ve rollerini getirir
        public async Task<object> GetUserRolesInClubsAsync(int userId)
        {
            // Onaylı üyelikleri getir
            var memberships = await _context
                .ClubMembership.Include(m => m.Club)
                .Where(m => m.UserId == userId && m.Status == "approved")
                .Select(m => new
                {
                    m.MembershipId,
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.Role,
                    m.Status,
                    m.JoinedAt,
                })
                .ToListAsync();

            // Bekleyen üyelikleri getir
            var pendingMemberships = await _context
                .ClubMembership.Include(m => m.Club)
                .Where(m => m.UserId == userId && m.Status == "pending")
                .Select(m => new
                {
                    m.MembershipId,
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.Role,
                    m.Status,
                    m.JoinedAt,
                })
                .ToListAsync();

            return new
            {
                Memberships = memberships,
                PendingMemberships = pendingMemberships,
                IsLeaderOfAnyClub = memberships.Any(m => m.Role.ToLower() == "başkan"),
            };
        }

        // Bir kullanıcıyı topluluk başkanı yapma metodu
        public async Task SetClubLeaderAsync(int membershipId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var membership = await _context
                    .ClubMembership.Include(m => m.Club)
                    .FirstOrDefaultAsync(m => m.MembershipId == membershipId);
                if (membership == null || membership.Status != "approved")
                    throw new ResourceNotFoundException("Üyelik bulunamadı.");

                // Only admins and advisors can set club leaders
                var user = await _context.Users.FindAsync(userId);
                if (user == null || (user.Role != "admin" && user.Role != "advisor"))
                    throw new UnauthorizedBusinessException("Bu işlem için yetkiniz yok.");

                // Kullanıcı başka bir kulübün başkanı mı kontrol et

                var isAlreadyPresidentElsewhere = await _context.ClubMembership.AnyAsync(m =>
                    m.UserId == membership.UserId
                    && m.Role.ToLower() == "başkan"
                    && m.Status == "approved"
                    && m.ClubId != membership.ClubId
                );

                if (isAlreadyPresidentElsewhere)
                    throw new BusinessException(
                        "Bir kullanıcı aynı anda yalnızca bir kulübün başkanı olabilir."
                    );

                // Check if the user is already a president of this club
                if (membership.Role.ToLower() == "başkan")
                    throw new BusinessException("Bu kullanıcı zaten bu kulübün başkanıdır.");

                // Mevcut başkanı bul ve rolünü üye olarak değiştir
                var currentLeader = await _context.ClubMembership.FirstOrDefaultAsync(m =>
                    m.ClubId == membership.ClubId
                    && m.Role.ToLower() == "başkan"
                    && m.Status == "approved"
                );

                if (currentLeader != null)
                {
                    currentLeader.Role = "member";
                    _context.ClubMembership.Update(currentLeader);
                }

                // Yeni başkanı ata
                membership.Role = "başkan";
                _context.ClubMembership.Update(membership);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send notification to the new president
                await _notificationService.CreateNotificationAsync(
                    membership.UserId,
                    "Başkanlık Atama",
                    $"Tebrikler! {membership.Club?.Name ?? "Kulüp"} başkanı olarak atandınız.",
                    "membership"
                );

                // Send notification to the old president if exists
                if (currentLeader != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        currentLeader.UserId,
                        "Başkanlık Devri",
                        $"{membership.Club?.Name ?? "Kulüp"} başkanlığınız başka bir üyeye devredildi.",
                        "membership"
                    );
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<object>> GetPendingApplicationsForAdvisorAsync(int userId)
        {
            // Önce kullanıcının danışman olduğu kulüpleri bul
            var advisorClubs = await _context
                .Club.Where(c => c.AdvisorId == userId)
                .Select(c => c.ClubId)
                .ToListAsync();

            // Kullanıcının başkan olduğu kulüpleri bul
            var leaderClubs = await _context
                .ClubMembership.Where(m =>
                    m.UserId == userId && m.Role.ToLower() == "başkan" && m.Status == "approved"
                )
                .Select(m => m.ClubId)
                .ToListAsync();

            // İki listeyi birleştir
            var allClubs = advisorClubs.Union(leaderClubs).ToList();

            if (!allClubs.Any())
                return new List<object>();

            // Bu kulüplere gelen bekleyen başvuruları getir
            return await _context
                .ClubMembership.Include(m => m.Club)
                .Include(m => m.User)
                .Where(m => allClubs.Contains(m.ClubId) && m.Status == "pending")
                .Select(m => new
                {
                    m.MembershipId,
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.UserId,
                    UserName = m.User.Name,
                    m.Status,
                    m.JoinedAt,
                })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetPendingApplicationsForMyLedClubsAsync(int userId)
        {
            // Find the clubs where the user is the leader ('başkan')
            var ledClubIds = await _context
                .ClubMembership.Where(m =>
                    m.UserId == userId && m.Role.ToLower() == "başkan" && m.Status == "approved"
                )
                .Select(m => m.ClubId)
                .ToListAsync();

            if (!ledClubIds.Any())
            {
                return new List<object>(); // Return empty list if user is not leading any clubs
            }

            // Get pending applications for these clubs
            var pendingApplications = await _context
                .ClubMembership.Include(m => m.Club)
                .Include(m => m.User)
                .Where(m => ledClubIds.Contains(m.ClubId) && m.Status == "pending")
                .Select(m => new
                {
                    m.MembershipId,
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.UserId,
                    Name = m.User.Name,
                    Surname = m.User.Surname,
                    SchoolNumber = m.User.SchoolNumber,
                    DepartmentId = m.User.DepartmentId,
                    m.Status,
                    m.JoinedAt,
                })
                .ToListAsync<object>();

            return pendingApplications;
        }

        public async Task<List<ClubMemberListDto>> GetClubMembersAsync(int clubId)
        {
            // Kulübün üyelerini getir
            var members = await _context
                .ClubMembership.Include(m => m.User)
                .Where(m => m.ClubId == clubId && m.Status == "approved")
                .Select(m => new ClubMemberListDto
                {
                    UserId = m.UserId,
                    MembershipId = m.MembershipId,
                    Name = m.User.Name,
                    UserName = m.User.Name,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                })
                .ToListAsync();

            return members;
        }

        public async Task<List<object>> GetApprovedMembersAsync(int clubId)
        {
            var members = await _context
                .ClubMembership.Include(m => m.User)
                .Where(m => m.ClubId == clubId && m.Status.ToLower() == "approved")
                .Select(m => new
                {
                    m.MembershipId,
                    m.UserId,
                    Username = m.User.Name + " " + m.User.Surname,
                    FullName = m.User.Name + " " + m.User.Surname,
                    m.Role,
                    m.JoinedAt,
                })
                .ToListAsync<object>();

            return members;
        }

        public async Task DeleteApprovedMemberAsync(int membershipId, int userId)
        {
            var membership = await _context.ClubMembership.FirstOrDefaultAsync(m =>
                m.MembershipId == membershipId && m.Status == "approved"
            );

            if (membership == null)
                throw new ResourceNotFoundException("Üyelik bulunamadı veya onaylı değil.");

            // Kullanıcının yetkisini kontrol et (admin veya kulübün başkanı olmalı)
            var userRole = await _context
                .Users.Where(u => u.UserId == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            var isClubLeader = await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId
                && m.ClubId == membership.ClubId
                && m.Role.ToLower() == "başkan"
                && m.Status == "approved"
            );

            if (userRole != "admin" && !isClubLeader)
                throw new UnauthorizedBusinessException("Bu işlem için yetkiniz yok.");

            _context.ClubMembership.Remove(membership);
            await _context.SaveChangesAsync();

            // Kulüp üye sayısını güncelle
            var club = await _context.Club.FindAsync(membership.ClubId);
            if (club != null)
            {
                club.MemberCount = await _context.ClubMembership.CountAsync(m =>
                    m.ClubId == club.ClubId && m.Status == "approved"
                );
                await _context.SaveChangesAsync();
            }
        }

        public async Task<object> GetClubMemberUserDetailsAsync(int userId)
        {
            // Önce kullanıcının herhangi bir kulübe üye olup olmadığını kontrol et
            var isMemberOfAnyClub = await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId && m.Status == "approved"
            );

            if (!isMemberOfAnyClub)
                throw new ResourceNotFoundException("Kullanıcı herhangi bir kulübe üye değil.");

            // Kullanıcı bilgilerini getir
            var userDetails = await _context
                .Users.Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Surname,
                    u.Email,
                    u.PhoneNumber,
                    u.SchoolNumber,
                    u.Role,
                    u.CreatedAt,
                    u.DepartmentId,
                    Memberships = _context
                        .ClubMembership.Where(m => m.UserId == userId && m.Status == "approved")
                        .Select(m => new
                        {
                            m.ClubId,
                            ClubName = m.Club.Name,
                            m.Role,
                            m.JoinedAt,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync();

            if (userDetails == null)
                throw new ResourceNotFoundException("Kullanıcı bulunamadı.");

            return userDetails;
        }

        public async Task LeaveClubAsync(int membershipId, int userId)
        {
            var membership = await _context
                .ClubMembership.Include(m => m.Club)
                .FirstOrDefaultAsync(m => m.MembershipId == membershipId);

            if (membership == null)
                throw new ResourceNotFoundException("Üyelik bulunamadı.");

            if (membership.UserId != userId)
                throw new UnauthorizedBusinessException("Bu üyelik size ait değil.");

            if (membership.Status != "approved")
                throw new BusinessException(
                    "Sadece onaylanmış üyelikler için ayrılma işlemi yapılabilir."
                );

            if (membership.Role.ToLower() == "başkan")
                throw new BusinessException(
                    "Topluluk başkanı doğrudan ayrılamaz. Önce başkanlık görevini devretmelisiniz."
                );

            _context.ClubMembership.Remove(membership);

            // Üye sayısını güncelle
            if (membership.Club != null)
            {
                membership.Club.MemberCount = await _context.ClubMembership.CountAsync(m =>
                    m.ClubId == membership.ClubId
                    && m.Status == "approved"
                    && m.MembershipId != membershipId
                );
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates that a club has exactly one president. This method can be used for data integrity checks.
        /// </summary>
        /// <param name="clubId">The ID of the club to validate</param>
        /// <returns>True if the club has exactly one president, false otherwise</returns>
        public async Task<bool> ValidateClubPresidentUniquenessAsync(int clubId)
        {
            var presidentCount = await _context.ClubMembership.CountAsync(m =>
                m.ClubId == clubId && m.Role.ToLower() == "başkan" && m.Status == "approved"
            );

            return presidentCount == 1;
        }

        /// <summary>
        /// Gets the current president of a club
        /// </summary>
        /// <param name="clubId">The ID of the club</param>
        /// <returns>The president's membership information or null if no president exists</returns>
        public async Task<ClubMembershipModel?> GetClubPresidentAsync(int clubId)
        {
            return await _context
                .ClubMembership.Include(m => m.User)
                .FirstOrDefaultAsync(m =>
                    m.ClubId == clubId && m.Role.ToLower() == "başkan" && m.Status == "approved"
                );
        }

        public async Task ChangeClubPresidentAsync(int clubId, int newPresidentUserId, int adminUserId, string userRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Admin veya danışman yetkisi kontrolü
                if (userRole != "admin" && userRole != "advisor")
                    throw new UnauthorizedBusinessException("Bu işlem için yetkiniz yok.");

                // Kulübün var olduğunu kontrol et
                var club = await _context.Club.FindAsync(clubId);
                if (club == null)
                    throw new ResourceNotFoundException("Topluluk bulunamadı.");

                // Yeni başkan olacak kişinin topluluk üyesi olduğunu kontrol et
                var newPresidentMembership = await _context.ClubMembership
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.ClubId == clubId &&
                                            m.UserId == newPresidentUserId &&
                                            m.Status == "approved");

                if (newPresidentMembership == null)
                    throw new BusinessException("Seçilen kişi bu topluluğun onaylı üyesi değil.");

                // Kişinin öğrenci olduğunu kontrol et
                if (newPresidentMembership.Role?.ToLower() != "member")
                    throw new BusinessException("Sadece öğrenciler topluluk başkanı olabilir.");

                // Kişinin başka bir toplulukta başkan olup olmadığını kontrol et
                var isPresidentElsewhere = await _context.ClubMembership.AnyAsync(m =>
                    m.UserId == newPresidentUserId &&
                    m.ClubId != clubId &&
                    m.Role.ToLower() == "başkan" &&
                    m.Status == "approved");

                if (isPresidentElsewhere)
                    throw new BusinessException("Bu kişi zaten başka bir topluluğun başkanı.");

                // Mevcut başkanı bul (varsa)
                var currentPresident = await _context.ClubMembership
                    .FirstOrDefaultAsync(m => m.ClubId == clubId &&
                                            m.Role.ToLower() == "başkan" &&
                                            m.Status == "approved");

                // Seçilen kişi zaten başkan mı kontrol et
                if (currentPresident?.UserId == newPresidentUserId)
                    throw new BusinessException("Bu kişi zaten bu topluluğun başkanı.");

                // Eski başkanı normal üye yap (varsa)
                if (currentPresident != null)
                {
                    currentPresident.Role = "member";
                    _context.ClubMembership.Update(currentPresident);

                    // Eski başkana bildirim gönder
                    await _notificationService.CreateNotificationAsync(
                        currentPresident.UserId,
                        "Başkanlık Devri",
                        $"{club.Name} topluluk başkanlığınız başka bir üyeye devredildi.",
                        "membership"
                    );
                }

                // Yeni başkanı ata
                newPresidentMembership.Role = "başkan";
                _context.ClubMembership.Update(newPresidentMembership);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Yeni başkana bildirim gönder
                await _notificationService.CreateNotificationAsync(
                    newPresidentUserId,
                    "Başkanlık Atama",
                    $"Tebrikler! {club.Name} topluluk başkanı olarak atandınız.",
                    "membership"
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
