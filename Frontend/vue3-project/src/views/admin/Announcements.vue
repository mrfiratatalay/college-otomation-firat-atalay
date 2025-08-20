<template>
  <div class="admin-announcements">
    <div class="page-header">
      <h1>Duyurular</h1>
      <button @click="showAddModal = true" class="add-btn">
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
          <option v-for="community in communities" :key="community.clubId" :value="community.clubId">
            {{ community.name }}
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
      <button @click="fetchAnnouncements" class="retry-btn">
        <i class="fas fa-sync"></i> Tekrar Dene
      </button>
    </div>
    
    <div v-if="!loading && !error && announcements.length === 0" class="empty-state">
      <p>Hiç duyuru bulunamadı.</p>
      <button @click="showAddModal = true" class="add-btn">
        <i class="fas fa-plus"></i> Yeni Duyuru Oluştur
      </button>
    </div>
    
    <div v-if="!loading && announcements.length > 0" class="announcements-grid">
      <div v-for="announcement in filteredAnnouncements" :key="announcement.id" class="announcement-card"
        @click="viewAnnouncement(announcement.id)">

        <div class="announcement-header">
          <div class="announcement-meta">
            <span class="community">
              <i class="fas fa-users"></i>
              {{ getCommunityName(announcement.communityId) }}
            </span>
            <span class="type">
              <i class="fas fa-tag"></i>
              {{ getAnnouncementTypeName(announcement.typeId) }}
            </span>
            <span class="date">
              <i class="fas fa-calendar"></i>
              {{ formatDate(announcement.date) }}
            </span>
          </div>

        </div>
        <div class="announcement-content">
          <h3>{{ announcement.title }}</h3>
          <p class="description">{{ announcement.description }}</p>
          <div class="announcement-stats">
            <span>
              <i class="fas fa-eye"></i>
              {{ announcement.views }} Görüntülenme
            </span>
            <span>
              <i class="fas fa-comment"></i>
              {{ announcement.comments }} Yorum
            </span>
          </div>
          <div class="announcement-actions">
            <button @click.stop="editAnnouncement(announcement)" class="edit-btn">
              <i class="fas fa-edit"></i>
              Düzenle
            </button>
            <button @click.stop="deleteAnnouncement(announcement.id)" class="delete-btn">
              <i class="fas fa-trash"></i>
              Sil
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Duyuru Ekleme/Düzenleme Modalı -->
    <div v-if="showAddModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ editingAnnouncement ? 'Duyuru Düzenle' : 'Yeni Duyuru' }}</h2>
          <button @click="closeModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveAnnouncement">
            <div class="form-group">
              <label>Duyuru Başlığı</label>
              <input v-model="announcementForm.title" type="text" required>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Topluluk</label>
                <select v-model="announcementForm.communityId" required>
                  <option :value="null" disabled selected>Topluluk seçin</option>
                  <option v-for="community in communities" :key="community.clubId" :value="community.clubId">
                    {{ community.name }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>Duyuru Tipi</label>
                <select v-model="announcementForm.typeId" required>
                  <option :value="null" disabled selected>Duyuru tipi seçin</option>
                  <option v-for="type in announcementTypes" :key="type.id" :value="type.id">
                    {{ type.name }}
                  </option>
                </select>
              </div>
            </div>
            <div class="form-group">
              <label>İçerik</label>
              <textarea v-model="announcementForm.content" rows="6" required></textarea>
            </div>
            <div class="form-group">
              <label>Kısa Açıklama</label>
              <textarea v-model="announcementForm.description" rows="3" required></textarea>
            </div>
            <div class="form-group">
              <label>Duyuru Görseli</label>
              <div class="image-upload">
                <input type="file" @change="handleImageUpload" accept="image/*">
                <div v-if="announcementForm.imagePreview" class="upload-preview">
                  <img :src="announcementForm.imagePreview" alt="Preview">
                  <button type="button" @click="removeImage" class="remove-image">
                    <i class="fas fa-times"></i>
                  </button>
                </div>
              </div>
            </div>
            <div class="form-group">
              <label>Ekler</label>
              <div class="attachments">
                <div v-for="(file, index) in announcementForm.attachments" :key="index" class="attachment-item">
                  <span>{{ file.name }}</span>
                  <button type="button" @click="removeAttachment(index)" class="remove-attachment">
                    <i class="fas fa-times"></i>
                  </button>
                </div>
                <input type="file" @change="handleAttachmentUpload" multiple>
              </div>
            </div>
            <div class="form-actions">
              <button type="button" @click="closeModal" class="cancel-btn">İptal</button>
              <button type="submit" class="save-btn">Kaydet</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { announcementService, clubService } from '@/services';

export default {
  name: 'AdminAnnouncements',
  data() {
    return {
      searchQuery: '',
      communityFilter: 'all',
      showAddModal: false,
      editingAnnouncement: null,
      loading: false,
      error: null,
      communities: [],
      announcementTypes: [
        { id: 1, name: 'Genel' },
        { id: 2, name: 'Etkinlik' },
        { id: 3, name: 'Önemli' },
        { id: 4, name: 'Bilgilendirme' }
      ],
      announcementForm: {
        title: '',
        communityId: null,
        typeId: null,
        content: '',
        description: '',
        image: null,
        imagePreview: null,
        attachments: []
      },
      announcements: []
    }
  },
  created() {
    // Component oluşturulduğunda toplulukları ve duyuruları getir
    this.fetchCommunities();
    this.fetchAnnouncements();
  },
  computed: {
    filteredAnnouncements() {
      let filtered = this.announcements;

      // Arama filtresi
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(announcement =>
          announcement.title.toLowerCase().includes(query) ||
          announcement.content.toLowerCase().includes(query)
        );
      }

      // Topluluk filtresi
      if (this.communityFilter !== 'all') {
        const communityId = parseInt(this.communityFilter);
        filtered = filtered.filter(announcement => announcement.communityId === communityId);
      }

      return filtered;
    }
  },
  methods: {
    getCommunityName(communityId) {
      const community = this.communities.find(c => c.clubId === communityId);
      return community ? community.name : 'Bilinmeyen Topluluk';
    },
    getAnnouncementTypeName(typeId) {
      const type = this.announcementTypes.find(t => t.id === typeId);
      return type ? type.name : 'Genel';
    },
    async fetchCommunities() {
      try {
        // Backend API'den topluluk verilerini al
        const response = await clubService.getAllClubs();
        // Map response data properly with correct field names
        this.communities = (response.data || []).map(club => ({
          clubId: club.clubId,
          name: club.name
        }));
        console.log('Loaded communities:', this.communities);
      } catch (error) {
        console.error('Topluluklar alınırken hata:', error);
        // Hata durumunda test verilerini göster (sadece geliştirme aşamasında)
        this.communities = [
          { clubId: 1, name: 'Teknoloji Topluluğu' },
          { clubId: 2, name: 'Fotoğrafçılık Kulübü' },
          { clubId: 3, name: 'Kitap Okuma Kulübü' }
        ];
      }
    },
    async fetchAnnouncements() {
      try {
        this.loading = true;
        this.error = null;
        
        // Backend API'den duyuru verilerini al
        const response = await announcementService.getAllAnnouncements();
        
        if (response && response.data) {
          this.announcements = response.data.map(announcement => ({
            id: announcement.announcementId || announcement.id,
            title: announcement.title,
            communityId: announcement.clubId || announcement.communityId,
            typeId: announcement.typeId || 1,
            content: announcement.content,
            description: announcement.description || announcement.content.substring(0, 100),
            image: announcement.imageUrl || '/images/default-announcement.jpg',
            date: announcement.createdAt ? announcement.createdAt.substring(0, 10) : new Date().toISOString().substring(0, 10),
            views: announcement.viewCount || 0,
            comments: announcement.commentCount || 0
          }));
          console.log('Mapped announcements:', this.announcements);
        } else {
          // API veri dönmezse boş dizi kullan
          this.announcements = [];
          console.warn('API\'den duyuru verisi alınamadı');
        }
      } catch (error) {
        console.error('Duyurular yüklenirken hata:', error);
        this.error = 'Duyurular yüklenirken bir hata oluştu.';
        // Hata durumunda duyuruları temizle
        this.announcements = [];
      } finally {
        this.loading = false;
      }
    },
    getStatusText(status) {
      const statusMap = {
        active: 'Aktif',
        inactive: 'Pasif',
        draft: 'Taslak'
      };
      return statusMap[status] || status;
    },
    formatDate(dateString) {
      const options = { year: 'numeric', month: 'long', day: 'numeric' };
      return new Date(dateString).toLocaleDateString('tr-TR', options);
    },
    filterAnnouncements() {
      // Filtreleme işlemi computed property üzerinden yapılıyor
    },
    editAnnouncement(announcement) {
      console.log('Editing announcement:', announcement);
      if (!announcement.id) {
        console.error('Uyarı: Düzenlenecek duyurunun ID bilgisi yok!', announcement);
        alert('Duyuru düzenlenemedi, ID bilgisi eksik.');
        return;
      }
      
      this.editingAnnouncement = announcement;
      this.announcementForm = { 
        id: announcement.id,
        title: announcement.title,
        communityId: announcement.communityId,
        typeId: announcement.typeId,
        content: announcement.content,
        description: announcement.description,
        image: announcement.image,
        imagePreview: announcement.imagePreview,
        attachments: announcement.attachments || []
      };
      this.showAddModal = true;
    },
    async deleteAnnouncement(id) {
      if (confirm('Bu duyuruyu silmek istediğinizden emin misiniz?')) {
        try {
          // Backend API'ye silme isteği gönder
          await announcementService.deleteAnnouncement(id);
          
          // UI güncellemesi
          this.announcements = this.announcements.filter(a => a.id !== id);
        } catch (error) {
          console.error('Duyuru silinirken hata:', error);
          alert('Duyuru silinirken bir hata oluştu.');
        }
      }
    },
    handleImageUpload(event) {
      const file = event.target.files[0];
      if (file) {
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
    handleAttachmentUpload(event) {
      const files = Array.from(event.target.files);
      // Dosya bilgilerini sakla
      this.announcementForm.attachments.push(...files);
    },
    removeAttachment(index) {
      this.announcementForm.attachments.splice(index, 1);
    },
    async saveAnnouncement() {
      try {
        const formData = {
          title: this.announcementForm.title,
          content: this.announcementForm.content,
          clubId: this.announcementForm.communityId,
          imageUrl: this.announcementForm.image
        };

        if (this.editingAnnouncement) {
          await announcementService.updateAnnouncement(this.editingAnnouncement.id, formData);
        } else {
          await announcementService.createAnnouncement(formData);
        }

        this.closeModal();
        this.fetchAnnouncements();
      } catch (error) {
        console.error('Duyuru kaydedilirken hata oluştu:', error);
        this.error = 'Duyuru kaydedilirken bir hata oluştu.';
      }
    },
    closeModal() {
      this.showAddModal = false;
      this.editingAnnouncement = null;
      this.announcementForm = {
        title: '',
        communityId: null,
        typeId: null,
        content: '',
        description: '',
        image: null,
        imagePreview: null,
        attachments: []
      };
    },
    async viewAnnouncement(id) {
      try {
        // Duyuru görüntüleme işlemini API'ye bildir (izleme/okunma sayısını artırmak için)
        await announcementService.getAnnouncementById(id);
        
        // Duyuru kartına tıklandığında duyuru detay sayfasına yönlendirme
        this.$router.push(`/admin/announcements/detail/${id}`);
      } catch (error) {
        console.error('Duyuru görüntülenirken hata:', error);
      }
    }

  }
}
</script>

<style scoped>
.admin-announcements {
  padding: 1rem;
}

/* Sayfa Başlığı */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  color: #2c3e50;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: #2ecc71;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.add-btn:hover {
  background: #27ae60;
}

/* Filtreler */
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

/* Duyuru Kartları */
.announcements-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1.5rem;
}

