<template>
  <div class="notification-dropdown">
    <div class="notification-bell" @click="toggleDropdown">
      <i class="fas fa-bell"></i>
      <span v-if="unreadCount > 0" class="notification-badge">{{ unreadCount }}</span>
    </div>

    <div v-if="isOpen" class="dropdown-menu">
      <div class="dropdown-header">
        <h3>Bildirimler</h3>
        <button v-if="notifications.length > 0" @click="markAllAsRead" class="mark-all-read">
          Tümünü Okundu İşaretle
        </button>
      </div>

      <div v-if="loading" class="loading">
        <div class="spinner"></div>
        <p>Bildirimler yükleniyor...</p>
      </div>
      
      <div v-else-if="error" class="error">
        <p>{{ error }}</p>
        <button @click="fetchNotifications" class="retry-button">
          <i class="fas fa-sync"></i> Tekrar Dene
        </button>
      </div>

      <div v-else-if="notifications.length === 0" class="empty-state">
        <i class="fas fa-check-circle"></i>
        <p>Yeni bildiriminiz yok</p>
      </div>

      <div v-else class="notification-list">
        <div 
          v-for="notification in notifications" 
          :key="notification.id" 
          class="notification-item"
          :class="{ 'unread': !notification.read }"
          @click="viewNotification(notification)"
        >
          <div class="notification-icon" :class="notification.type">
            <i :class="getIconClass(notification.type)"></i>
          </div>
          <div class="notification-content">
            <h4>{{ notification.title }}</h4>
            <p>{{ notification.message }}</p>
            <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
          </div>
          <div class="notification-actions">
            <button 
              v-if="!notification.read" 
              @click.stop="markAsRead(notification.id)" 
              title="Okundu olarak işaretle"
              class="action-button"
            >
              <i class="fas fa-check"></i>
            </button>
            <button 
              @click.stop="deleteNotification(notification.id)" 
              title="Bildirimi sil"
              class="action-button delete"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="dropdown-footer">
        <a href="#" @click.prevent="viewAllNotifications">Tüm Bildirimleri Görüntüle</a>
      </div>
    </div>
  </div>
</template>

<script>
import { notificationService } from '@/services';

