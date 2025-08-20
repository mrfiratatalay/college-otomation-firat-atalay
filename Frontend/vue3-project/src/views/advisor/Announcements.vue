<template>
  <div class="advisor-announcements">
    <div class="page-header">
      <h1>Duyurular</h1>
      <button @click="openNewAnnouncementModal" class="add-btn">
        <i class="fas fa-plus"></i>
        Yeni Duyuru
      </button>
    </div>

    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>
        <input type="text" v-model="searchQuery" placeholder="Duyuru ara..." @input="filterAnnouncements">
      </div>
      <div class="filter-group">
        <select v-model="communityFilter" @change="filterAnnouncements">
          <option value="all">Tüm Topluluklar</option>
          <option v-for="club in advisorClubs" :key="club.clubId" :value="club.clubId">
            {{ club.name }}
          </option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Duyurular yükleniyor...</p>
    </div>
    
    <div v-if="error" class="error-message">
      <p>{{ error }}</p>
      <button @click="loadAnnouncements" class="retry-btn">
        <i class="fas fa-sync"></i> Tekrar Dene
      </button>
    </div>
    
    <div v-if="!loading && !error && filteredAnnouncements.length === 0" class="empty-state">
      <p>Hiç duyuru bulunamadı.</p>
      <button @click="openNewAnnouncementModal" class="add-btn">
        <i class="fas fa-plus"></i> Yeni Duyuru Oluştur
      </button>
    </div>

    <div v-if="!loading && !error && filteredAnnouncements.length > 0" class="announcements-grid">
      <div v-for="announcement in filteredAnnouncements" :key="announcement.announcementId" class="announcement-card">
        <div class="announcement-image" v-if="announcement.imageUrl">
          <img 
            :src="getImageUrl(announcement.imageUrl)" 
            :alt="announcement.title"
            @error="handleImageError"
            loading="lazy"
          >
        </div>
        <div class="announcement-content">
          <div class="announcement-header">
            <div class="announcement-meta">
              <span class="community">
                <i class="fas fa-users"></i>
                {{ getClubName(announcement.clubId) }}
              </span>
              <span class="date">
                <i class="fas fa-calendar"></i>
                {{ formatDate(announcement.createdAt) }}
              </span>
            </div>
          </div>
          <h3 class="announcement-title">{{ announcement.title }}</h3>
          <p class="announcement-text">{{ announcement.content }}</p>
          <div class="announcement-actions">
            <button @click="editAnnouncement(announcement)" class="edit-btn">
              <i class="fas fa-edit"></i>
              Düzenle
            </button>
            <button @click="deleteAnnouncement(announcement.announcementId)" class="delete-btn">
              <i class="fas fa-trash"></i>
              Sil
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Yeni/Düzenle Duyuru Modal -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ editingAnnouncement ? 'Duyuru Düzenle' : 'Yeni Duyuru' }}</h2>
          <button @click="closeModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveAnnouncement">
            <div class="form-group">
              <label>Kulüp:</label>
              <select v-model="announcementForm.clubId" required :disabled="!!editingAnnouncement">
                <option v-for="club in advisorClubs" :key="club.clubId" :value="club.clubId">
                  {{ club.name }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Başlık:</label>
              <input type="text" v-model="announcementForm.title" required>
            </div>
            <div class="form-group">
              <label>İçerik:</label>
              <textarea v-model="announcementForm.content" required rows="6"></textarea>
            </div>
            <div class="form-group">
              <label>Topluluk Fotoğrafı</label>
              <div class="image-upload-container">
                <div class="image-upload-box" @click="triggerFileInput">
                  <input 
                    type="file" 
                    ref="imageInput" 
                    @change="handleImageChange" 
                    accept="image/*" 
                    style="display: none;"
                  >
                  <template v-if="!announcementForm.imagePreview">
                    <div class="upload-icon">
                      <i class="fas fa-cloud-upload-alt"></i>
                    </div>
                    <div class="upload-text">
                      Görsel yüklemek için tıklayın veya sürükleyin
                    </div>
                  </template>
                  <div v-if="announcementForm.imagePreview" class="preview-container">
                    <img :src="announcementForm.imagePreview" alt="Duyuru Görseli">
                    <button class="remove-image" @click.stop="removeImage">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            <div class="form-actions">
              <button type="button" @click="closeModal" class="cancel-btn">İptal</button>
              <button type="submit" class="save-btn" :disabled="isSubmitting">
                {{ isSubmitting ? 'Kaydediliyor...' : 'Kaydet' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { announcementService, clubService } from '@/services';
import authService from '@/services/authService';

export default {
  name: 'AdvisorAnnouncements',
  data() {
    return {
      announcements: [],
      filteredAnnouncements: [],
      searchQuery: '',
      communityFilter: 'all',
      loading: false,
      error: null,
      showModal: false,
      isSubmitting: false,
      editingAnnouncement: null,
      advisorClubs: [],
      currentUser: null,
      announcementForm: {
        clubId: '',
        title: '',
        content: '',
        image: null,
        imagePreview: null
      },
      clubNames: {}
    }
  },
  async created() {
    this.currentUser = authService.getUser();
    if (!this.currentUser) {
      this.$router.push('/login');
      return;
    }
    await this.loadAdvisorClubs();
    await this.loadAnnouncements();
  },
  methods: {
    async loadAdvisorClubs() {
      try {
        // Kullanıcı bilgilerini al
        const user = authService.getUser();
        if (!user || user.userType !== 'advisor') {
          console.error('Kullanıcı danışman değil veya oturum açılmamış.');
          return;
        }
        
        // Özel API endpointi ile danışmanın kulüplerini doğrudan getirelim
        const response = await clubService.getAdvisorMyClubs();
        
        if (response && response.data && Array.isArray(response.data)) {
          this.advisorClubs = response.data.map(club => ({
            clubId: club.clubId,
            name: club.name,
            status: club.status || 'active',
            memberCount: club.memberCount || 0,
            eventCount: club.eventCount || 0
          }));
            
          console.log('Danışman kulüpleri yüklendi:', this.advisorClubs);
            
          // Eğer danışmanın kulüpleri varsa, ilk kulübü seç
          if (this.advisorClubs.length > 0) {
            this.announcementForm.clubId = this.advisorClubs[0].clubId;
          }
        } else {
          console.warn('Kulüp verisi beklenen formatta değil');
          this.advisorClubs = [];
        }
      } catch (error) {
        console.error('Danışman kulüpleri yüklenirken hata:', error);
      }
    },
    async loadAnnouncements() {
      try {
        this.loading = true;
        this.error = null;
        const response = await announcementService.getAdvisorAnnouncements();
        
        if (response.data) {
          this.announcements = response.data.sort((a, b) => 
            new Date(b.createdAt) - new Date(a.createdAt)
          );
          this.filterAnnouncements();
        } else {
          this.announcements = [];
        }
      } catch (error) {
        console.error('Duyurular yüklenirken hata:', error);
        this.error = 'Duyurular yüklenirken bir hata oluştu.';
      } finally {
        this.loading = false;
      }
    },
    filterAnnouncements() {
      let filtered = [...this.announcements];
      
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(announcement =>
          announcement.title.toLowerCase().includes(query) ||
          announcement.content.toLowerCase().includes(query)
        );
      }

      if (this.communityFilter !== 'all') {
        filtered = filtered.filter(announcement => 
          announcement.clubId === this.communityFilter
        );
      }

      this.filteredAnnouncements = filtered;
    },
    async getClubName(clubId) {
      if (this.clubNames[clubId]) {
        return this.clubNames[clubId];
      }

      try {
        const response = await clubService.getClubById(clubId);
        if (response.data) {
          this.clubNames[clubId] = response.data.name;
          return response.data.name;
        }
      } catch (error) {
        console.error('Kulüp bilgisi alınırken hata:', error);
      }

      return 'Bilinmeyen Kulüp';
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('tr-TR');
    },
    openNewAnnouncementModal() {
      this.editingAnnouncement = null;
      this.announcementForm = {
        clubId: this.advisorClubs.length > 0 ? this.advisorClubs[0].clubId : '',
        title: '',
        content: '',
        image: null,
        imagePreview: null
      };
      this.showModal = true;
    },
    editAnnouncement(announcement) {
      const announcementId = announcement.id || announcement.announcementId || announcement._id;
      
      if (!announcementId) {
        console.error('Uyarı: Düzenlenecek duyurunun ID bilgisi yok!', announcement);
        alert('Duyuru düzenlenemedi, ID bilgisi eksik.');
        return;
      }
      
      const clubId = announcement.clubId?.toString();
      
      this.editingAnnouncement = announcement;
      this.announcementForm = {
        id: announcementId,
        clubId: clubId,
        title: announcement.title,
        content: announcement.content,
        image: announcement.imageUrl || '',
        imagePreview: announcement.imageUrl || ''
      };

      this.showModal = true;
    },
    triggerFileInput() {
      this.$refs.imageInput.click();
    },
    handleImageChange(event) {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5MB limit
          alert('Dosya boyutu çok büyük. Lütfen 5MB\'dan küçük bir dosya seçin.');
          return;
        }
        
        const reader = new FileReader();
        reader.onload = (e) => {
          this.announcementForm.imagePreview = e.target.result;
          this.announcementForm.image = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    },
    removeImage() {
      this.announcementForm.image = null;
      this.announcementForm.imagePreview = null;
    },
    async saveAnnouncement() {
      try {
        this.isSubmitting = true;
        
        if (!this.announcementForm.clubId) {
          alert('Lütfen bir kulüp seçin.');
          return;
        }

        const formData = {
          title: this.announcementForm.title,
          content: this.announcementForm.content,
          clubId: this.announcementForm.clubId,
          imageUrl: this.announcementForm.image
        };

        if (this.editingAnnouncement) {
          const announcementId = this.editingAnnouncement.id || this.editingAnnouncement.announcementId || this.editingAnnouncement._id;
          await announcementService.updateAnnouncement(announcementId, formData);
          
          const index = this.announcements.findIndex(a => {
            const currentId = a.id || a.announcementId || a._id;
            return currentId === announcementId;
          });
          
          if (index !== -1) {
            this.announcements[index] = {
              ...this.announcements[index],
              ...formData,
              updatedAt: new Date().toISOString()
            };
          }
        } else {
          const response = await announcementService.createAnnouncement(formData);
          if (response.data) {
            this.announcements.unshift(response.data);
          }
        }

        this.closeModal();
        this.filterAnnouncements();
      } catch (error) {
        console.error('Duyuru kaydedilirken hata:', error);
        alert('Duyuru kaydedilirken bir hata oluştu: ' + (error.response?.data?.message || error.message));
      } finally {
        this.isSubmitting = false;
      }
    },
    async deleteAnnouncement(announcementId) {
      if (!announcementId) {
        console.error('Silinecek duyuru ID\'si bulunamadı!');
        alert('Duyuru silinemedi: ID bilgisi eksik');
        return;
      }

      console.log('Silinecek duyuru ID:', announcementId); // Debug için ID'yi logla

      if (confirm('Bu duyuruyu silmek istediğinizden emin misiniz?')) {
        try {
          this.loading = true;
          await announcementService.deleteAnnouncement(announcementId);
          
          // UI'dan duyuruyu kaldır
          this.announcements = this.announcements.filter(announcement => 
            announcement.announcementId !== announcementId
          );
          
          // Filtrelenmiş listeyi güncelle
          this.filterAnnouncements();
          
          // Başarı mesajı göster
          alert('Duyuru başarıyla silindi.');
        } catch (error) {
          console.error('Duyuru silinirken hata:', error);
          alert('Duyuru silinirken bir hata oluştu: ' + (error.response?.data?.message || error.message));
        } finally {
          this.loading = false;
        }
      }
    },
    closeModal() {
      this.showModal = false;
      this.editingAnnouncement = null;
      this.announcementForm = {
        clubId: '',
        title: '',
        content: '',
        image: null,
        imagePreview: null
      };
    },
    getClubNameAndUpdate(announcement) {
      if (this.clubNames[announcement.clubId]) {
        return this.clubNames[announcement.clubId];
      }

      if (!announcement.loadingClubName) {
        announcement.loadingClubName = true;
        this.getClubName(announcement.clubId).then(name => {
          this.$set(announcement, 'clubName', name);
          announcement.loadingClubName = false;
        });
      }

      return 'Kulüp bilgisi yükleniyor...';
    },
    getImageUrl(imageUrl) {
      if (!imageUrl) return '/default-announcement.png';
      if (imageUrl.startsWith('http')) return imageUrl;
      return `https://localhost:7274${imageUrl}`;
    },
    handleImageError(event) {
      event.target.src = '/default-announcement.png';
    }
  }
}
</script>

<style scoped>
.advisor-announcements {
  padding: 2rem;
  background-color: #f8f9fa;
  min-height: 100vh;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.page-header h1 {
  font-size: 2rem;
  color: #2c3e50;
  margin: 0;
  font-weight: 600;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: #2ecc71;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.add-btn:hover {
  background: #27ae60;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(46, 204, 113, 0.2);
}

.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  background: white;
  padding: 1rem;
  border-radius: 12px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.search-box {
  position: relative;
  flex: 1;
  max-width: 400px;
}

.search-box input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #e1e8ed;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.3s ease;
}

.search-box input:focus {
  border-color: #2ecc71;
  box-shadow: 0 0 0 3px rgba(46, 204, 113, 0.1);
  outline: none;
}

.search-box i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #7f8c8d;
}

.filter-group select {
  padding: 0.75rem 2rem;
  border: 1px solid #e1e8ed;
  border-radius: 8px;
  background-color: white;
  color: #2c3e50;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-group select:focus {
  border-color: #2ecc71;
  box-shadow: 0 0 0 3px rgba(46, 204, 113, 0.1);
  outline: none;
}

.announcements-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
  padding: 0.5rem;
}

