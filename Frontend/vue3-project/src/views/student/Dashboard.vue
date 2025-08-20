<template>
  <div class="student-dashboard">
    <div class="dashboard-header">
      <h1>Öğrenci Paneli</h1>
      <div class="user-info">
        <span>Hoş geldiniz, {{ studentName }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      Veriler yükleniyor...
    </div>

    <div v-else-if="error" class="error-state">
      {{ error }}
    </div>

    <template v-else>
      <div class="dashboard-stats">
        <div class="stat-card">
          <i class="fas fa-users"></i>
          <div class="stat-info">
            <h3>Üye Olduğum Topluluklar</h3>
            <p>{{ getTotalClubs() }}</p>
          </div>
        </div>
        <div class="stat-card">
          <i class="fas fa-calendar"></i>
          <div class="stat-info">
            <h3>Katıldığım Etkinlikler</h3>
            <p>{{ getTotalEvents() }}</p>
          </div>
        </div>
        <div class="stat-card">
          <i class="fas fa-building"></i>
          <div class="stat-info">
            <h3>Aktif Etkinlikler</h3>
            <p>{{ getUpcomingEvents() }}</p>
          </div>
        </div>
      </div>

      <div class="dashboard-content">
        <div class="content-section">
          <h2>Aktif Etkinlikler</h2>
          <div class="activity-list">
            <div v-for="event in filteredUpcomingEvents" :key="event.eventId" class="activity-item">
              <div class="activity-icon event">
                <i class="fas fa-calendar-plus"></i>
              </div>
              <div class="activity-info">
                <p>{{ event.clubName || 'Topluluk' }}: {{ event.name }}</p>
                <span class="activity-time">{{ formatDate(event.startDate) }} - {{ formatDate(event.endDate) }}</span>
              </div>
            </div>
            <div v-if="!filteredUpcomingEvents.length" class="activity-item">
              <div class="activity-icon event">
                <i class="fas fa-info-circle"></i>
              </div>
              <div class="activity-info">
                <p>Aktif etkinlik bulunmuyor.</p>
              </div>
            </div>
          </div>
        </div>

        <div class="content-section">
          <h2>Hızlı İşlemler</h2>
          <div class="quick-actions">
            <button @click="$router.push('/student/communities')" class="action-btn">
              <i class="fas fa-building"></i>
              Toplulukları Görüntüle
            </button>
            <button @click="$router.push('/student/events')" class="action-btn">
              <i class="fas fa-calendar-plus"></i>
              Etkinlikleri Görüntüle
            </button>
            <button @click="$router.push('/student/profile')" class="action-btn">
              <i class="fas fa-user"></i>
              Profil Ayarları
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script>
import authService from '@/services/authService';
import dashboardService from '@/services/dashboardService';
import studentService from '@/services/studentService';

export default {
  name: 'StudentDashboard',
  data() {
    return {
      studentName: '',
      loading: true,
      error: null,
      studentData: {
        upcomingEvents: [],
        joinedClubs: [],
        totalEvents: 0
      },
      activeEvents: []
    };
  },
  async created() {
    try {
      this.loading = true;
      const user = authService.getUser();
      this.studentName = user?.fullName || 'Öğrenci';

      // Önce Events.vue'da kullanılan servisi çağıralım
      const eventsResponse = await studentService.getEvents();
      console.log('Events verisi:', eventsResponse.data);
      
      // Aktif etkinlikleri filtrele
      this.activeEvents = eventsResponse.data.filter(event => event.status === 'approved');
      console.log('Aktif etkinlikler:', this.activeEvents);

      const res = await dashboardService.getStudentDashboard();
      if (res.data) {
        this.studentData = {
          upcomingEvents: this.activeEvents, // Aktif etkinlikleri kullan
          joinedClubs: res.data.joinedClubs || [],
          totalEvents: res.data.totalEvents || 0
        };
      }
    } catch (err) {
      console.error('Öğrenci dashboard hatası:', err);
      this.error = 'Veriler yüklenirken bir hata oluştu: ' + err.message;
    } finally {
      this.loading = false;
    }
  },
  methods: {
    formatDate(dateStr) {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      return date.toLocaleDateString('tr-TR');
    },
    getTotalClubs() {
      const count = this.studentData.joinedClubs?.length || 0;
      console.log('Topluluk verisi:', this.studentData.joinedClubs);
      console.log('Topluluk sayısı:', count);
      return count;
    },
    getTotalEvents() {
      const count = this.studentData.totalEvents || 0;
      console.log('Toplam etkinlik sayısı:', count);
      return count;
    },
    getUpcomingEvents() {
      // Events.vue'dan gelen aktif etkinlik sayısını döndür
      return this.activeEvents.length;
    }
  },
  computed: {
    filteredUpcomingEvents() {
      // Aktif etkinlikleri döndür
      return this.activeEvents;
    }
  }
};
</script>

<style scoped>
.student-dashboard {
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
  background: #e3f2fd;
  color: #1976d2;
}

.activity-info p {
  margin: 0;
  font-weight: 500;
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
  justify-content: center;
  gap: 8px;
  padding: 10px 15px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  transition: background-color 0.3s ease;
  text-align: center;
}

.action-btn:hover {
  background-color: #2980b9;
}

.loading-state,
.error-state {
  padding: 40px;
  text-align: center;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  color: #666;
}

/* Responsive Adjustments */
@media (max-width: 992px) {
  .dashboard-content {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .student-dashboard {
    padding: 15px;
  }
  .dashboard-header {
    margin-bottom: 20px;
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  .dashboard-stats {
    gap: 15px;
  }
  .dashboard-content {
    gap: 15px;
  }
  .content-section {
    padding: 15px;
  }
}

@media (max-width: 480px) {
  .stat-card {
    flex-direction: column;
    align-items: flex-start;
    text-align: left;
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