export default {
  name: 'NotificationDropdown',
  data() {
    return {
      isOpen: false,
      loading: false,
      error: null,
      notifications: [],
      unreadCount: 0
    };
  },
  mounted() {
    // Sayfa yüklendiğinde bildirimleri getir
    this.fetchNotifications();
    
    // Dışarı tıklandığında dropdown'ı kapat
    document.addEventListener('click', this.closeOnClickOutside);
    
    // Bildirimleri her 1 dakikada bir yenile
    this.startPolling();
  },
  beforeUnmount() {
    // Event listener'ları temizle
    document.removeEventListener('click', this.closeOnClickOutside);
    
    // Polling'i durdur
    this.stopPolling();
  },
  methods: {
    toggleDropdown(event) {
      // Event'in yukarı propagasyonunu durdur
      event.stopPropagation();
      this.isOpen = !this.isOpen;
      
      // Açıldığında bildirimleri yenile
      if (this.isOpen) {
        this.fetchNotifications();
      }
    },
    closeOnClickOutside(event) {
      // Dropdown dışına tıklandığında kapat
      if (this.isOpen && !this.$el.contains(event.target)) {
        this.isOpen = false;
      }
    },
    async fetchNotifications() {
      this.loading = true;
      this.error = null;
      
      try {
        // Okunmamış bildirimleri getir
        const response = await notificationService.getUnreadNotifications();
        
        if (response && response.data) {
          this.notifications = response.data.notifications || [];
          this.unreadCount = response.data.count || this.notifications.length;
        } else {
          // Test için örnek veriler
          this.loadSampleData();
        }
      } catch (error) {
        console.error('Bildirimler getirilirken hata:', error);
        this.error = 'Bildirimler yüklenirken bir hata oluştu.';
        
        // Backend henüz hazır değilse örnek veriler kullan
        this.loadSampleData();
      } finally {
        this.loading = false;
      }
    },
    loadSampleData() {
      // Örnek bildirim verileri
      this.notifications = [
        {
          id: 1,
          title: 'Yeni Etkinlik',
          message: 'Yazılım Kampı etkinliği oluşturuldu',
          createdAt: new Date(Date.now() - 1000 * 60 * 10), // 10 dakika önce
          read: false,
          type: 'event'
        },
        {
          id: 2,
          title: 'Topluluk Daveti',
          message: 'Yapay Zeka Topluluğuna davet edildiniz',
          createdAt: new Date(Date.now() - 1000 * 60 * 60), // 1 saat önce
          read: false,
          type: 'invite'
        },
        {
          id: 3,
          title: 'Yeni Duyuru',
          message: 'Bahar Şenliği Duyurusu yayınlandı',
          createdAt: new Date(Date.now() - 1000 * 60 * 60 * 3), // 3 saat önce
          read: true,
          type: 'announcement'
        }
      ];
      
      // Okunmamış bildirimleri say
      this.unreadCount = this.notifications.filter(n => !n.read).length;
    },
    async markAsRead(id) {
      try {
        // Backend API'sine bildirim okundu bildirimi gönder
        await notificationService.markAsRead(id);
        
        // Frontend'de bildirimi okundu olarak işaretle
        const notification = this.notifications.find(n => n.id === id);
        if (notification) {
          notification.read = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      } catch (error) {
        console.error(`Bildirim ${id} okundu olarak işaretlenirken hata:`, error);
        
        // API çağrısı başarısız olsa bile UI'ı güncelle (optimistik güncelleme)
        const notification = this.notifications.find(n => n.id === id);
        if (notification) {
          notification.read = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      }
    },
    async markAllAsRead() {
      try {
        // Backend API'sine tüm bildirimleri okundu bildirimi gönder
        await notificationService.markAllAsRead();
        
        // Frontend'de tüm bildirimleri okundu olarak işaretle
        this.notifications.forEach(notification => {
          notification.read = true;
        });
        this.unreadCount = 0;
      } catch (error) {
        console.error('Tüm bildirimler okundu olarak işaretlenirken hata:', error);
        
        // API çağrısı başarısız olsa bile UI'ı güncelle (optimistik güncelleme)
        this.notifications.forEach(notification => {
          notification.read = true;
        });
        this.unreadCount = 0;
      }
    },
    async deleteNotification(id) {
      try {
        // Backend API'sine bildirim silme isteği gönder
        await notificationService.deleteNotification(id);
        
        // Frontend'den bildirimi kaldır
        const index = this.notifications.findIndex(n => n.id === id);
        if (index !== -1) {
          // Silinen bildirim okunmamış ise sayacı güncelle
          if (!this.notifications[index].read) {
            this.unreadCount = Math.max(0, this.unreadCount - 1);
          }
          // Bildirimi diziden çıkar
          this.notifications.splice(index, 1);
        }
      } catch (error) {
        console.error(`Bildirim ${id} silinirken hata:`, error);
        
        // API çağrısı başarısız olsa bile UI'ı güncelle (optimistik güncelleme)
        const index = this.notifications.findIndex(n => n.id === id);
        if (index !== -1) {
          // Silinen bildirim okunmamış ise sayacı güncelle
          if (!this.notifications[index].read) {
            this.unreadCount = Math.max(0, this.unreadCount - 1);
          }
          // Bildirimi diziden çıkar
          this.notifications.splice(index, 1);
        }
      }
    },
    viewNotification(notification) {
      // Eğer okunmamışsa, okundu olarak işaretle
      if (!notification.read) {
        this.markAsRead(notification.id);
      }
      
      // Bildirim tipine göre yönlendirme yap
      switch (notification.type) {
        case 'event':
          // Etkinliğe yönlendir
          // this.$router.push(`/events/details/${notification.entityId}`);
          break;
        case 'announcement':
          // Duyuruya yönlendir
          // this.$router.push(`/announcements/details/${notification.entityId}`);
          break;
        case 'invite':
          // Topluluk davetlerine yönlendir
          // this.$router.push('/communities/invites');
          break;
        default:
          // Genel bildirimler sayfasına yönlendir
          // this.$router.push('/notifications');
      }
      
      // Dropdown'ı kapat
      this.isOpen = false;
    },
    viewAllNotifications() {
      // Tüm bildirimler sayfasına yönlendir
      // this.$router.push('/notifications');
      
      // Dropdown'ı kapat
      this.isOpen = false;
    },
    formatTime(date) {
      // Date string veya Date objesi alıp, '5 dakika önce', '2 saat önce' gibi formatlı metin döndürür
      if (!date) return '';
      
      const now = new Date();
      const notificationDate = new Date(date);
      const diffMs = now - notificationDate;
      
      // Saniye bazında fark
      const diffSeconds = Math.floor(diffMs / 1000);
      
      if (diffSeconds < 60) {
        return 'Az önce';
      }
      
      // Dakika bazında fark
      const diffMinutes = Math.floor(diffSeconds / 60);
      
      if (diffMinutes < 60) {
        return `${diffMinutes} dakika önce`;
      }
      
      // Saat bazında fark
      const diffHours = Math.floor(diffMinutes / 60);
      
      if (diffHours < 24) {
        return `${diffHours} saat önce`;
      }
      
      // Gün bazında fark
      const diffDays = Math.floor(diffHours / 24);
      
      if (diffDays < 7) {
        return `${diffDays} gün önce`;
      }
      
      // Hafta bazında fark
      const diffWeeks = Math.floor(diffDays / 7);
      
      if (diffWeeks < 4) {
        return `${diffWeeks} hafta önce`;
      }
      
      // Ay bazında fark (yaklaşık)
      const diffMonths = Math.floor(diffDays / 30);
      
      if (diffMonths < 12) {
        return `${diffMonths} ay önce`;
      }
      
      // Yıl bazında fark
      const diffYears = Math.floor(diffDays / 365);
      return `${diffYears} yıl önce`;
    },
    getIconClass(type) {
      // Bildirim tipine göre font awesome icon class döndürür
      switch (type) {
        case 'event':
          return 'fas fa-calendar-alt';
        case 'announcement':
          return 'fas fa-bullhorn';
        case 'invite':
          return 'fas fa-user-plus';
        case 'message':
          return 'fas fa-envelope';
        case 'alert':
          return 'fas fa-exclamation-triangle';
        case 'task':
          return 'fas fa-tasks';
        case 'comment':
          return 'fas fa-comment';
        case 'like':
          return 'fas fa-heart';
        default:
          return 'fas fa-bell';
      }
    },
    startPolling() {
      // Bildirimleri düzenli aralıklarla yenile (her 1 dakikada bir)
      this.pollingInterval = setInterval(() => {
        if (!this.isOpen) { // Sadece dropdown kapalıyken yenile
          this.fetchNotifications();
        }
      }, 60000); // 60 saniye
    },
    stopPolling() {
      // Polling interval'ı temizle
      if (this.pollingInterval) {
        clearInterval(this.pollingInterval);
      }
    }
  }
};
</script>

<style scoped>
.notification-dropdown {
  position: relative;
  display: inline-block;
}

.notification-bell {
  position: relative;
  cursor: pointer;
  padding: 10px;
  font-size: 1.2rem;
  color: #555;
}

.notification-bell:hover {
  color: #3498db;
}

.notification-badge {
  position: absolute;
  top: 0;
  right: 0;
  background-color: #e74c3c;
  color: white;
  border-radius: 50%;
  min-width: 18px;
  height: 18px;
  font-size: 0.7rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2px;
}

.dropdown-menu {
  position: absolute;
  top: 40px;
  right: 0;
  width: 320px;
  max-width: 90vw;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  overflow: hidden;
  animation: dropdown-animation 0.2s ease-out;
}

@keyframes dropdown-animation {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.dropdown-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 15px;
  border-bottom: 1px solid #eee;
}

.dropdown-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: #333;
}

.mark-all-read {
  background: none;
  border: none;
  color: #3498db;
  cursor: pointer;
  font-size: 0.8rem;
  padding: 0;
}

.mark-all-read:hover {
  text-decoration: underline;
}

.notification-list {
  max-height: 380px;
  overflow-y: auto;
}

.notification-item {
  display: flex;
  padding: 12px 15px;
  border-bottom: 1px solid #f0f0f0;
  cursor: pointer;
  transition: background-color 0.2s;
}

.notification-item:hover {
  background-color: #f9f9f9;
}

.notification-item.unread {
  background-color: #f0f7ff;
}

.notification-item.unread:hover {
  background-color: #e1f0ff;
}

.notification-icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-color: #e1e1e1;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  flex-shrink: 0;
}

