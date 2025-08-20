import api from './api';

const advisorService = {
  /**
   * Tüm danışmanları getirir
   * @returns {Promise} - API yanıtı
   */
  getAllAdvisors() {
    return api.get('/advisors');
  },

  /**
   * Belirli bir danışmanın detaylarını getirir
   * @param {number} id - Danışman ID'si
   * @returns {Promise} - API yanıtı
   */
  getAdvisorById(id) {
    return api.get(`/advisors/${id}`);
  },

  /**
   * Kullanılabilir danışmanları getirir
   * @returns {Promise} - API yanıtı
   */
  getAvailableAdvisors() {
    return api.get('/advisors/available');
  },

  /**
   * Danışmanın kulüplerini getirir
   * @param {number} id - Danışman ID'si
   * @returns {Promise} - API yanıtı
   */
  getAdvisorClubs(id) {
    return api.get(`/advisors/${id}/clubs`);
  },

  /**
   * Danışmanın istatistiklerini getirir
   * @param {number} id - Danışman ID'si
   * @returns {Promise} - API yanıtı
   */
  getAdvisorStats(id) {
    return api.get(`/advisors/${id}/stats`);
  },

  /**
   * Danışmanları arama
   * @param {string} name - Danışman adı
   * @returns {Promise} - API yanıtı
   */
  searchAdvisors(name) {
    return api.get(`/advisors/search?name=${name}`);
  },

  /**
   * Yeni danışman ekleme
   * @param {string} fullName - Danışmanın tam adı
   * @returns {Promise} - API yanıtı
   */
  addAdvisor(fullName) {
    return api.post('/advisors', fullName);
  },

  /**
   * Danışman silme
   * @param {number} id - Danışman ID'si
   * @returns {Promise} - API yanıtı
   */
  deleteAdvisor(id) {
    return api.delete(`/advisors/${id}`);
  }
};

export default advisorService;
