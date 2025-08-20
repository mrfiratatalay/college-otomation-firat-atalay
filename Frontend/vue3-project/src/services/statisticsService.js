import api from './api';

/**
 * İstatistik servis fonksiyonları
 */
const statisticsService = {
  /**
   * Özet istatistikleri getirir (üye sayısı, topluluk sayısı, etkinlik sayısı, harcamalar)
   */
  getSummaryStatistics() {
    return api.get('/statistics/summary');
  },

  /**
   * Topluluk bazında istatistikleri getirir
   */
  getClubStatistics() {
    return api.get('/statistics/clubs');
  },

  /**
   * Belirli bir yıl için aylık etkinlik istatistiklerini getirir
   * @param {number} year - İstenen yıl (opsiyonel, belirtilmediğinde mevcut yıl kullanılır)
   */
  getEventStatistics(year = 0) {
    return api.get(`/statistics/events?year=${year}`);
  },

  /**
   * Harcama istatistiklerini filtreleri ile getirir
   * @param {string} clubId - Topluluk ID'si (hepsi için "all")
   * @param {string} dateRange - Tarih aralığı (all, month, quarter, year, custom)
   * @param {string} startDate - Özel başlangıç tarihi (YYYY-MM-DD formatında)
   * @param {string} endDate - Özel bitiş tarihi (YYYY-MM-DD formatında)
   */
  getExpenseStatistics(clubId = 'all', dateRange = 'all', startDate = null, endDate = null) {
    let url = `/statistics/expenses?clubId=${clubId}&dateRange=${dateRange}`;
    
    if (startDate) {
      url += `&startDate=${startDate}`;
    }
    
    if (endDate) {
      url += `&endDate=${endDate}`;
    }
    
    return api.get(url);
  }
};

export default statisticsService; 