.notification-icon.event {
  background-color: #e1f5fe;
  color: #039be5;
}

.notification-icon.announcement {
  background-color: #fff8e1;
  color: #ffa000;
}

.notification-icon.invite {
  background-color: #e8f5e9;
  color: #43a047;
}

.notification-icon.message {
  background-color: #e0f2f1;
  color: #00897b;
}

.notification-icon.alert {
  background-color: #ffebee;
  color: #e53935;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-content h4 {
  margin: 0 0 5px 0;
  font-size: 0.9rem;
  font-weight: 600;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.notification-content p {
  margin: 0 0 5px 0;
  font-size: 0.8rem;
  color: #666;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.notification-time {
  font-size: 0.7rem;
  color: #999;
}

.notification-actions {
  display: flex;
  align-items: center;
  margin-left: 10px;
}

.action-button {
  background: none;
  border: none;
  color: #bbb;
  cursor: pointer;
  padding: 5px;
  margin-left: 5px;
  border-radius: 50%;
  width: 25px;
  height: 25px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.action-button:hover {
  background-color: #f0f0f0;
  color: #3498db;
}

.action-button.delete:hover {
  color: #e74c3c;
}

.loading, .error, .empty-state {
  padding: 25px;
  text-align: center;
  color: #666;
}

.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid rgba(0, 0, 0, 0.1);
  border-top-color: #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 10px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error p {
  margin-bottom: 15px;
  color: #e74c3c;
}

.retry-button {
  background: #f8f9fa;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 5px 12px;
  font-size: 0.8rem;
  cursor: pointer;
}

.retry-button:hover {
  background: #e9ecef;
}

.empty-state i {
  font-size: 2rem;
  color: #3498db;
  margin-bottom: 10px;
}

.dropdown-footer {
  padding: 12px 15px;
  text-align: center;
  border-top: 1px solid #eee;
}

.dropdown-footer a {
  color: #3498db;
  font-size: 0.85rem;
  text-decoration: none;
}

.dropdown-footer a:hover {
  text-decoration: underline;
}

/* Responsive styles */
@media (max-width: 480px) {
  .dropdown-menu {
    width: 280px;
    right: -70px; /* Adjust this as needed to center the dropdown */
  }
  
  .dropdown-menu::before {
    /* Arrow to indicate where the dropdown is coming from */
    content: '';
    position: absolute;
    top: -5px;
    right: 80px; /* Adjust this to align with the bell icon */
    width: 10px;
    height: 10px;
    background: white;
    transform: rotate(45deg);
    box-shadow: -3px -3px 5px rgba(0, 0, 0, 0.04);
  }
}
</style> 