.announcement-card {
  display: flex;
  flex-direction: column;
  background: linear-gradient(135deg, #ffffff, #f2f2f2);
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s, box-shadow 0.3s;
  position: relative;
  padding: 1.5rem;
  cursor: pointer;
}

.announcement-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
}

/* Sol taraftaki yan çizgi yerine ince gradient border */
.announcement-card::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 5px;
  height: 100%;
  background: linear-gradient(180deg, #2ecc71, #3498db);
}

/* Loading States */
.loading-indicator,
.error-message,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: #f8f9fa;
  border-radius: 8px;
  margin: 1rem 0;
  min-height: 200px;
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
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  color: #e74c3c;
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

.retry-btn:hover {
  background: #2980b9;
}

/* Başlık Alanı */
.announcement-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.announcement-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.85rem;
  color: #7f8c8d;
}

/* Duyuru İçerik Alanı */
.announcement-content h3 {
  margin: 0 0 0.5rem;
  color: #2c3e50;
  font-size: 1.25rem;
  font-weight: 600;
}

.description {
  color: #444;
  line-height: 1.5;
  margin-bottom: 1rem;
}

/* Görüntülenme & Yorum Bilgileri */
.announcement-stats {
  display: flex;
  gap: 1rem;
  font-size: 0.85rem;
  color: #7f8c8d;
  margin-bottom: 1rem;
}

