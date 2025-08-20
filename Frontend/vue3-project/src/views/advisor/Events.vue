<template>
  <div class="advisor-events">
    <!-- Page Header -->
    <div class="page-header">
      <h1>Etkinlik Onayları</h1>
    </div>

    <!-- FILTERS -->
    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>
        <input type="text" v-model="searchQuery" placeholder="Etkinlik ara...">
      </div>
      <div class="filter-group">
        <select v-model="statusFilter">
          <option value="all">Tüm Durumlar</option>
          <option value="upcoming">Yaklaşan</option>
          <option value="approved">Onaylanan</option>
          <option value="ongoing">Devam Eden</option>
          <option value="completed">Tamamlanan</option>
          <option value="cancelled">İptal Edilen</option>
        </select>
        <select v-model="communityFilter">
          <option value="all">Tüm Topluluklar</option>
          <option v-for="community in communities" :key="community.id" :value="community.name">
            {{ community.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- LOADING STATE -->
    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Etkinlikler yükleniyor...</p>
    </div>

    <!-- ERROR STATE -->
    <div v-else-if="error" class="error-message">
      <p>{{ error }}</p>
      <button @click="fetchAdvisorEvents" class="retry-btn">
        <i class="fas fa-sync"></i> Tekrar Dene
      </button>
    </div>

    <!-- EVENTS LIST -->
    <div v-else class="events-grid">
      <!-- Eğer etkinlik yoksa bu mesaj gösterilecek -->
      <div v-if="filteredEvents.length === 0" class="no-events">
        Hiç etkinlik bulunamadı.
      </div>
      <div v-for="event in filteredEvents" :key="event.id" class="event-card" @click="showEventDetails(event)">
        <div class="event-image">
          <img :src="event.imageUrl || '/images/event-placeholder.png'" :alt="event.title">
          <div class="status-badge" :class="getStatusBadgeClass(event)">
            <i class="fas" :class="getStatusIcon(event.status)"></i>
            {{ getStatusText(event.status) }}
          </div>
        </div>
        <div class="event-content">
          <h3>{{ event.title }}</h3>
          <div class="event-details">
            <div class="detail-item">
              <i class="fas fa-calendar"></i>
              <span>{{ formatDate(event.startDate) }} - {{ formatDate(event.endDate) }}</span>
            </div>
            <div class="detail-item">
              <i class="fas fa-clock"></i>
              <span>{{ event.startTime }} - {{ event.endTime }}</span>
            </div>
            <div class="detail-item">
              <i class="fas fa-users"></i>
              <span>{{ event.communityName }}</span>
            </div>
          </div>
          <div class="event-stats">
            <span>
              <i class="fas fa-user-check"></i>
              {{ event.currentParticipants || 0 }}/{{ event.maxParticipants }} Katılımcı
            </span>
            <span v-if="event.startTime && event.endTime">
              <i class="fas fa-clock"></i>
              {{
                calculateDuration(event.startTime, event.endTime)
              }} saat
            </span>
          </div>
          <p class="description">{{ event.description }}</p>
          <!-- Butonları tüm etkinlikler için görünür hale getir ve uygun duruma göre göster -->
          <div class="event-actions">
            <button v-if="event.status === 'advisor_pending'" class="btn-approve" @click.stop="approveEvent(event.id)">
              <i class="fas fa-check"></i> Onayla
            </button>
            <button v-if="event.status === 'advisor_pending'" class="btn-reject" @click.stop="rejectEvent(event.id)">
              <i class="fas fa-times"></i> Reddet
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="selectedEvent" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ selectedEvent.title }}</h2>
          <button @click="closeModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <p><strong>Topluluk:</strong> {{ selectedEvent.communityName }}</p>
          <p><strong>Tarih:</strong> {{ formatDate(selectedEvent.startDate) }} - {{ formatDate(selectedEvent.endDate) }}
          </p>
          <p><strong>Saat:</strong> {{ selectedEvent.startTime }} - {{ selectedEvent.endTime }}</p>
          <p><strong>Açıklama:</strong> {{ selectedEvent.description }}</p>
          <p><strong>Katılımcılar:</strong> {{ selectedEvent.currentParticipants || 0 }}/{{
            selectedEvent.maxParticipants }}</p>
          <p v-if="selectedEvent.startTime && selectedEvent.endTime"><strong>Süre:</strong> {{
            calculateDuration(selectedEvent.startTime, selectedEvent.endTime)
          }} saat</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '@/services/api';
import eventService from '@/services/eventService';

