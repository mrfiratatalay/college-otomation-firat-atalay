<template>
  <div class="admin-dashboard">
    <div class="dashboard-header">
      <h1>Yönetici Paneli</h1>
      <div class="user-info">
        <span>Hoş geldiniz, Admin</span>
      </div>
    </div>
    <div>
      
    </div>
    <div v-if="notification.show" class="notification" :class="`notification-${notification.type}`">
      <div class="notification-content">
        <div class="notification-icon">
          <i v-if="notification.type === 'success'" class="fas fa-check-circle"></i>
          <i v-else-if="notification.type === 'error'" class="fas fa-exclamation-circle"></i>
          <i v-else-if="notification.type === 'warning'" class="fas fa-exclamation-triangle"></i>
          <i v-else class="fas fa-info-circle"></i>
        </div>
        <span class="notification-message">{{ notification.message }}</span>
        <button class="notification-close" @click="notification.show = false">
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="notification-progress" :style="{ animationDuration: notification.duration + 'ms' }"></div>
    </div>

    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Veriler yükleniyor...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p>{{ error }}</p>
      <button @click="fetchAllData" class="retry-btn">Tekrar Dene</button>
    </div>

    <div v-else>
      <div class="dashboard-stats">
        <StatCard icon="fas fa-users" title="Toplam Üye" :value="dashboardData.totalUsers" />
        <StatCard icon="fas fa-calendar" title="Aktif Etkinlikler" :value="dashboardData.activeEvents" />
        <StatCard icon="fas fa-building" title="Topluluklar" :value="dashboardData.totalClubs" />
        <StatCard icon="fas fa-clock" title="Bekleyen İstekler" :value="pendingClubs.length" />
      </div>

      <div class="dashboard-content">
        <div class="content-section">
          <h2>Son Etkinlikler</h2>
          <div v-if="dashboardData.recentEvents && dashboardData.recentEvents.length > 0" class="activity-list">
            <div v-for="(event, index) in dashboardData.recentEvents" :key="'event-' + index" class="activity-item">
              <div class="activity-icon event">
                <i class="fas fa-calendar-check"></i>
              </div>
              <div class="activity-info">
                <p>{{ event.name }}</p>
                <span class="activity-time">{{ formatDate(event.startDate) }} - {{ formatDate(event.endDate) }}</span>
              </div>
            </div>
          </div>
          <p v-else class="no-data">Henüz gösterilecek etkinlik bulunmamaktadır.</p>
        </div>

        <div class="content-section">
          <h2>Son Başvurular</h2>
          <div v-if="pendingClubs.length > 0" class="application-cards">
            <ApplicationCard
              v-for="(club, index) in pendingClubs"
              :key="'club-app-' + index"
              :application="club"
              type="club"
              @approve="approveClub"
              @reject="rejectClub"
            />
          </div>
          <div v-else-if="dashboardData.recentApplications && dashboardData.recentApplications.length > 0" class="application-cards">
             <ApplicationCard
              v-for="(application, index) in dashboardData.recentApplications"
              :key="'member-app-' + index"
              :application="application"
              type="member"
              @approve="approveMemberApplication"
              @reject="rejectMemberApplication"
            />
          </div>
          <p v-else class="no-data">Henüz bekleyen başvuru bulunmamaktadır.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import dashboardService from '@/services/dashboardService';
import clubService from '@/services/clubService';
import StatCard from '@/components/admin/StatCard.vue';
import ApplicationCard from '@/components/admin/ApplicationCard.vue';