.announcement-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
}

.announcement-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.1);
}

.announcement-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
}

.announcement-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.announcement-card:hover .announcement-image img {
  transform: scale(1.05);
}

.announcement-content {
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.announcement-header {
  margin-bottom: 1rem;
}

.announcement-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #7f8c8d;
  margin-bottom: 0.5rem;
}

.announcement-meta span {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.announcement-title {
  font-size: 1.25rem;
  color: #2c3e50;
  margin: 0 0 1rem 0;
  font-weight: 600;
  line-height: 1.4;
}

.announcement-text {
  color: #505c66;
  margin: 0 0 1.5rem 0;
  line-height: 1.6;
  flex: 1;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.announcement-actions {
  display: flex;
  gap: 1rem;
  margin-top: auto;
}

.edit-btn,
.delete-btn {
  flex: 1;
  padding: 0.75rem;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  transition: all 0.3s ease;
}

.edit-btn {
  background: #3498db;
  color: white;
}

.edit-btn:hover {
  background: #2980b9;
  transform: translateY(-1px);
}

.delete-btn {
  background: #e74c3c;
  color: white;
}

.delete-btn:hover {
  background: #c0392b;
  transform: translateY(-1px);
}

.loading-indicator,
.error-message,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(46, 204, 113, 0.1);
  border-radius: 50%;
  border-top: 4px solid #2ecc71;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.retry-btn {
  background: #3498db;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  margin-top: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

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
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #e1e8ed;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #7f8c8d;
  transition: color 0.3s ease;
}

.close-btn:hover {
  color: #34495e;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 500;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e1e8ed;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  border-color: #2ecc71;
  box-shadow: 0 0 0 3px rgba(46, 204, 113, 0.1);
  outline: none;
}

.image-upload-container {
  border: 2px dashed #e1e8ed;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.image-upload-container:hover {
  border-color: #2ecc71;
  background: rgba(46, 204, 113, 0.05);
}

.upload-icon {
  font-size: 2.5rem;
  color: #7f8c8d;
  margin-bottom: 1rem;
}

.upload-text {
  color: #7f8c8d;
  font-size: 0.95rem;
}

.preview-container {
  position: relative;
  margin-top: 1rem;
}

.preview-container img {
  max-width: 100%;
  border-radius: 8px;
}

.remove-image {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: rgba(231, 76, 60, 0.9);
  color: white;
  border: none;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.remove-image:hover {
  background: #e74c3c;
  transform: scale(1.1);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}

.cancel-btn,
.save-btn {
  padding: 0.75rem 2rem;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s ease;
}

.cancel-btn {
  background: #f8f9fa;
  color: #7f8c8d;
}

.cancel-btn:hover {
  background: #e9ecef;
}

.save-btn {
  background: #2ecc71;
  color: white;
}

.save-btn:hover {
  background: #27ae60;
  transform: translateY(-1px);
}

.save-btn:disabled {
  background: #95a5a6;
  cursor: not-allowed;
  transform: none;
}

@media (max-width: 768px) {
  .advisor-announcements {
    padding: 1rem;
  }

  .filters {
    flex-direction: column;
  }

  .search-box {
    max-width: 100%;
  }

  .announcements-grid {
    grid-template-columns: 1fr;
  }

  .modal-content {
    width: 95%;
    margin: 1rem;
  }
}
</style>
