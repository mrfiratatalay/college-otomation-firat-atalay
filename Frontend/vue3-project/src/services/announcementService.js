import api from './api';

const announcementService = {
  /**
   * Tüm duyuruları getirir
   * @returns {Promise} - API yanıtı
   */
  getAllAnnouncements() {
    return api.get('/announcements');
  },


  /**
   * Belirli bir duyurunun detaylarını getirir
   * @param {number} id - Duyuru ID'si
   * @returns {Promise} - API yanıtı
   */
  getAnnouncementById(id) {
    return api.get(`/announcements/${id}`);
  },

  /**
   * Belirli bir kulübün duyurularını getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getAnnouncementsByClubId(clubId) {
    return api.get(`/announcements/club/${clubId}`);
  },

  /**
   * Yeni duyuru oluşturur
   * @param {Object} announcementData - Duyuru bilgileri
   * @returns {Promise} - API yanıtı
   */
  createAnnouncement(announcementData) {
    return api.post('/announcements', announcementData);
  },

  /**
   * Duyuru bilgilerini günceller
   * @param {number} id - Duyuru ID'si
   * @param {Object} announcementData - Güncellenecek duyuru bilgileri
   * @returns {Promise} - API yanıtı
   */
  updateAnnouncement(id, announcementData) {
    return api.put(`/announcements/${id}`, announcementData);
  },

  /**
   * Duyuru siler
   * @param {number} id - Duyuru ID'si
   * @returns {Promise} - API yanıtı
   */
  deleteAnnouncement(id) {
    return api.delete(`/announcements/${id}`);
  },

  getAdvisorAnnouncements() {
    return api.get('/announcements/advisor');
  },
  async loadAnnouncements() {
    try {
      this.loading = true;
      const response = await announcementService.getAdvisorAnnouncements();
      this.announcements = response.data.sort((a, b) =>
        new Date(b.createdAt) - new Date(a.createdAt)
      );
      this.filterAnnouncements();
      console.log('Yüklenen duyurular:', this.announcements);
    } catch (error) {
      console.error('Duyurular yüklenirken hata:', error);
      this.error = 'Duyurular yüklenirken bir hata oluştu.';
    } finally {
      this.loading = false;
    }
  },

  /**
   * Duyuru durumunu değiştirir (aktif/pasif)
   * @param {number} id - Duyuru ID'si
   * @returns {Promise} - API yanıtı
   */
  toggleAnnouncementStatus(id) {
    return api.put(`/announcements/${id}/toggle-status`);
  }
};

export default announcementService;