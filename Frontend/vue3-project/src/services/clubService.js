import api from './api';

const clubService = {
  /**
   * Tüm kulüpleri getirir
   * @returns {Promise} - API yanıtı
   */
  getAllClubs() {
    return api.get('/clubs/listele');
  },
  getClubStatistics() {
    return api.get('/clubs/istatistikler');
  },
  /**
   * Danışmanın kendi kulüplerini getirir
   * @param {number} advisorId - Danışman ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubsByAdvisorId(advisorId) {
    return api.get(`/clubs/advisor/${advisorId}`);
  },

  /**
   * Belirli bir kulübün detaylarını getirir
   * @param {number} id - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubById(id) {
    return api.get(`/clubs/${id}`);
  },

  /**
   * Yeni kulüp oluşturur
   * @param {Object} clubData - Kulüp bilgileri
   * @returns {Promise} - API yanıtı
   */
  createClub(clubData) {
    return api.post('/clubs', clubData, {
      headers: {
        'Content-Type': undefined // FormData için Axios'un otomatik Content-Type belirlemesini sağlar
      }
    });
  },

  /**
   * Kulüp bilgilerini günceller
   * @param {number} id - Kulüp ID'si
   * @param {Object} clubData - Güncellenecek kulüp bilgileri
   * @returns {Promise} - API yanıtı
   */
  updateClub(id, clubData) {
    return api.put(`/clubs/${id}/guncelle`, clubData, {
      headers: {
        'Content-Type': undefined // FormData için Axios'un otomatik Content-Type belirlemesini sağlar
      }
    });
  },

  /**
   * Kulüp siler
   * @param {number} id - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  deleteClub(id) {
    return api.delete(`/clubs/${id}`);
  },

  /**
   * Kulüp üyeliklerini getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubMemberCount(clubId) {
    return api.get(`/clubs/${clubId}/club-member-count`);
  },

  /**
   * Kulübün etkinlik sayısını getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubEventCount(clubId) {
    return api.get(`/clubs/${clubId}/club-event-count`);
  },

  /**
   * Kulübü aktif et
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  activateClub(clubId) {
    return api.put(`/clubs/${clubId}/aktif-et`);
  },

  /**
   * Kullanıcının üye olduğu tüm kulüplerdeki rollerini getirir
   * @param {number} userId - Kullanıcı ID'si
   * @returns {Promise} - API yanıtı
   */

  getUserRolesInClubs(userId) {
    return api.get(`/memberships/whoami`);
  },

  /**
   * Kulübe üyelik başvurusu yapar
   * @param {Object} data - Üyelik bilgileri (clubId, userId)
   * @returns {Promise} - API yanıtı
   */
  async applyToClub(data) {
    try {
      console.log('ClubService: applyToClub çağrıldı, veri:', data);

      // Doğrudan API'nin beklediği endpoint'e istek yapıyoruz
      const response = await api.post('/memberships/basvur', data);
      console.log('Başvuru başarılı:', response.data);
      return response.data;
    } catch (error) {
      console.log('ClubService: applyToClub hata:', error);

      if (error.response) {
        console.log('Hata Detayları:', {
          status: error.response.status,
          headers: error.response.headers,
          data: error.response.data
        });
      }

      throw error; // Hatayı yukarı aktar
    }
  },

  /**
   * Kullanıcının bekleyen başvurularını getirir
   * @returns {Promise} - API yanıtı
   */
  getMyPendingApplications() {
    return api.get('/memberships/my-club/basvurular');
  },

  /**
   * Kulüp üyeliğini siler (kulüpten ayrılma)
   * @param {number} membershipId - Üyelik ID'si
   * @returns {Promise} - API yanıtı
   */
  leaveClub(membershipId) {
    return api.delete(`/memberships/${membershipId}/ayril`);
  },

  /**
   * Danışman listesini getirir
   * @returns {Promise} - API yanıtı
   */
  getAdvisors() {
    return api.get('/advisors');
  },

  /**
   * Kulüp detayları ve üyeleri getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubDetails(clubId) {
    return api.get(`/clubs/${clubId}/details`);
  },

  /**
   * Kulüp üye başvurularını listeler
   * @returns {Promise} - API yanıtı
   */
  getPendingMemberships() {
    return api.get('/memberships/advisor/pending-applications');
  },

  /**
  * Kulüp üye başvurularını listeler
  * @param {number} clubId - Kulüp ID'si
  * @returns {Promise} - API yanıtı
  */
  getPendingMembershipss(clubId) {
    return api.get(`/memberships/pending/${clubId}`);
  },

  /**
   * Kulüp üye başvurusunu onaylar
   * @param {number} membershipId - Üyelik ID'si
   * @returns {Promise} - API yanıtı
   */
  approveMembership(membershipId) {
    return api.put(`/memberships/${membershipId}/basvuruonaylama`);
  },

  /**
   * Kulüp üye başvurusunu reddeder
   * @param {number} membershipId - Üyelik ID'si
   * @returns {Promise} - API yanıtı
   */
  rejectMembership(membershipId) {
    return api.delete(`/memberships/${membershipId}/basvurureddet`);
  },

  /**
   * Kulüp başvurusunu reddeder (durumu 'rejected' yapar)
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  rejectClub(clubId) {
    return api.put(`/clubs/${clubId}/reddet`);
  },

  /**
   * Kullanıcının tüm topluluk üyeliklerini getirir
   * @returns {Promise} - API yanıtı
   */
  getMyMemberships() {
    return api.get('/api/memberships/whoami');
  },

  /**
   * Kulüp durumunu günceller
   * @param {number} clubId - Kulüp ID'si
   * @param {string} status - Yeni durum (active, inactive, pending)
   * @returns {Promise} - API yanıtı
   */
  updateClubStatus(clubId, status) {
    return api.put(`/clubs/${clubId}/aktif-et`);
  },

  /**
   * Kulübün etkinliklerini getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubEvents(clubId) {
    return api.get(`/events/listele?clubId=${clubId}`);
  },

  /**
   * Kulübün üyelerini getirir
   * @param {number} clubId - Kulüp ID'si
   * @returns {Promise} - API yanıtı
   */
  getClubMembers(clubId) {
    return api.get(`/memberships/club/${clubId}/members`);
  },

  /**
   * Giriş yapan danışmanın kendi danışmanı olduğu kulüpleri getirir
   * @returns {Promise} - API yanıtı
   */
  getAdvisorMyClubs() {
    return api.get('/clubs/advisor/my-clubs');
  },

  /**
   * Topluluk başkanını değiştirir
   * @param {number} clubId - Topluluk ID'si
   * @param {number} newPresidentUserId - Yeni başkan kullanıcı ID'si
   * @returns {Promise} - API yanıtı
   */
  changeClubPresident(clubId, newPresidentUserId) {
    return api.put(`/memberships/club/${clubId}/change-president`, {
      newPresidentUserId: newPresidentUserId
    });
  }
};

export default clubService; 