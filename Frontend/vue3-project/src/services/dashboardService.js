import api from './api';

const dashboardService = {
  /**
   * Danışman dashboard verileri
   */
  getAdvisorDashboard() {
    return api.get('/dashboard/advisordashboard');
  },

  /**
   * Admin dashboard verileri
   */
  getAdminDashboard() {
    return api.get('/dashboard/admindashboard');
  },

  /**
   * Öğrenci dashboard verileri
   */
  getStudentDashboard() {
    return api.get('/dashboard/studentdashboard');
  }
};

export default dashboardService;