export default {
  name: 'AdminDashboard',
  components: {
    StatCard,
    ApplicationCard
  },
  data() {
    return {
      loading: true,
      error: null,
      dashboardData: {
        totalUsers: 0,
        totalClubs: 0,
        totalEvents: 0,
        activeEvents: 0,
        pendingApplications: 0,
        recentEvents: [],
        recentApplications: []
      },
      notification: {
        show: false,
        type: 'info',
        message: '',
        timeout: null,
        duration: 3000
      },
      pendingClubs: []
    }
  },
  created() {
    this.fetchAllData();
  },
  methods: {
    async fetchAllData() {
      this.loading = true;
      this.error = null;
      try {
        // Fetch both dashboard data and ALL clubs concurrently
        const [dashboardResponse, allClubsResponse] = await Promise.all([
          dashboardService.getAdminDashboard(),
          clubService.getAllClubs() // Fetch ALL clubs using the existing function
        ]);

        this.dashboardData = dashboardResponse.data || this.dashboardData; // Assign dashboard data
        console.log('Dashboard data:', JSON.stringify(this.dashboardData, null, 2)); // Log dashboard data structure


        // Filter ALL clubs to find pending ones on the frontend
        const allClubs = allClubsResponse.data || [];
        this.pendingClubs = allClubs.filter(club => club.status === 'pending');

        console.log('Fetched all clubs:', JSON.stringify(allClubs, null, 2)); // Log all clubs
        console.log('Filtered pending clubs:', JSON.stringify(this.pendingClubs, null, 2)); // Log pending clubs structure and IDs

      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        this.error = 'Veriler yüklenirken bir hata oluştu. Lütfen ağ bağlantınızı kontrol edin veya daha sonra tekrar deneyin.';
        // Log the specific error related to getAllClubs if it occurs
        if (err?.config?.url?.includes('/clubs/listele')) {
            console.error('Error specifically fetching clubs:', err);
        }
      } finally {
        this.loading = false;
      }
    },
    async approveClub(clubId) {
      console.log(`[approveClub] Received clubId: ${clubId}`); // Log received ID
      if (!clubId) return;
      console.log('[approveClub] Checking $toast:', this.$toast); // Log toast object
      try {
        console.log('[approveClub] Calling updateClubStatus...');
        await clubService.updateClubStatus(clubId);
        this.showNotification('success', 'Topluluk başarıyla onaylandı.');
        console.log('[approveClub] updateClubStatus successful.');
        console.log('[approveClub] Calling this.$toast.success...');
        this.$toast.success('Topluluk başarıyla onaylandı.');
        console.log('[approveClub] Toast success called.');
        console.log('[approveClub] Current pendingClubs before filter:', JSON.stringify(this.pendingClubs)); // Log before filter
        // Remove the approved club from the local list
        this.pendingClubs = this.pendingClubs.filter(club => {
            console.log(`[approveClub] Comparing filter: club.id (${club.clubId}) === clubId (${clubId}) -> ${club.clubId === clubId}`); // Log filter comparison detail - Assuming club object has clubId now based on ApplicationCard
            return club.clubId !== clubId; // IMPORTANT: Changed from club.id to club.clubId based on ApplicationCard emit
        });
        console.log('[approveClub] Current pendingClubs after filter:', JSON.stringify(this.pendingClubs)); // Log after filter
      } catch (error) {
        console.error('Error approving club:', error);
         console.log('[approveClub] Checking $toast in error:', this.$toast); // Log toast object in error
        this.$toast.error(`Topluluk onaylanırken bir hata oluştu: ${error.message || ''}`);
        this.showNotification('error', 'Topluluk onaylanırken bir hata oluştu.');
      }
    },
    async rejectClub(clubId) {
      if (!clubId) return;
      console.log(`Rejecting club with ID: ${clubId}`);
      if (confirm(`ID: ${clubId} olan topluluk başvurusunu reddetmek (silmek) istediğinizden emin misiniz? Bu işlem geri alınamaz.`)) {
        try {
          await clubService.deleteClub(clubId);
          this.$toast.success('Topluluk başvurusu başarıyla reddedildi (silindi).');
          this.showNotification('success', 'Topluluk başvurusu başarıyla reddedildi (silindi).');
          await this.fetchAllData();
        } catch (error) {
          console.error('Error rejecting club:', error);
          this.$toast.error(`Topluluk reddedilirken bir hata oluştu: ${error.message || ''}`);
        }
      }
    },
    async approveMemberApplication(applicationId) {
      if (!applicationId) return;
      console.log(`Approving member application with ID: ${applicationId}`);
      this.showNotification('success', 'Üyelik başvurusu başarıyla onaylandı.');
      // TODO: Replace with actual service call to approve member application
      try {
        // Example: await memberService.approveApplication(applicationId);
        this.$toast.success('Üyelik başvurusu başarıyla onaylandı.');
         // Remove the approved application from the local list
        this.dashboardData.recentApplications = this.dashboardData.recentApplications.filter(app => app.id !== applicationId); // Assuming application object has an 'id'
      } catch (error) {
        console.error('Error approving member application:', error);
        this.$toast.error(`Üyelik başvurusu onaylanırken bir hata oluştu: ${error.message || ''}`);
      }
    },
    async rejectMemberApplication(applicationId) {
      if (!applicationId) return;
      console.log(`Rejecting member application with ID: ${applicationId}`);
      this.showNotification('error', 'Üyelik başvurusu reddedildi.');
      // TODO: Replace with actual service call to reject member application
      // Consider if rejection means deleting or setting a 'rejected' status
      if (confirm(`ID: ${applicationId} olan üyelik başvurusunu reddetmek istediğinizden emin misiniz?`)) {
        try {
          // Example: await memberService.rejectApplication(applicationId);
          this.$toast.success('Üyelik başvurusu başarıyla reddedildi.');
          await this.fetchAllData(); // Refresh data
        } catch (error) {
          console.error('Error rejecting member application:', error);
          this.$toast.error(`Üyelik başvurusu reddedilirken bir hata oluştu: ${error.message || ''}`);
        }
      }
    },
    showNotification(type, message, duration = 3000) {
      this.notification.type = type;
      this.notification.message = message;
      this.notification.show = true;

      if (this.notification.timeout) {
        clearTimeout(this.notification.timeout);
      }

      this.notification.timeout = setTimeout(() => {
        this.notification.show = false;
      }, duration);
    },
    formatDate(dateString) {
      if (!dateString) return '';
      
      const date = new Date(dateString);
      return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric', month: 'long', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
      }).format(date);
    }
  }
}
</script>

