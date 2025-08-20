using ElasoftCommunityManagementSystem.Dtos.StatisticsDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ElasoftCommunityManagementSystem.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Özet istatistikleri ve büyüme oranlarını döndürür
        /// </summary>
        public async Task<SummaryStatisticsDto> GetSummaryStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            // Toplam üye, topluluk, etkinlik ve gider sayıları
            var totalMembers = await _context.Users.CountAsync();
            var totalClubs = await _context.Club.CountAsync();
            var totalEvents = await _context.Event.CountAsync();
            var totalExpenses = await _context.ClubExpenses.SumAsync(e => e.CashSupport + e.InKindSupport);

            // Önceki aya göre büyüme oranları
            var prevMonthMembers = await _context.Users
                .Where(u => u.CreatedAt < oneMonthAgo)
                .CountAsync();
            var memberGrowth = prevMonthMembers > 0 
                ? Math.Round(((double)(totalMembers - prevMonthMembers) / prevMonthMembers) * 100, 2) 
                : 0;

            var prevMonthClubs = await _context.Club
                .Where(c => c.CreatedAt < oneMonthAgo)
                .CountAsync();
            var clubGrowth = prevMonthClubs > 0 
                ? Math.Round(((double)(totalClubs - prevMonthClubs) / prevMonthClubs) * 100, 2) 
                : 0;

            var prevMonthEvents = await _context.Event
                .Where(e => e.CreatedAt < oneMonthAgo)
                .CountAsync();
            var eventGrowth = prevMonthEvents > 0 
                ? Math.Round(((double)(totalEvents - prevMonthEvents) / prevMonthEvents) * 100, 2) 
                : 0;

            var prevMonthExpenses = await _context.ClubExpenses
                .Where(e => e.Date < oneMonthAgo)
                .SumAsync(e => e.CashSupport + e.InKindSupport);
            var expenseGrowth = prevMonthExpenses > 0 
                ? Math.Round(((double)Convert.ToDouble(totalExpenses - prevMonthExpenses) / Convert.ToDouble(prevMonthExpenses)) * 100, 2) 
                : 0;

            return new SummaryStatisticsDto
            {
                TotalMembers = totalMembers,
                TotalClubs = totalClubs,
                TotalEvents = totalEvents,
                TotalExpenses = totalExpenses,
                Growth = new GrowthRatesDto
                {
                    Members = memberGrowth,
                    Clubs = clubGrowth,
                    Events = eventGrowth,
                    Expenses = expenseGrowth
                }
            };
        }

        /// <summary>
        /// Topluluk bazında istatistikleri döndürür
        /// </summary>
        public async Task<List<ClubStatisticsDto>> GetClubStatisticsAsync()
        {
            var clubStats = await _context.Club
                .Select(c => new ClubStatisticsDto
                {
                    Id = c.ClubId,
                    Name = c.Name,
                    MemberCount = c.ClubMemberships.Count(cm => cm.Status.ToLower() == "approved" || cm.Status.ToLower() == "onaylı"),
                    EventCount = c.Events.Count(),
                    ParticipationRate = c.Events.Count() > 0
                        ? Math.Round((double)c.Events.Sum(e => e.EventParticipants.Count()) / c.Events.Count(), 0)
                        : 0
                })
                .OrderByDescending(c => c.MemberCount)
                .ToListAsync();

            return clubStats;
        }

        /// <summary>
        /// Belirli bir yıl için aylık etkinlik istatistiklerini döndürür
        /// </summary>
        public async Task<EventStatisticsResponseDto> GetEventStatisticsAsync(int year)
        {
            // Yıl belirtilmediyse mevcut yıl kullanılır
            if (year == 0)
            {
                year = DateTime.UtcNow.Year;
            }

            var monthlyStats = new List<MonthlyEventStatisticsDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthEvents = await _context.Event
                    .Where(e => e.StartDate >= monthStart && e.StartDate <= monthEnd)
                    .ToListAsync();

                var eventCount = monthEvents.Count;
                var totalParticipants = 0;
                var averageParticipation = 0;

                if (eventCount > 0)
                {
                    totalParticipants = await _context.EventParticipant
                        .Where(ep => monthEvents.Select(me => me.EventId).Contains(ep.EventId))
                        .CountAsync();

                    averageParticipation = eventCount > 0 ? totalParticipants / eventCount : 0;
                }

                var monthName = new DateTime(year, month, 1).ToString("MMMM", new CultureInfo("tr-TR"));

                monthlyStats.Add(new MonthlyEventStatisticsDto
                {
                    Name = monthName,
                    EventCount = eventCount,
                    TotalParticipants = totalParticipants,
                    AverageParticipation = averageParticipation
                });
            }

            // Kullanılabilir yılları hesapla
            var availableYears = await _context.Event
                .Select(e => e.StartDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Any())
            {
                availableYears.Add(DateTime.UtcNow.Year);
            }

            return new EventStatisticsResponseDto
            {
                MonthlyStats = monthlyStats,
                AvailableYears = availableYears
            };
        }

        /// <summary>
        /// Harcama istatistiklerini filtreli olarak döndürür
        /// </summary>
        public async Task<ExpenseStatisticsResponseDto> GetExpenseStatisticsAsync(string clubId, string dateRange, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ClubExpenses.AsQueryable();

            // Kulüp filtresi
            if (clubId != "all" && int.TryParse(clubId, out int clubIdInt))
            {
                query = query.Where(e => e.ClubId == clubIdInt);
            }

            // Tarih filtresi
            var now = DateTime.UtcNow;

            switch (dateRange)
            {
                case "month":
                    startDate = now.AddMonths(-1);
                    break;
                case "quarter":
                    startDate = now.AddMonths(-3);
                    break;
                case "year":
                    startDate = now.AddYears(-1);
                    break;
                case "custom":
                    // Burada startDate ve endDate parametrelerini kullanıyoruz
                    // Eğer belirtilmemişse işlem yapmıyoruz
                    break;
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.Date <= endDate.Value);
            }

            var expenses = await query
                .Include(e => e.Club)
                .OrderByDescending(e => e.Date)
                .Select(e => new ExpenseStatisticsDto
                {
                    Id = e.Id,
                    ClubId = e.ClubId,
                    ClubName = e.Club.Name,
                    Date = e.Date.ToString("yyyy-MM-dd"),
                    Description = e.Description,
                    CashSupport = e.CashSupport,
                    InKindSupport = e.InKindSupport,
                    DokumanUrl = e.DokumanUrl
                })
                .ToListAsync();

            // Toplam değerleri hesapla
            var totalCashSupport = expenses.Sum(e => e.CashSupport);
            var totalInKindSupport = expenses.Sum(e => e.InKindSupport);

            // Kulüp listesi
            var clubs = await _context.Club
                .Select(c => new ClubBasicInfoDto
                {
                    Id = c.ClubId,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new ExpenseStatisticsResponseDto
            {
                Expenses = expenses,
                TotalCashSupport = totalCashSupport,
                TotalInKindSupport = totalInKindSupport,
                Clubs = clubs
            };
        }
    }
} 