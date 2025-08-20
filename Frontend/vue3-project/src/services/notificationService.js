import api from './api';

/**
 * Bildirimlerle ilgili API işlemlerini yöneten servis
 */
const notificationService = {
  /**
   * Okunmamış bildirimleri getirir
   * @returns {Promise} API yanıtı
   */
  getUnreadNotifications() {
    return api.get('/notifications/unread');
  },

  /**
   * Tüm bildirimleri getirir
   * @param {number} page - Sayfa numarası
   * @param {number} limit - Sayfa başına bildirim sayısı
   * @returns {Promise} API yanıtı
   */
  getAllNotifications(page = 1, limit = 10) {
    return api.get(`/notifications?page=${page}&limit=${limit}`);
  },

  /**
   * Bildirimi okundu olarak işaretler
   * @param {number} id - Bildirim ID
   * @returns {Promise} API yanıtı
   */
  markAsRead(id) {
    return api.put(`/notifications/${id}/read`);
  },

  /**
   * Tüm bildirimleri okundu olarak işaretler
   * @returns {Promise} API yanıtı
   */
  markAllAsRead() {
    return api.put('/notifications/mark-all-read');
  },

  /**
   * Bildirimi siler
   * @param {number} id - Bildirim ID
   * @returns {Promise} API yanıtı
   */
  deleteNotification(id) {
    return api.delete(`/notifications/${id}`);
  }
};

export default notificationService; 