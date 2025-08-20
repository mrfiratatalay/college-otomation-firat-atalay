import api from './api';

const categoryService = {
  /**
   * Tu00fcm kategorileri getirir
   * @returns {Promise} - API yanu0131tu0131
   */
  getAllCategories() {
    return api.get('/categories/listele');
  },

  /**
   * Belirli bir kategorinin detaylaru0131nu0131 getirir
   * @param {number} id - Kategori ID'si
   * @returns {Promise} - API yanu0131tu0131
   */
  getCategoryById(id) {
    return api.get(`/categories/${id}`);
  },
};

export default categoryService;
