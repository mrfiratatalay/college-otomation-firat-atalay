<template>
    <div class="event-detail container py-4">
      <!-- Loading State -->
      <div v-if="loading" class="loading-indicator">
        <div class="spinner"></div>
        <p>Etkinlik bilgileri yükleniyor...</p>
      </div>
  
      <!-- Error State -->
      <div v-else-if="error" class="error-message">
        <p>{{ error }}</p>
        <button @click="fetchEventDetails" class="retry-btn">
          <i class="fas fa-sync"></i> Tekrar Dene
        </button>
      </div>
  
      <!-- Event Content -->
      <div v-else>
        <!-- Geri Dön Butonu -->
        <button class="back-btn" @click="$router.back()">
          <i class="fas fa-arrow-left"></i> Geri Dön
        </button>
  
        <!-- Etkinlik Başlık ve Üst Meta Alanı -->
        <div class="detail-header">
          <h1>{{ event.title }}</h1>
          <!-- Durum Göstergesi -->
          <div class="status-badge" :class="event.status?.toLowerCase()">
            <i class="fas" :class="getStatusIcon(event.status)"></i>
            {{ getStatusText(event.status) }}
          </div>
  
          <!-- META BİLGİLERİ GÖRSELLEŞTİRİLMİŞ HALDE -->
          <div class="meta-bar">
            <div class="meta-item" v-if="event.communityName">
              <i class="fas fa-users"></i>
              <span>{{ event.communityName }}</span>
            </div>
            <div class="meta-item" v-if="event.eventType">
              <i class="fas fa-tag"></i>
              <span>{{ event.eventType }}</span>
            </div>
            <div class="meta-item" v-if="event.startDate">
              <i class="fas fa-calendar"></i>
              <span>{{ formatDate(event.startDate) }}</span>
            </div>
            <div class="meta-item" v-if="event.startTime">
              <i class="fas fa-clock"></i>
              <span>{{ event.startTime }} - {{ event.endTime }}</span>
            </div>
            <div class="meta-item" v-if="event.location">
              <i class="fas fa-map-marker-alt"></i>
              <span>{{ event.location }}</span>
            </div>
          </div>
        </div>
  
        <!-- Etkinlik Görseli -->
        <div class="detail-image" v-if="event.imageUrl">
          <img :src="event.imageUrl" :alt="event.title">
        </div>
  
        <!-- Etkinlik Açıklama ve İstatistikler -->
        <div class="detail-content">
          <p class="description">{{ event.description }}</p>
          <div class="detail-stats">
            <span v-if="event.maxParticipants">
              <i class="fas fa-user-check"></i>
              {{ event.participantCount || 0 }}/{{ event.maxParticipants }} Katılımcı
            </span>
            <span v-if="event.startTime && event.endTime">
              <i class="fas fa-clock"></i>
              {{ calculateDuration(event.startTime, event.endTime) }} saat
            </span>
          </div>
  
          <!-- Onay/Red Butonları - Sadece PENDING durumunda göster -->
          <div class="action-buttons" v-if="event.status === 'PENDING' || event.status === 'pending'">
            <button class="btn-approve" @click="approveEvent()">
              <i class="fas fa-check"></i> Etkinliği Onayla
            </button>
            <button class="btn-reject" @click="confirmRejectEvent()">
              <i class="fas fa-times"></i> Etkinliği Reddet
            </button>
          </div>
        </div>
  
        <!-- Katılımcılar Bölümü -->
        <div class="participants-section" v-if="event.participantCount && event.participantCount > 0">
          <h3>Etkinlik Katılımcıları</h3>
          <div class="participant-list">
            <p>Bu etkinliğe {{ event.participantCount }} kişi katılıyor.</p>
            <!-- Katılımcı listesi erişilebilir olduğunda burada gösterilebilir -->
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import api from '@/services/api';
  import eventService from '@/services/eventService';
  
  export default {
    name: 'AdvisorEventDetail',
    data() {
      return {
        loading: true,
        error: null,
        event: {
          id: null,
          title: '',
          communityName: '',
          eventType: '',
          startDate: '',
          endDate: '',
          startTime: '',
          endTime: '',
          location: '',
          maxParticipants: 0,
          participantCount: 0,
          description: '',
          imageUrl: '',
          status: ''
        }
      }
    },
    created() {
      this.fetchEventDetails();
    },
    methods: {
      async fetchEventDetails() {
        this.loading = true;
        this.error = null;
        
        try {
          const eventId = this.$route.params.id;
          if (!eventId) {
            throw new Error('Etkinlik ID bulunamadı');
          }
          
          const response = await eventService.getEventById(eventId);
          
          if (response && response.data) {
            // Topluluklar gibi base64 formatındaki resmi işle
            const imageUrl = response.data.image ? `data:image/jpeg;base64,${response.data.image}` : 
                          (response.data.imageUrl || response.data.imagePath || '/images/event-placeholder.png');
            
            this.event = {
              ...response.data,
              id: response.data.id || response.data.eventId, // API'ye bağlı olarak id veya eventId kullanabiliriz
              participantCount: response.data.participantCount || 0,
              status: response.data.status || 'PENDING',
              imageUrl: imageUrl
            };
          } else {
            throw new Error('Etkinlik verisi alınamadı');
          }
        } catch (error) {
          console.error('Etkinlik detayları yüklenirken hata:', error);
          this.error = 'Etkinlik detayları yüklenirken bir hata oluştu.';
        } finally {
          this.loading = false;
        }
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
          const hours = Math.round((end - start) / 3600000);
          return hours;
        } catch (error) {
          console.error('Süre hesaplanırken hata:', error);
          return '';
        }
      },
      
      getStatusText(status) {
        if (!status) return 'Belirsiz';
        
        const statusMap = {
          'PENDING': 'Onay Bekliyor',
          'APPROVED': 'Onaylandı',
          'REJECTED': 'Reddedildi',
          'ONGOING': 'Devam Ediyor', 
          'COMPLETED': 'Tamamlandı',
          'CANCELLED': 'İptal Edildi',
          'pending': 'Onay Bekliyor',
          'approved': 'Onaylandı',
          'rejected': 'Reddedildi',
          'ongoing': 'Devam Ediyor',
          'completed': 'Tamamlandı',
          'cancelled': 'İptal Edildi'
        }
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
          'pending': 'fa-clock',
          'approved': 'fa-check',
          'rejected': 'fa-times',
          'ongoing': 'fa-play',
          'completed': 'fa-check-double',
          'cancelled': 'fa-ban'
        }
        return iconMap[status] || '';
      },
      
      async approveEvent() {
        try {
          await eventService.approveEvent(this.event.id);
          this.event.status = 'APPROVED';
          alert('Etkinlik başarıyla onaylandı!');
        } catch (error) {
          console.error('Etkinlik onaylanırken hata:', error);
          alert('Etkinlik onaylanırken bir hata oluştu.');
        }
      },
      
      confirmRejectEvent() {
        const isSure = confirm('Bu etkinliği reddetmek istediğinize emin misiniz?');
        if (isSure) {
          this.rejectEvent();
        }
      },
      
      async rejectEvent() {
        try {
          await eventService.rejectEvent(this.event.id);
          this.event.status = 'REJECTED';
          alert('Etkinlik reddedildi!');
        } catch (error) {
          console.error('Etkinlik reddedilirken hata:', error);
          alert('Etkinlik reddedilirken bir hata oluştu.');
        }
      }
    }
  }
  </script>
  
  <style scoped>
  .event-detail {
    max-width: 1000px;
    margin: 0 auto;
    padding: 1rem;
    background: #fdfdfd;
    border-radius: 8px;
    box-shadow: 0 2px 6px rgba(0,0,0,0.1);
  }
  
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
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    min-height: 200px;
  }
  
  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid rgba(0,0,0,0.1);
    border-radius: 50%;
    border-top: 4px solid #3498db;
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
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
  
  .back-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 1.5rem;
    background: none;
    border: none;
    color: #3498db;
    font-size: 1rem;
    cursor: pointer;
  }
  
  .detail-header {
    position: relative;
    margin-bottom: 1.5rem;
  }
  
  .detail-header h1 {
    margin: 0 0 1rem;
    color: #2c3e50;
    font-size: 2rem;
    font-weight: 600;
  }
  
  .status-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border-radius: 4px;
    color: white;
    font-weight: 600;
    margin-bottom: 1rem;
    box-shadow: 0 2px 4px rgba(0,0,0,0.2);
  }
  
  .status-badge.pending {
    background: #f39c12;
  }
  
  .status-badge.approved {
    background: #27ae60;
  }
  
  .status-badge.rejected {
    background: #e74c3c;
  }
  
  .status-badge.ongoing {
    background: #3498db;
  }
  
  .status-badge.completed {
    background: #95a5a6;
  }
  
  .status-badge.cancelled {
    background: #e74c3c;
  }
  
  /* Meta Bar: daha büyük, daha belirgin */
  .meta-bar {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    background: #f4f6f7;
    border-radius: 6px;
    padding: 0.75rem 1rem;
    box-shadow: inset 0 1px 3px rgba(0,0,0,0.06);
  }
  
  .meta-item {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 1rem;
    color: #34495e;
    font-weight: 500;
  }
  
  .detail-image img {
    width: 100%;
    border-radius: 8px;
    margin-bottom: 1.5rem;
    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
  }
  
  .detail-content {
    margin-top: 1rem;
  }
  
  .description {
    font-size: 1.1rem;
    line-height: 1.6;
    color: #444;
    margin-bottom: 1.5rem;
  }
  
  .detail-stats {
    display: flex;
    gap: 2rem;
    font-size: 1rem;
    color: #7f8c8d;
    margin-bottom: 1.5rem;
  }
  
  .detail-stats span {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
  }
  
  .detail-stats i {
    color: #3498db;
  }
  
  /* Aksiyon Butonları */
  .action-buttons {
    display: flex;
    gap: 1rem;
    margin-top: 1.5rem;
    padding: 1.5rem;
    background-color: #f8f9fa;
    border-radius: 8px;
    justify-content: center;
  }
  
  .btn-approve,
  .btn-reject {
    padding: 1rem 2rem;
    border: none;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    cursor: pointer;
    font-weight: 600;
    font-size: 1.1rem;
    transition: background-color 0.2s, transform 0.1s;
    min-width: 200px;
  }
  
  .btn-approve:hover,
  .btn-reject:hover {
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(0,0,0,0.1);
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
  
  .participants-section {
    margin-top: 2rem;
    padding: 1.5rem;
    background: #f8f9fa;
    border-radius: 8px;
  }
  
  .participants-section h3 {
    margin-top: 0;
    color: #2c3e50;
  }
  
  @media (max-width: 768px) {
    .detail-stats {
      flex-direction: column;
      gap: 0.75rem;
    }
    
    .action-buttons {
      flex-direction: column;
    }
  }
  </style>
  