<template>
  <div class="announcement-detail container py-4">
    <button class="back-btn" @click="$router.back()">
      <i class="fas fa-arrow-left"></i> Geri Dön
    </button>

    <div v-if="loading" class="text-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Yükleniyor...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-else class="announcement-content">
      <div class="announcement-header">
        <img v-if="announcement.imageUrl" :src="getImageUrl(announcement.imageUrl)" :alt="announcement.title" class="announcement-image">
        <div class="announcement-title-section">
          <h1>{{ announcement.title }}</h1>
          <div class="announcement-meta">
            <span class="club-name" v-if="announcement.community">
              <i class="fas fa-users"></i> {{ announcement.community }}
            </span>
            <span class="type-badge" :class="getTypeBadgeClass(announcement)">
              {{ announcement.type }}
            </span>
            <span class="date">
              <i class="fas fa-calendar"></i> {{ formatDate(announcement.date) }}
            </span>
          </div>
        </div>
      </div>

      <div class="announcement-body">
        <div class="content-section">
          <h3>Duyuru İçeriği</h3>
          <div class="content-text">
            {{ announcement.content }}
          </div>
        </div>

        <div class="info-section">
          <div class="info-grid">
            <div class="info-item">
              <i class="fas fa-eye"></i>
              <div>
                <strong>Görüntülenme</strong>
                <p>{{ announcement.views || 0 }}</p>
              </div>
            </div>
            <div class="info-item">
              <i class="fas fa-comment"></i>
              <div>
                <strong>Yorum Sayısı</strong>
                <p>{{ announcement.comments || 0 }}</p>
              </div>
            </div>
          </div>
        </div>

        <div v-if="announcement.attachments && announcement.attachments.length > 0" class="attachments-section">
          <h3>Ekler</h3>
          <div class="attachments-list">
            <div v-for="(attachment, index) in announcement.attachments" :key="index" class="attachment-item">
              <i class="fas fa-paperclip"></i>
              <span>{{ attachment.name }}</span>
              <a :href="attachment.url" download class="download-btn">
                <i class="fas fa-download"></i>
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { studentService } from '@/services';

export default {
  name: 'AnnouncementDetail',
  data() {
    return {
      announcement: {
        id: null,
        title: '',
        content: '',
        description: '',
        imageUrl: '',
        date: null,
        community: '',
        type: '',
        views: 0,
        comments: 0,
        attachments: []
      },
      loading: true,
      error: null
    }
  },
  async created() {
    try {
      const announcementId = this.$route.params.id;
      const response = await studentService.getAnnouncementDetails(announcementId);
      
      if (response.data) {
        this.announcement = {
          id: response.data.announcementId,
          title: response.data.title,
          content: response.data.content,
          description: response.data.description,
          imageUrl: response.data.imageUrl,
          date: response.data.createdAt,
          community: response.data.club?.name || 'Bilinmeyen Topluluk',
          type: this.getAnnouncementType(response.data.typeId || 1),
          views: response.data.views || 0,
          comments: response.data.comments || 0,
          attachments: response.data.attachments || []
        };
      }
    } catch (error) {
      this.error = 'Duyuru detayları yüklenirken bir hata oluştu: ' + error.message;
      console.error('Duyuru detayları yüklenirken hata:', error);
    } finally {
      this.loading = false;
    }
  },
  methods: {
    formatDate(dateString) {
      if (!dateString) return 'Tarih belirtilmemiş';
      
      try {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('tr-TR', {
          year: 'numeric',
          month: 'long',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        }).format(date);
      } catch (error) {
        console.error('Tarih formatlanırken hata:', error);
        return 'Tarih formatlanamadı';
      }
    },
    getAnnouncementType(typeId) {
      const types = {
        1: 'Genel',
        2: 'Etkinlik',
        3: 'Önemli',
        4: 'Bilgilendirme'
      };
      return types[typeId] || 'Genel';
    },
    getTypeBadgeClass(announcement) {
      const typeClasses = {
        'Genel': 'type-general',
        'Etkinlik': 'type-event',
        'Önemli': 'type-important',
        'Bilgilendirme': 'type-info'
      };
      return typeClasses[announcement.type] || 'type-general';
    },
    getImageUrl(imageUrl) {
      if (!imageUrl) return '/default-announcement.png';
      if (imageUrl.startsWith('http')) return imageUrl;
      return `https://localhost:7274${imageUrl}`;
    }
  }
}
</script>

<style scoped>
.announcement-detail {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem;
}

.back-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 2rem;
  padding: 0.5rem 1rem;
  background: none;
  border: none;
  color: #3498db;
  font-size: 1rem;
  cursor: pointer;
  transition: color 0.3s;
}

.back-btn:hover {
  color: #2980b9;
}

.announcement-header {
  margin-bottom: 2rem;
}

.announcement-image {
  width: 100%;
  max-height: 400px;
  object-fit: cover;
  border-radius: 12px;
  margin-bottom: 1.5rem;
}

.announcement-title-section h1 {
  margin-bottom: 1rem;
  color: #2c3e50;
}

.announcement-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
  margin-bottom: 1rem;
}

.type-badge {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  color: white;
  font-size: 0.9rem;
}

.type-general {
  background-color: #3498db;
}

.type-event {
  background-color: #2ecc71;
}

.type-important {
  background-color: #e74c3c;
}

.type-info {
  background-color: #f39c12;
}

.club-name, .date {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #7f8c8d;
}

.content-section {
  background: #f8f9fa;
  padding: 2rem;
  border-radius: 12px;
  margin-bottom: 2rem;
}

.content-text {
  color: #444;
  line-height: 1.6;
  white-space: pre-line;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-top: 1rem;
}

.info-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.info-item i {
  font-size: 1.5rem;
  color: #3498db;
}

.info-item strong {
  display: block;
  margin-bottom: 0.25rem;
  color: #2c3e50;
}

.info-item p {
  margin: 0;
  color: #7f8c8d;
  font-size: 1.25rem;
  font-weight: bold;
}

.attachments-section {
  margin-top: 2rem;
}

.attachments-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.attachment-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.attachment-item i {
  color: #3498db;
}

.download-btn {
  margin-left: auto;
  color: #3498db;
  cursor: pointer;
  transition: color 0.3s;
}

.download-btn:hover {
  color: #2980b9;
}

@media (max-width: 768px) {
  .announcement-meta {
    flex-direction: column;
    align-items: flex-start;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }
}
</style>
  