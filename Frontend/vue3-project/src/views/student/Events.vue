<template>
  <div class="events-page">
    <!-- Bildirim bileşeni -->
    <div v-if="notification.show" class="notification" :class="`notification-${notification.type}`">
      <div class="notification-content">
        <span>{{ notification.message }}</span>
        <button @click="notification.show = false" class="notification-close">
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>

    <div class="events-header">
      <h1>Etkinlikler</h1>
    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="Etkinlik ara..."
            class="search-input"
          >
      </div>
        <select v-model="statusFilter" class="status-filter">
          <option value="all">Tüm Durumlar</option>
          <option value="upcoming">Yaklaşan</option>
          <option value="ongoing">Devam Eden</option>
          <option value="past">Geçmiş</option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Etkinlikler yükleniyor...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <i class="fas fa-exclamation-circle"></i>
      <p>{{ error }}</p>
      <button @click="loadEvents" class="retry-btn">
        <i class="fas fa-redo"></i> Tekrar Dene
      </button>
    </div>

    <div v-else class="events-grid">
      <div v-for="event in filteredEvents" :key="event.id" class="event-card">
        <div class="event-image">
          <img :src="event.imageUrl" :alt="event.title">
          <div class="event-date">
              <i class="fas fa-calendar"></i>
            {{ formatDate(event.startDate) }}
          </div>
          <div class="event-status" :class="getStatusBadgeClass(event)">
            {{ getEventStatus(event) }}
          </div>
        </div>
        
        <div class="event-content">
          <h3>{{ event.title }}</h3>
          <p class="event-description">{{ event.description }}</p>
          
          <div class="event-info">
            <p><i class="fas fa-clock"></i> {{ getRemainingTime(event.startDate) }}</p>
            <p><i class="fas fa-users"></i> {{ event.participantCount || 0 }} Katılımcı</p>
          </div>

          <div class="event-actions">
            <button
              class="details-btn"
              @click="openEventDetails(event)"
            >
              <i class="fas fa-info-circle"></i> Detaylar
            </button>
            <button 
              :class="['participation-btn', isUserJoined(event) ? 'leave' : 'join']"
              @click="(e) => toggleEventParticipation(event, e)"
              type="button"
            >
              <i :class="isUserJoined(event) ? 'fas fa-sign-out-alt' : 'fas fa-sign-in-alt'"></i>
              {{ isUserJoined(event) ? 'Ayrıl' : 'Katıl' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Etkinlik Detay Modalı -->
    <div v-if="showModal" class="modal-overlay" @click="closeEventDetails">
      <div class="modal-content" @click.stop>
        <button class="modal-close" @click="closeEventDetails">
          <i class="fas fa-times"></i>
        </button>
        
        <div v-if="selectedEvent" class="event-details">
          <div class="event-header">
            <img :src="selectedEvent.imageUrl" :alt="selectedEvent.title" class="event-detail-image">
            <div class="event-title-section">
              <h2>{{ selectedEvent.title }}</h2>
              <span :class="['status-badge', getStatusBadgeClass(selectedEvent)]">
                    {{ getEventStatus(selectedEvent) }}
                  </span>
            </div>
          </div>

          <div class="event-detail-content">
            <div class="detail-section">
              <h3>Etkinlik Bilgileri</h3>
              <p><i class="fas fa-align-left"></i> <strong>Açıklama:</strong> {{ selectedEvent.description }}</p>
              <p><i class="fas fa-calendar"></i> <strong>Başlangıç:</strong> {{ formatDate(selectedEvent.startDate) }}</p>
              <p><i class="fas fa-calendar-check"></i> <strong>Bitiş:</strong> {{ formatDate(selectedEvent.endDate) }}</p>
              <p><i class="fas fa-clock"></i> <strong>Süre:</strong> {{ calculateDuration(selectedEvent.startDate, selectedEvent.endDate) }}</p>
              <p><i class="fas fa-users"></i> <strong>Katılımcı Sayısı:</strong> {{ selectedEvent.participantCount || 0 }}</p>
            </div>

            <div class="detail-actions">
            <button
                :class="['participation-btn', isUserJoined(selectedEvent) ? 'leave' : 'join']"
                @click="(e) => toggleEventParticipation(selectedEvent, e)"
                type="button"
            >
              <i :class="isUserJoined(selectedEvent) ? 'fas fa-sign-out-alt' : 'fas fa-sign-in-alt'"></i>
                {{ isUserJoined(selectedEvent) ? 'Etkinlikten Ayrıl' : 'Etkinliğe Katıl' }}
            </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, reactive, computed } from 'vue';
import { studentService } from '@/services';

export default {
  name: 'Events',
  setup() {
    const events = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const searchQuery = ref('');
    const statusFilter = ref('all');
    const selectedEvent = ref(null);
    const showModal = ref(false);

    const notification = reactive({
      show: false,
      type: 'info',
      message: '',
      timeout: null
    });

    const showNotification = (type, message, duration = 3000) => {
      if (notification.timeout) {
        clearTimeout(notification.timeout);
      }
      notification.type = type;
      notification.message = message;
      notification.show = true;
      notification.timeout = setTimeout(() => {
        notification.show = false;
      }, duration);
    };

    const loadEvents = async () => {
      loading.value = true;
      error.value = null;
      try {
        const response = await studentService.getEvents();
        if (response && response.data) {
          events.value = response.data.map(event => ({
            ...event,
            imageUrl: event.image ? `data:image/jpeg;base64,${event.image}` : '/default-event.png'
          }));
        } else {
          throw new Error('Etkinlik verisi alınamadı');
        }
      } catch (err) {
        console.error('Etkinlik yükleme hatası:', err);
        error.value = err.message || 'Etkinlikler yüklenirken bir hata oluştu';
        showNotification('error', error.value);
      } finally {
        loading.value = false;
      }
    };

    const openEventDetails = (event) => {
      selectedEvent.value = event;
      showModal.value = true;
    };

    const closeEventDetails = () => {
      selectedEvent.value = null;
      showModal.value = false;
    };

    const joinEvent = async (eventId) => {
      try {
        await studentService.joinEvent(eventId);
        showNotification('success', 'Etkinliğe başarıyla katıldınız');
        await loadEvents();
      } catch (err) {
        showNotification('error', err.response?.data?.message || 'Etkinliğe katılırken bir hata oluştu');
      }
    };

    const leaveEvent = async (eventId) => {
      try {
        await studentService.leaveEvent(eventId);
        showNotification('success', 'Etkinlikten başarıyla ayrıldınız');
        await loadEvents();
      } catch (err) {
        showNotification('error', err.response?.data?.message || 'Etkinlikten ayrılırken bir hata oluştu');
      }
    };

    const formatDate = (dateString) => {
      if (!dateString) return '';
        const date = new Date(dateString);
      return date.toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'long',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
    };

    const getRemainingTime = (dateString) => {
      if (!dateString) return 'Tarih belirtilmemiş';
      const eventDate = new Date(dateString);
      const now = new Date();
      const difference = eventDate - now;

      if (difference < 0) {
        return 'Etkinlik sona erdi';
      }

      const days = Math.floor(difference / (1000 * 60 * 60 * 24));
      const hours = Math.floor((difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60));

      if (days > 0) {
        return `${days} gün ${hours} saat kaldı`;
      } else if (hours > 0) {
        return `${hours} saat ${minutes} dakika kaldı`;
      } else {
        return `${minutes} dakika kaldı`;
      }
    };

    const filteredEvents = computed(() => {
      if (!events.value || !Array.isArray(events.value)) {
        return [];
      }

      // Önce sadece onaylanmış etkinlikleri filtrele
      let filtered = events.value.filter(event => event.status === 'approved');

      // Arama filtrelemesi
      if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase();
        filtered = filtered.filter(event => {
          return (
            (event.title && event.title.toLowerCase().includes(query)) ||
            (event.description && event.description.toLowerCase().includes(query)) ||
            (event.location && event.location.toLowerCase().includes(query))
          );
        });
      }

      // Durum filtrelemesi
      const now = new Date();
      if (statusFilter.value !== 'all') {
        filtered = filtered.filter(event => {
          const startDate = event.startDate ? new Date(event.startDate) : null;
          const endDate = event.endDate ? new Date(event.endDate) : null;

          if (!startDate || !endDate) return false;

          switch (statusFilter.value) {
            case 'upcoming':
              return startDate > now;
            case 'ongoing':
              return startDate <= now && endDate >= now;
            case 'past':
              return endDate < now;
            default:
              return true;
          }
        });
      }

      return filtered;
    });

    const getEventStatus = (event) => {
      if (!event?.startDate || !event?.endDate) return 'Bilinmiyor';

        const now = new Date();
        const startDate = new Date(event.startDate);
        const endDate = new Date(event.endDate);

        if (now < startDate) {
          return 'Yaklaşan';
        } else if (now > endDate) {
          return 'Tamamlandı';
        } else {
          return 'Devam Ediyor';
        }
    };

    const getStatusBadgeClass = (event) => {
      const status = getEventStatus(event);
      switch (status) {
        case 'Yaklaşan':
          return 'bg-primary';
        case 'Devam Ediyor':
          return 'bg-success';
        case 'Tamamlandı':
          return 'bg-secondary';
        default:
          return 'bg-primary';
      }
    };

    const calculateDuration = (startDate, endDate) => {
      if (!startDate || !endDate) return 'Süre belirtilmemiş';
      
      const start = new Date(startDate);
      const end = new Date(endDate);
      const diffInHours = (end - start) / (1000 * 60 * 60);

      if (diffInHours < 24) {
        return `${Math.round(diffInHours)} saat`;
      } else {
        const days = Math.floor(diffInHours / 24);
        const hours = Math.round(diffInHours % 24);
        return `${days} gün${hours > 0 ? ` ${hours} saat` : ''}`;
      }
    };

    const isUserJoined = (event) => {
      return event?.isParticipating || false;
    };

    const toggleEventParticipation = async (event, e) => {
      if (e) {
        e.preventDefault();
      }

      if (!event?.eventId) {
        showNotification('error', 'Etkinlik ID\'si bulunamadı');
        return;
      }

      try {
        if (isUserJoined(event)) {
          await studentService.leaveEvent(event.eventId);
          event.isParticipating = false;
          event.participantCount = Math.max(0, (event.participantCount || 1) - 1);
          showNotification('success', 'Etkinlikten başarıyla ayrıldınız');
        } else {
          await studentService.joinEvent(event.eventId);
          event.isParticipating = true;
          event.participantCount = (event.participantCount || 0) + 1;
          showNotification('success', 'Etkinliğe başarıyla katıldınız');
        }
      } catch (error) {
        console.error('Etkinlik katılım/ayrılma hatası:', error);
        showNotification('error', error.response?.data?.message || 'İşlem sırasında bir hata oluştu');
      }
    };

    onMounted(() => {
      loadEvents();
    });

    return {
      events,
      loading,
      error,
      notification,
      selectedEvent,
      showModal,
      searchQuery,
      statusFilter,
      joinEvent,
      leaveEvent,
      formatDate,
      getRemainingTime,
      openEventDetails,
      closeEventDetails,
      filteredEvents,
      calculateDuration,
      getEventStatus,
      getStatusBadgeClass,
      isUserJoined,
      toggleEventParticipation
    };
  }
}
</script>