<style scoped>
.admin-dashboard {
  padding: 25px;
  background-color: #f8f9fa;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 1px solid #dee2e6;
}

.dashboard-header h1 {
  color: #343a40;
  font-weight: 600;
}

.user-info span {
  color: #495057;
}

.loading-container, .error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  text-align: center;
  padding: 20px;
}

.loading-spinner {
  border: 4px solid rgba(0, 123, 255, 0.2);
  border-radius: 50%;
  border-top: 4px solid #007bff;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin-bottom: 15px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container {
  background-color: #fff3f3;
  border: 1px solid #f5c6cb;
  color: #721c24;
  border-radius: 8px;
}

.retry-btn {
  background-color: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 5px;
  cursor: pointer;
  margin-top: 15px;
  transition: background-color 0.2s ease;
}

.retry-btn:hover {
  background-color: #0056b3;
}

.dashboard-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.dashboard-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.content-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
  min-height: 250px;
}

.content-section h2 {
  margin: 0 0 20px;
  color: #343a40;
  font-size: 1.1rem;
  font-weight: 600;
  border-bottom: 1px solid #eee;
  padding-bottom: 10px;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px solid #e9ecef;
}

.activity-icon {
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.activity-icon.event {
  background-color: #17a2b8;
}

.activity-info p {
  margin: 0 0 3px 0;
  font-weight: 500;
  color: #343a40;
}

.activity-info .activity-time {
  font-size: 0.8rem;
  color: #6c757d;
}

.application-cards {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.no-data {
  color: #6c757d;
  text-align: center;
  padding: 30px 20px;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px dashed #dee2e6;
  font-style: italic;
}


.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  max-width: 350px;
  background-color: white;
  border-radius: 6px;
  overflow: hidden;
  box-shadow: 0 3px 15px rgba(0, 0, 0, 0.2);
  z-index: 9999;
  animation: slideInDown 0.3s ease-out;
  transition: all 0.3s ease;
}

@keyframes slideInDown {
  from {
    transform: translateY(-50px);
    opacity: 0;
  }

  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.notification-success {
  border-left: 4px solid #2ecc71;
}

.notification-error {
  border-left: 4px solid #e74c3c;
}

.notification-warning {
  border-left: 4px solid #f39c12;
}

.notification-info {
  border-left: 4px solid #3498db;
}

.notification-content {
  display: flex;
  align-items: center;
  padding: 12px 15px;
  gap: 10px;
}

.notification-icon {
  font-size: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-success .notification-icon {
  color: #2ecc71;
}

.notification-error .notification-icon {
  color: #e74c3c;
}

.notification-warning .notification-icon {
  color: #f39c12;
}

.notification-info .notification-icon {
  color: #3498db;
}

.notification-message {
  color: #333;
  font-size: 0.95rem;
  font-weight: 500;
  flex-grow: 1;
}

.notification-close {
  background: transparent;
  border: none;
  color: #999;
  cursor: pointer;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  transition: all 0.2s;
}

.notification-close:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: #333;
}

.notification-progress {
  height: 3px;
  background: linear-gradient(to right, rgba(0, 0, 0, 0.1), rgba(0, 0, 0, 0.3));
  width: 100%;
  animation: progress-bar 3000ms linear forwards;
  transform-origin: left;
}

@keyframes progress-bar {
  from {
    transform: scaleX(1);
  }

  to {
    transform: scaleX(0);
  }
}

/* Responsive Adjustments */
@media (max-width: 992px) { 
  .dashboard-content {
    grid-template-columns: 1fr; /* Stack content sections */
  }
}

@media (max-width: 768px) {
  .admin-dashboard {
    padding: 15px;
  }
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
    margin-bottom: 20px;
  }
  .dashboard-stats,
  .dashboard-content {
    gap: 15px;
  }
  .content-section {
    padding: 15px;
  }
  /* Add specific adjustments for ApplicationCard if needed */
  .application-cards {
    /* e.g., display: flex; flex-direction: column; */
  }
}

@media (max-width: 480px) {
  /* Further adjustments for very small screens */
  .stat-card /* Assuming StatCard component uses this class or similar */ {
      /* Maybe stack icon and text if using flex */
      /* flex-direction: column; align-items: flex-start; */
  }
}
</style> 