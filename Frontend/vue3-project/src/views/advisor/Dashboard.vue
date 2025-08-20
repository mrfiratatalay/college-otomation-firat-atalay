<template>
  <div class="admin-dashboard">
    <div class="dashboard-header">
      <h1>Danışman Paneli</h1>
      <div class="user-info">
        <span>Hoş geldiniz, {{ advisorName }}</span>
      </div>
    </div>

    <div class="dashboard-stats">
      <div class="stat-card">
        <i class="fas fa-users"></i>
        <div class="stat-info">
          <h3>Toplam Üye</h3>
          <p>{{ getTotalMembers() }}</p>
        </div>
      </div>
      <div class="stat-card">
        <i class="fas fa-calendar"></i>
        <div class="stat-info">
          <h3>Toplam Etkinlik</h3>
          <p>{{ getTotalEvents() }}</p>
        </div>
      </div>
      <div class="stat-card">
        <i class="fas fa-building"></i>
        <div class="stat-info">
          <h3>Aktif Etkinlikler</h3>
          <p>{{ getTotalActiveEvents() }}</p>
        </div>
      </div>
      <div class="stat-card">
        <i class="fas fa-clock"></i>
        <div class="stat-info">
          <h3>Topluluklar</h3>
          <p>{{ advisorData.length }}</p>
        </div>
      </div>
    </div>

    <div class="dashboard-content">
      <div class="content-section">
        <h2>Son Aktiviteler</h2>
        <div class="activity-list">
          <div v-for="club in advisorData" :key="`club-${club.clubId}`">
            <div 
              v-for="(event, index) in club.recentClubEvents" 
              :key="`${club.clubId}-${index}`" 
              class="activity-item"
            >
              <div class="activity-icon event">
                <i class="fas fa-calendar-plus"></i>
              </div>
              <div class="activity-info">
                <p>{{ club.clubName }}: {{ event.name }}</p>
                <span class="activity-time">{{ formatDate(event.startDate) }} - {{ formatDate(event.endDate) }}</span>
              </div>
            </div>
          </div>
          <div v-if="getAllEvents().length === 0" class="activity-item">
            <div class="activity-icon event">
              <i class="fas fa-info-circle"></i>
            </div>
            <div class="activity-info">
              <p>Henüz etkinlik bulunmuyor.</p>
            </div>
          </div>
        </div>
      </div>

      <div class="content-section">
        <h2>Hızlı İşlemler</h2>
        <div class="quick-actions">
          <button @click="$router.push('/advisor/communities')" class="action-btn">
            <i class="fas fa-building"></i>
            Toplulukları Yönet
          </button>
          <button @click="$router.push('/advisor/events')" class="action-btn">
            <i class="fas fa-calendar-plus"></i>
            Etkinlikleri Görüntüle
          </button>
          <button @click="$router.push('/advisor/profile')" class="action-btn">
            <i class="fas fa-user"></i>
            Profil Ayarları
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import authService from '@/services/authService';
import dashboardService from '@/services/dashboardService';

export default {
  name: 'AdvisorDashboard',
  data() {
    return {
      advisorName: '',
      advisorData: []
    };
  },
  async created() {
    try {
      const user = authService.getUser();
      this.advisorName = user?.fullName || 'Danışman';

      const res = await dashboardService.getAdvisorDashboard();
      console.log('API response:', res.data);
      this.advisorData = res.data || [];
    } catch (err) {
      console.error('Danışman dashboard hatası:', err);
    }
  },
  methods: {
    formatDate(dateStr) {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      return date.toLocaleDateString('tr-TR');
    },
    getTotalMembers() {
      return this.advisorData.reduce((sum, club) => sum + (parseInt(club.totalMembers) || 0), 0);
    },
    getTotalEvents() {
      return this.advisorData.reduce((sum, club) => sum + (parseInt(club.totalEvents) || 0), 0);
    },
    getTotalActiveEvents() {
      return this.advisorData.reduce((sum, club) => sum + (parseInt(club.activeEvents) || 0), 0);
    },
    getAllEvents() {
      let allEvents = [];
      if (this.advisorData && this.advisorData.length) {
        this.advisorData.forEach(club => {
          if (club.recentClubEvents && club.recentClubEvents.length) {
            allEvents = [...allEvents, ...club.recentClubEvents.map(event => ({
              ...event,
              clubName: club.clubName
            }))];
          }
        });
      }
      return allEvents;
    }
  }
};
</script>

<style scoped>
.admin-dashboard {
  padding: 20px;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 15px;
}

.logout-btn {
  padding: 8px 16px;
  background-color: #e74c3c;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.dashboard-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 15px;
}

.stat-card i {
  font-size: 2rem;
  color: #3498db;
}

.stat-info h3 {
  margin: 0;
  font-size: 0.9rem;
  color: #666;
}

.stat-info p {
  margin: 5px 0 0;
  font-size: 1.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.dashboard-content {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 20px;
}

.content-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.content-section h2 {
  margin: 0 0 20px;
  color: #2c3e50;
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
  padding: 10px;
  background: #f8f9fa;
  border-radius: 4px;
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.activity-icon.join {
  background-color: #27ae60;
}

.activity-icon.event {
  background-color: #3498db;
}

.activity-icon.community {
  background-color: #f39c12;
}

.activity-info {
  flex: 1;
}

.activity-info p {
  margin: 0;
  color: #2c3e50;
}

.activity-time {
  font-size: 0.8rem;
  color: #666;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px;
  background: #f8f9fa;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  color: #2c3e50;
  transition: background-color 0.2s;
}

.action-btn:hover {
  background: #e9ecef;
}

.action-btn i {
  color: #3498db;
}

/* Responsive Adjustments */
@media (max-width: 992px) { 
  .dashboard-content {
    grid-template-columns: 1fr; /* Stack content sections */
  }
}

@media (max-width: 768px) {
  .admin-dashboard /* Assuming this class is used, or use .advisor-dashboard if specific */ {
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
  .quick-actions {
     /* Buttons are likely already stacking due to flex-direction: column default? */
     /* Add flex-direction: column; if needed */
     gap: 10px; /* Adjust gap if needed */
  }
}

@media (max-width: 480px) {
  .stat-card {
    flex-direction: column; 
    align-items: flex-start;
  }
  .stat-info p {
      font-size: 1.3rem; 
  }
  .action-btn {
      padding: 8px 12px;
      font-size: 0.9rem;
  }
}
</style>