/* Eylem Düğmeleri */
.announcement-actions {
  display: flex;
  gap: 0.75rem;
}

.edit-btn,
.delete-btn {
  flex: 1;
  padding: 0.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.5rem;
  transition: background-color 0.3s;
}

.edit-btn {
  background: #3498db;
  color: white;
}

.edit-btn:hover {
  background: #2980b9;
}

.delete-btn {
  background: #e74c3c;
  color: white;
}

.delete-btn:hover {
  background: #c0392b;
}

/* Modal Stilleri */
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
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #7f8c8d;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.image-upload {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.upload-preview {
  width: 100%;
  height: 200px;
  border-radius: 4px;
  overflow: hidden;
}

.upload-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.attachments {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.attachment-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.remove-attachment {
  background: none;
  border: none;
  color: #e74c3c;
  cursor: pointer;
  padding: 0.25rem;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1.5rem;
}

.cancel-btn,
.save-btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
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
}

/* Add Responsive Styles */
@media (max-width: 992px) {
  .announcements-grid {
    /* Adjust minmax based on card content */
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); 
  }
}

@media (max-width: 768px) {
  .admin-announcements {
    padding: 15px;
  }
  .page-header {
    flex-direction: column; /* Stack title and button */
    align-items: flex-start;
    gap: 10px;
    margin-bottom: 15px;
  }
  .filters {
    flex-direction: column; /* Stack filter groups */
    gap: 10px;
  }
  .filter-group {
    flex-direction: column; /* Stack select dropdowns */
    width: 100%; /* Make selects full width */
    gap: 10px;
  }
  .search-box {
    width: 100%; /* Make search full width */
  }
  .announcements-grid {
    grid-template-columns: 1fr; /* Single column grid */
    gap: 15px;
  }
  .announcement-card {
    /* Optional: Adjust card padding/styles */
  }
  .announcement-meta {
    flex-direction: column; /* Stack meta items */
    align-items: flex-start;
    gap: 5px;
    margin-bottom: 10px;
  }
  .announcement-stats {
    flex-direction: column; /* Stack stats */
    align-items: flex-start;
    gap: 5px;
  }
  .modal-content {
    max-width: 95%;
    max-height: 90vh;
  }
  .modal-body {
    max-height: calc(85vh - 120px); /* Adjust based on header/footer */
    overflow-y: auto; /* Ensure modal body scrolls */
  }
  .form-row {
    flex-direction: column; /* Stack form fields in rows */
    gap: 0;
  }
  .form-row .form-group {
     width: 100%;
  }
}

@media (max-width: 480px) {
  .announcement-actions {
    flex-direction: column; /* Stack buttons */
    gap: 10px;
    margin-top: 10px;
  }
  .announcement-actions button {
    width: 100%; /* Make buttons full width */
  }
  .form-actions {
    flex-direction: column; /* Stack modal buttons */
    gap: 10px;
  }
   .form-actions button {
    width: 100%;
   }
}
</style>