<style scoped>
.events-page {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.events-header {
  margin-bottom: 30px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 20px;
}

.events-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 2rem;
}

.filters {
  display: flex;
  gap: 15px;
  align-items: center;
}

.search-box {
  position: relative;
  width: 300px;
}

.search-box i {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #666;
}

.search-input {
  width: 100%;
  padding: 10px 10px 10px 35px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 0.9rem;
  transition: border-color 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: #3498db;
}

.status-filter {
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: white;
  color: #333;
  font-size: 0.9rem;
  cursor: pointer;
  transition: border-color 0.3s;
}

.status-filter:focus {
  outline: none;
  border-color: #3498db;
}

.events-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
  margin-top: 20px;
}

.event-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.event-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.event-image {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.event-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.event-date {
  position: absolute;
  bottom: 10px;
  left: 10px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 5px 10px;
  border-radius: 5px;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 5px;
}

.event-status {
  position: absolute;
  top: 10px;
  right: 10px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  backdrop-filter: blur(8px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 5px;
}

.event-status.bg-primary {
  background: rgba(52, 152, 219, 0.9);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.event-status.bg-success {
  background: rgba(46, 204, 113, 0.9);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.event-status.bg-secondary {
  background: rgba(149, 165, 166, 0.9);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.event-status::before {
  content: '';
  display: inline-block;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background-color: currentColor;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.8;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.event-content {
  padding: 20px;
}

.event-content h3 {
  margin: 0 0 10px 0;
  color: #2c3e50;
  font-size: 1.3rem;
}

.event-description {
  color: #666;
  margin-bottom: 15px;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.event-info {
  margin-bottom: 20px;
}

.event-info p {
  margin: 5px 0;
  color: #666;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
}

.event-info i {
  color: #3498db;
  width: 16px;
}

.event-actions {
  display: flex;
  justify-content: space-between;
  gap: 10px;
}

.details-btn, .participation-btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.details-btn {
  background: #f8f9fa;
  color: #2c3e50;
  border: 1px solid #dee2e6;
}

.details-btn:hover {
  background: #e9ecef;
}

.participation-btn.join {
  background: #2ecc71;
  color: white;
}

.participation-btn.join:hover {
  background: #27ae60;
}

.participation-btn.leave {
  background: #e74c3c;
  color: white;
}

.participation-btn.leave:hover {
  background: #c0392b;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.modal-close {
  position: absolute;
  top: 15px;
  right: 15px;
  background: rgba(0, 0, 0, 0.1);
  border: none;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.3s;
  z-index: 1;
}

.modal-close:hover {
  background: rgba(0, 0, 0, 0.2);
}

.event-details {
  padding: 0;
}

.event-header {
  position: relative;
}

.event-detail-image {
  width: 100%;
  height: 300px;
  object-fit: cover;
}

.event-title-section {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 25px;
  background: linear-gradient(transparent, rgba(0, 0, 0, 0.85));
  color: white;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.event-title-section h2 {
  margin: 0;
  font-size: 1.8rem;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 15px;
  font-size: 0.65rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  position: relative;
  transition: all 0.3s ease;
  width: fit-content;
  white-space: nowrap;
}

.status-badge::before {
  content: '';
  display: inline-block;
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background-color: currentColor;
  animation: pulse-ring 1.5s cubic-bezier(0.455, 0.03, 0.515, 0.955) infinite;
}

.status-badge.bg-primary {
  background: #3498db;
  color: white;
}

.status-badge.bg-success {
  background: #2ecc71;
  color: white;
}

.status-badge.bg-secondary {
  background: #95a5a6;
  color: white;
}

.event-detail-content {
  padding: 30px;
}

.detail-section {
  margin-bottom: 30px;
}

.detail-section h3 {
  color: #2c3e50;
  margin-bottom: 20px;
}

.detail-section p {
  margin: 10px 0;
  color: #666;
  display: flex;
  align-items: center;
  gap: 10px;
}

.detail-section i {
  color: #3498db;
  width: 20px;
}

.detail-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid #eee;
}

/* Loading ve Error Styles */
.loading-container, .error-container {
  text-align: center;
  padding: 40px;
  background: white;
  border-radius: 12px;
  margin: 20px 0;
}

.loading-spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container {
  color: #e74c3c;
}

.error-container i {
  font-size: 2rem;
  margin-bottom: 10px;
}

.retry-btn {
  margin-top: 15px;
  padding: 8px 20px;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  transition: background 0.3s;
}

.retry-btn:hover {
  background: #2980b9;
}

/* Notification Styles */
.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 15px 20px;
  border-radius: 8px;
  z-index: 1000;
  min-width: 300px;
  box-shadow: 0 3px 6px rgba(0,0,0,0.16);
}

.notification-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.notification-success {
  background-color: #2ecc71;
  color: white;
}

.notification-error {
  background-color: #e74c3c;
  color: white;
}

.notification-info {
  background-color: #3498db;
  color: white;
}

.notification-close {
  background: transparent;
  border: none;
  color: white;
  cursor: pointer;
  padding: 5px;
  margin-left: 10px;
  opacity: 0.8;
  transition: opacity 0.2s;
}

.notification-close:hover {
  opacity: 1;
}

/* Responsive Styles */
@media (max-width: 768px) {
  .events-header {
    flex-direction: column;
    align-items: stretch;
  }

  .filters {
    flex-direction: column;
  }

  .search-box {
    width: 100%;
  }

  .status-filter {
    width: 100%;
  }

  .events-grid {
    grid-template-columns: 1fr;
  }

  .modal-content {
    width: 95%;
    margin: 10px;
  }

  .event-detail-image {
    height: 200px;
  }

  .event-title-section h2 {
    font-size: 1.5rem;
  }

  .notification {
    width: 90%;
    right: 5%;
    left: 5%;
    min-width: auto;
  }
}
</style>