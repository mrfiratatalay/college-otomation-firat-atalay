using ElasoftCommunityManagementSystem.Dtos.StatisticsDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IStatisticsService
    {
        /// <summary>
        /// Özet istatistikleri ve büyüme oranlarını döndürür
        /// </summary>
        Task<SummaryStatisticsDto> GetSummaryStatisticsAsync();

        /// <summary>
        /// Topluluk bazında istatistikleri döndürür
        /// </summary>
        Task<List<ClubStatisticsDto>> GetClubStatisticsAsync();

        /// <summary>
        /// Belirli bir yıl için aylık etkinlik istatistiklerini döndürür
        /// </summary>
        /// <param name="year">İstatistiklerin isteneceği yıl</param>
        Task<EventStatisticsResponseDto> GetEventStatisticsAsync(int year);

        /// <summary>
        /// Harcama istatistiklerini filtreli olarak döndürür
        /// </summary>
        /// <param name="clubId">Filtre için topluluk ID'si (hepsi için "all" gönderilir)</param>
        /// <param name="dateRange">Tarih aralığı filtresi (all, month, quarter, year, custom)</param>
        /// <param name="startDate">Özel tarih aralığı başlangıcı</param>
        /// <param name="endDate">Özel tarih aralığı sonu</param>
        Task<ExpenseStatisticsResponseDto> GetExpenseStatisticsAsync(string clubId, string dateRange, DateTime? startDate = null, DateTime? endDate = null);
    }
} 