export default {
  name: 'AdvisorEvents',
  data() {
    return {
      searchQuery: '',
      statusFilter: 'all',
      communityFilter: 'all',
      communities: [],
      events: [],
      selectedEvent: null,
      loading: false,
      error: null
    }
  },
  created() {
    this.fetchAdvisorEvents();
    this.fetchCommunities();
  },
  computed: {
    filteredEvents() {
      let filtered = this.events

      // Arama filtresi
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase()
        filtered = filtered.filter(event =>
          event.title?.toLowerCase().includes(query) ||
          event.description?.toLowerCase().includes(query)
        )
      }

      // Durum filtresi
      if (this.statusFilter !== 'all') {
        filtered = filtered.filter(event =>
          event.status?.toLowerCase() === this.statusFilter.toLowerCase()
        )
      }

      // Topluluk filtresi
      if (this.communityFilter !== 'all') {
        filtered = filtered.filter(event =>
          event.communityName === this.communityFilter
        )
      }

      return filtered
    }
  },
  methods: {
    async fetchAdvisorEvents() {
      this.loading = true;
      this.error = null;

      try {
        const response = await eventService.getAdvisorEvents();
        console.log("API'den gelen etkinlik verileri:", response.data);

        if (response && response.data) {
          this.events = response.data.map(event => {
            console.log("Etkinlik ID:", event.id, "Etkinlik:", event);

            // Topluluklar gibi base64 resmi işle
            const imageUrl = event.image ? `data:image/jpeg;base64,${event.image}` :
              (event.imageUrl || event.imagePath || '/images/event-placeholder.png');

            return {
              ...event,
              id: event.id || event.eventId, // API'ye bağlı olarak id veya eventId kullanabiliriz
              status: event.status || 'PENDING',
              currentParticipants: event.participantCount || 0,
              imageUrl: imageUrl
            };
          });
        }
      } catch (error) {
        console.error('Etkinlikler yüklenirken hata oluştu:', error);
        this.error = 'Etkinlikler yüklenirken bir hata oluştu.';
      } finally {
        this.loading = false;
      }
    },

    async fetchCommunities() {
      try {
        const response = await clubService.getAdvisorMyClubs();
        if (response && response.data) {
          // API'nin döndürdüğü veri formatına göre gerekirse alanları mapleme
          this.communities = response.data.map(club => ({
            id: club.clubId,
            name: club.name
          }));
        }
      } catch (error) {
        console.error('Topluluklar yüklenirken hata oluştu:', error);
      }
    },

    getStatusText(status) {
      const statusMap = {
        advisor_pending: 'Danışman Onayında',
        admin_pending: 'Admin Onayında',
        approved: 'Onaylandı',
        rejected: 'Reddedildi',
        upcoming: 'Yaklaşan',
        ongoing: 'Devam Eden',
        completed: 'Tamamlanan',
        cancelled: 'İptal Edilen'
      };
      return statusMap[status] || status;
    },

    getStatusIcon(status) {
      if (!status) return '';

      const iconMap = {
        'PENDING': 'fa-clock',
        'APPROVED': 'fa-check',
        'REJECTED': 'fa-times',
        'ONGOING': 'fa-play',
        'COMPLETED': 'fa-check-double',
        'CANCELLED': 'fa-ban',
        'upcoming': 'fa-clock',
        'approved': 'fa-check',
        'ongoing': 'fa-play',
        'completed': 'fa-check-double',
        'cancelled': 'fa-ban'
      }
      return iconMap[status] || '';
    },

    formatDate(dateString) {
      if (!dateString) return '';
      const options = { year: 'numeric', month: 'long', day: 'numeric' };
      return new Date(dateString).toLocaleDateString('tr-TR', options);
    },

    calculateDuration(startTime, endTime) {
      if (!startTime || !endTime) return '';

      try {
        const start = new Date(`1970-01-01T${startTime}`);
        const end = new Date(`1970-01-01T${endTime}`);
        return Math.round((end - start) / 3600000);
      } catch (error) {
        console.error('Süre hesaplanırken hata:', error);
        return '';
      }
    },

    async approveEvent(eventId) {
      try {
        await eventService.approveEventAsAdvisor(eventId);
        // Local olarak güncelle
        const event = this.events.find(e => e.id === eventId);
        if (event) {
          event.status = 'admin_pending';
        }
        this.$toast.success('Etkinlik danışman tarafından onaylandı. Şimdi admin onayında.');
        // Arka planda güncel veri çek
        this.fetchAdvisorEvents();
      } catch (error) {
        this.$toast.error('Onaylama işlemi başarısız.');
      }
    },

    async rejectEvent(eventId) {
      try {
        await eventService.rejectEventAsAdvisor(eventId);
        // Local olarak güncelle

        const event = this.events.find(e => e.id === eventId);

        if (event) {

          event.status = 'rejected';

        }
        this.$toast.success('Etkinlik danışman tarafından reddedildi.');
        await this.fetchAdvisorEvents();
      } catch (error) {
        this.$toast.error('Reddetme işlemi başarısız.');
      }
    },

    showEventDetails(event) {
      console.log("Tıklanan etkinlik:", event);

      if (event && event.id) {
        this.$router.push(`/advisor/events/${event.id}`);
      } else {
        console.error("Etkinlik ID'si bulunamadı");
        alert("Etkinlik detayları görüntülenemedi. Etkinlik ID'si yok.");
      }
    },

    showModal(event) {
      this.selectedEvent = event;
    },

    closeModal() {
      this.selectedEvent = null;
    },

    getStatusBadgeClass(event) {
      if (event.status === 'admin_pending' || event.status === 'advisor_pending') {
        return 'bg-warning';
      }
      switch (event.status) {
        case 'approved':
          return 'bg-success';
        case 'rejected':
          return 'bg-secondary';
        default:
          return 'bg-primary';
      }
    }
  }
}
</script>

