using ElasoftCommunityManagementSystem.Dtos.ClubExpenceDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class ClubExpenseService : IClubExpenseService
    {
        private readonly AppDbContext _context;
        private readonly ClubExpenseAuthorizationService _authService;

        public ClubExpenseService(AppDbContext context, ClubExpenseAuthorizationService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<List<ClubExpenseDto>> GetAllExpensesAsync()
        {
            return await _context.ClubExpenses
                .Include(e => e.Club)
                .Select(e => new ClubExpenseDto
                {
                    Id = e.Id,
                    ClubId = e.ClubId,
                    ClubName = e.Club.Name,
                    CashSupport = e.CashSupport,
                    InKindSupport = e.InKindSupport,
                    Description = e.Description,
                    Date = e.Date,
                    DokumanUrl = e.DokumanUrl // Belge URL'sini ekliyoruz
                }).ToListAsync();
        }

        public async Task<List<ClubExpenseDto>> GetExpensesByClubIdAsync(int clubId)
        {
            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            return await _context.ClubExpenses
                .Include(e => e.Club)
                .Where(e => e.ClubId == clubId)
                .Select(e => new ClubExpenseDto
                {
                    Id = e.Id,
                    ClubId = e.ClubId,
                    ClubName = e.Club.Name,
                    CashSupport = e.CashSupport,
                    InKindSupport = e.InKindSupport,
                    Description = e.Description,
                    Date = e.Date,
                    DokumanUrl = e.DokumanUrl // Belge URL'sini ekliyoruz
                }).ToListAsync();
        }

        public async Task<bool> AddExpenseAsync(CreateExpenseDto dto, int userId, string userRole)
        {
            var club = await _context.Club.FindAsync(dto.ClubId);
            Console.WriteLine("📦 Serviste DTO ile gelen DokumanUrl: " + dto.DokumanUrl);

            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");
            //test amaçlı tüm topluluklara maliyet ekleniyor.
            //if (club.Status != "Aktif")   
             //   throw new BusinessException("Sadece aktif topluluklar için gider kaydı oluşturulabilir.");

            await _authService.IsUserAuthorizedForExpense(userId, userRole, dto.ClubId);

            if (dto.CashSupport < 0 || dto.InKindSupport < 0)
                throw new ValidationException("Gider tutarları negatif olamaz.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ValidationException("Gider açıklaması boş olamaz.");

            if (dto.Description.Length > 500)
                throw new ValidationException("Gider açıklaması 500 karakterden uzun olamaz.");

            if (dto.Date > DateTime.Now)
                throw new ValidationException("Gelecek tarihli gider kaydı oluşturulamaz.");

            var expense = new ClubExpenceModel
            {
                ClubId = dto.ClubId,
                CashSupport = dto.CashSupport,
                InKindSupport = dto.InKindSupport,
                Description = dto.Description,
                Date = dto.Date,
                DokumanUrl = dto.DokumanUrl // Doküman URL'sini kaydediyoruz
            };

            _context.ClubExpenses.Add(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ClubExpenseDto?> UpdateExpenseAsync(int id, CreateExpenseDto dto, int userId, string userRole)
        {
            var expense = await _context.ClubExpenses
                .Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                throw new ResourceNotFoundException("Gider kaydı bulunamadı.");

            // Check if the club referenced in the DTO exists, especially if ClubId can be changed
            var club = await _context.Club.FindAsync(dto.ClubId);
            if (club == null)
                throw new ResourceNotFoundException("Güncellemek istenen topluluk bulunamadı.");

            // Authorize the user for the *original* club the expense belonged to
            await _authService.IsUserAuthorizedForExpense(userId, userRole, expense.ClubId);
            // If the ClubId is being changed, also authorize for the *new* club
            if (expense.ClubId != dto.ClubId)
            {
                await _authService.IsUserAuthorizedForExpense(userId, userRole, dto.ClubId);
            }

            // Apply validations
            if (dto.CashSupport < 0 || dto.InKindSupport < 0)
                throw new ValidationException("Gider tutarları negatif olamaz.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ValidationException("Gider açıklaması boş olamaz.");

            if (dto.Description.Length > 500)
                throw new ValidationException("Gider açıklaması 500 karakterden uzun olamaz.");

            if (dto.Date > DateTime.Now)
                throw new ValidationException("Gelecek tarihli gider kaydı oluşturulamaz.");

            // Update properties
            expense.ClubId = dto.ClubId;
            expense.CashSupport = dto.CashSupport;
            expense.InKindSupport = dto.InKindSupport;
            expense.Description = dto.Description;
            expense.Date = dto.Date;
            // Update the Club navigation property if ClubId changed
            expense.Club = club;

            await _context.SaveChangesAsync();

            // Map updated entity back to DTO
            return new ClubExpenseDto
            {
                Id = expense.Id,
                ClubId = expense.ClubId,
                ClubName = expense.Club.Name, // Use the potentially updated club name
                CashSupport = expense.CashSupport,
                InKindSupport = expense.InKindSupport,
                Description = expense.Description,
                Date = expense.Date,
                DokumanUrl = expense.DokumanUrl
            };
        }

        public async Task<bool> DeleteExpenseAsync(int id, int userId, string userRole)
        {
            var expense = await _context.ClubExpenses
                .Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                throw new ResourceNotFoundException("Gider kaydı bulunamadı.");

            await _authService.IsUserAuthorizedForExpense(userId, userRole, expense.ClubId);

            // Prevent deletion of old expenses (e.g., older than 30 days)
            if (expense.Date < DateTime.Now.AddDays(-30))
                throw new BusinessException("30 günden eski gider kayıtları silinemez.");

            _context.ClubExpenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
