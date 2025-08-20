<template>
  <div class="admin-announcements">
    <div class="page-header">
      <h1>Duyurular</h1>
    </div>

    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>
        <input type="text" v-model="searchQuery" placeholder="Duyuru ara..." @input="filterAnnouncements">
      </div>
      <div class="filter-group">
        <select v-model="communityFilter" @change="filterAnnouncements">
          <option value="all">Tüm Topluluklar</option>
          <option v-for="community in communities" :key="community.id" :value="community.id">
            {{ community.name }}
          </option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading-container">
      <p>Yükleniyor...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p>{{ error }}</p>
      <button @click="fetchAnnouncements" class="btn btn-primary">Tekrar Dene</button>
    </div>

    <div v-else class="announcements-grid">
      <div 
        v-for="announcement in filteredAnnouncements" 
        :key="announcement.id" 
        class="announcement-card"
      >
        <div class="announcement-image" v-if="announcement.imageUrl">
          <img 
            :src="getImageUrl(announcement.imageUrl)" 
            :alt="announcement.title"
            @error="handleImageError"
            loading="lazy"
          >
        </div>
        <div class="announcement-header">
          <div class="announcement-meta">
            <span class="community">
              <i class="fas fa-users"></i> {{ announcement.community }}
            </span>
            <span class="type">
              <i class="fas fa-tag"></i> {{ announcement.type }}
            </span>
            <span class="date">
              <i class="fas fa-calendar"></i> {{ formatDate(announcement.date) }}
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
            <button @click="viewAnnouncement(announcement.id)" class="edit-btn">
              <i class="fas fa-info-circle"></i>
              Detaylar
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Duyuru Ekleme/Düzenleme Modalı -->
    <div v-if="showAddModal" class="modal">
      <div class="modal-content" @click.stop>
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
                <select v-model="announcementForm.community" required>
                  <option v-for="community in communities" :key="community.id" :value="community.id">
                    {{ community.name }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>Duyuru Tipi</label>
                <select v-model="announcementForm.type" required>
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
                <div class="upload-preview" v-if="announcementForm.image">
                  <img :src="announcementForm.image" alt="Preview">
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
import { studentService } from '@/services';
import { ref } from 'vue';

export default {
  name: 'StudentAnnouncements',
  data() {
    return {
      searchQuery: '',
      communityFilter: 'all',
      typeFilter: 'all',
      showAddModal: false,
      editingAnnouncement: null,
      communities: [],
      announcementTypes: [
        { id: 1, name: 'Genel' },
        { id: 2, name: 'Etkinlik' },
        { id: 3, name: 'Önemli' },
        { id: 4, name: 'Bilgilendirme' }
      ],
      announcementForm: {
        title: '',
        community: '',
        type: '',
        content: '',
        description: '',
        image: null,
        attachments: []
      },
      announcements: [],
      loading: true,
      error: null
    }
  },
  computed: {
    filteredAnnouncements() {
      let filtered = this.announcements

      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase()
        filtered = filtered.filter(announcement =>
          announcement.title.toLowerCase().includes(query) ||
          announcement.content.toLowerCase().includes(query)
        )
      }

      if (this.communityFilter !== 'all') {
        filtered = filtered.filter(announcement => announcement.community === this.communityFilter)
      }

      if (this.typeFilter !== 'all') {
        filtered = filtered.filter(announcement => announcement.type === this.typeFilter)
      }

      return filtered
    }
  },
  async created() {
    await this.fetchAnnouncements();
    await this.fetchCommunities();
  },
  methods: {
    async fetchAnnouncements() {
      try {
        this.loading = true;
        this.error = null;
        const response = await studentService.getAnnouncements();
        
        // Duyuruları işle ve resim URL'lerini düzenle
        this.announcements = response.data.map(announcement => ({
          id: announcement.announcementId,
          title: announcement.title,
          content: announcement.content,
          description: announcement.content.substring(0, 150) + '...',
          imageUrl: announcement.imageUrl,
          date: announcement.createdAt,
          community: announcement.club?.name || 'Bilinmeyen Topluluk',
          type: this.getAnnouncementType(announcement.typeId || 1),
          views: 0,
          comments: 0
        }));
      } catch (error) {
        this.error = 'Duyurular yüklenirken bir hata oluştu: ' + error.message;
        console.error('Duyurular yüklenirken hata:', error);
      } finally {
        this.loading = false;
      }
    },

    async fetchCommunities() {
      try {
        const response = await studentService.getClubs();
        this.communities = response.data || [];
      } catch (error) {
        console.error('Topluluklar yüklenirken hata:', error);
      }
    },

    formatDate(dateString) {
      if (!dateString) return 'Tarih belirtilmemiş';
      
      try {
        const date = new Date(dateString);
        if (isNaN(date.getTime())) {
          return 'Geçersiz tarih';
        }
        
        return new Intl.DateTimeFormat('tr-TR', {
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        }).format(date);
      } catch (error) {
        console.error('Tarih formatlanırken hata:', error);
        return 'Tarih formatlanamadı';
      }
    },

    filterAnnouncements() {
      // Filtreleme işlemi computed property üzerinden yapılıyor
    },

    editAnnouncement(announcement) {
      this.editingAnnouncement = announcement
      this.announcementForm = { ...announcement }
      this.showAddModal = true
    },

    deleteAnnouncement(id) {
      if (confirm('Bu duyuruyu silmek istediğinizden emin misiniz?')) {
        this.announcements = this.announcements.filter(a => a.id !== id)
        // API çağrısı burada yapılacak
      }
    },

    handleImageUpload(event) {
      const file = event.target.files[0]
      if (file) {
        const reader = new FileReader()
        reader.onload = e => {
          this.announcementForm.image = e.target.result
        }
        reader.readAsDataURL(file)
      }
    },

    handleAttachmentUpload(event) {
      const files = Array.from(event.target.files)
      this.announcementForm.attachments.push(...files)
    },

    removeAttachment(index) {
      this.announcementForm.attachments.splice(index, 1)
    },

    saveAnnouncement() {
      if (this.editingAnnouncement) {
        // Mevcut duyuruyu güncelle
        const index = this.announcements.findIndex(a => a.id === this.editingAnnouncement.id)
        if (index !== -1) {
          this.announcements[index] = { ...this.editingAnnouncement, ...this.announcementForm }
        }
      } else {
        // Yeni duyuru ekle
        const newAnnouncement = {
          id: this.announcements.length + 1,
          ...this.announcementForm,
          date: new Date().toISOString().split('T')[0],
          views: 0,
          comments: 0
        }
        this.announcements.push(newAnnouncement)
      }
      this.closeModal()
      // API çağrısı burada yapılacak
    },

    closeModal() {
      this.showAddModal = false
      this.editingAnnouncement = null
      this.announcementForm = {
        title: '',
        community: '',
        type: '',
        content: '',
        description: '',
        image: null,
        attachments: []
      }
    },

    viewAnnouncement(id) {
      this.$router.push(`/student/announcements/detail/${id}`);
    },

    getImageUrl(imageUrl) {
      if (!imageUrl) return '/default-announcement.png';
      if (imageUrl.startsWith('http')) return imageUrl;
      return `https://localhost:7274${imageUrl}`;
    },

    handleImageError(event) {
      event.target.src = '/default-announcement.png';
    },

    getAnnouncementType(typeId) {
      const type = this.announcementTypes.find(t => t.id === typeId);
      return type ? type.name : 'Genel';
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
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  padding: 1rem;
}
.announcement-card {
  background: #ffffff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  display: flex;
  flex-direction: column;
}
.announcement-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.announcement-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
  border-radius: 8px 8px 0 0;
  position: relative;
  background-color: #f5f5f5; /* Resim yüklenene kadar arka plan rengi */
}

.announcement-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
  background-color: #f5f5f5;
}

.announcement-image:hover img {
  transform: scale(1.05);
}

.announcement-header {
  padding: 1rem;
  border-bottom: 1px solid #eee;
}

.announcement-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #666;
}

.announcement-meta i {
  margin-right: 0.5rem;
}

.announcement-content {
  padding: 1rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.announcement-content h3 {
  margin: 0 0 0.5rem;
  font-size: 1.25rem;
  color: #333;
}

.description {
  color: #666;
  font-size: 0.95rem;
  line-height: 1.5;
  margin-bottom: 1rem;
  flex: 1;
}

.announcement-stats {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #888;
  margin-bottom: 1rem;
}

.announcement-actions {
  display: flex;
  gap: 0.5rem;
}

.edit-btn {
  flex: 1;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  background: #3498db;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: background-color 0.3s;
}

.edit-btn:hover {
  background: #2980b9;
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

@media (max-width: 768px) {
  .announcements-grid {
    grid-template-columns: 1fr;
  }
}
</style>