<style scoped>
.advisor-events {
  padding: 2rem;
}

/* HEADER */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

/* FILTERS */
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
}

.search-box {
  position: relative;
  flex: 1;
  max-width: 400px;
}

.search-box i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #7f8c8d;
}

.search-box input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.filter-group {
  display: flex;
  gap: 1rem;
}

.filter-group select {
  padding: 0.75rem 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
}

/* LOADING VE ERROR STATE */
.loading-indicator,
.error-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  background: white;
  border-radius: 8px;
  margin: 1rem 0;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top: 4px solid #3498db;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.5rem 1rem;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.no-events {
  grid-column: 1 / -1;
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 8px;
  color: #7f8c8d;
}

/* EVENTS LIST (desktop defaults) */
.events-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

/* EVENT CARD */
.event-card {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  cursor: pointer;
}

/* EVENT IMAGE */
.event-image {
  position: relative;
  height: 200px;
}

.event-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.status-badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  color: white;
  font-size: 0.9rem;
  font-weight: 600;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  z-index: 5;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}

.status-badge.bg-warning {
  background: #f1c40f;
  color: #222;
}

.status-badge.bg-success {
  background: #02a947;
}

.status-badge.bg-secondary {
  background: #95a5a6;
}

.status-badge.bg-primary {
  background: #3498db;
}

/* EVENT CONTENT */
.event-content {
  padding: 1.5rem;
}

.event-content h3 {
  margin: 0 0 0.5rem;
  color: #2c3e50;
}

.event-meta {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.event-meta span {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.event-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #34495e;
}

.detail-item i {
  color: #7f8c8d;
  width: 20px;
}

.community {
  color: #666;
  font-size: 0.9rem;
  margin: 0.25rem 0;
}

.event-stats {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.event-stats span {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.description {
  color: #444;
  margin: 0.5rem 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
}

/* ACTION BUTTONS */
.event-actions {
  display: flex;
  gap: 0.5rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-top: 1px solid #eee;
  justify-content: center;
}

.btn-approve,
.btn-reject {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  flex: 1;
  max-width: 150px;
  font-weight: 600;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  cursor: pointer;
  transition: background-color 0.2s, transform 0.1s;
}

.btn-approve:hover,
.btn-reject:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}

.btn-approve:active,
.btn-reject:active {
  transform: translateY(0);
  box-shadow: none;
}

.btn-approve {
  background-color: #27ae60;
  color: white;
}

.btn-approve:hover {
  background-color: #219a52;
}

.btn-reject {
  background-color: #e74c3c;
  color: white;
}

.btn-reject:hover {
  background-color: #c0392b;
}

/* Modal Styles */
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  padding: 2rem;
  position: relative;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
}

.modal-body {
  font-size: 1rem;
  color: #333;
}

/* RESPONSIVE DESIGN */

/* Tablets (below 992px) */
@media (max-width: 992px) {
  .filters {
    flex-wrap: wrap;
    gap: 0.75rem;
  }

  .filter-group {
    flex-wrap: wrap;
  }

  .events-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
}

/* Mobile (below 576px) */
@media (max-width: 576px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .filters {
    flex-direction: column;
    gap: 0.75rem;
  }

  .search-box {
    max-width: 100%;
  }

  .filter-group {
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  .events-grid {
    grid-template-columns: 1fr;
  }

  .event-image {
    height: 180px;
  }

  .event-content {
    padding: 1rem;
  }
}
</style>
