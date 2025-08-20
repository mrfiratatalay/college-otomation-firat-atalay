<template>
  <div class="announcement-detail container py-4">
    <button class="back-btn" @click="$router.back()">
      <i class="fas fa-arrow-left"></i> Geri Dön
    </button>
    <div v-if="loading" class="loading">Yükleniyor...</div>
    <div v-else-if="error" class="error">Hata: {{ error }}</div>
    <div v-else-if="announcement">
      <img v-if="announcement.imageUrl" :src="announcement.imageUrl" alt="Duyuru Görseli" class="detail-image mb-4">

      <div class="detail-header mb-4">
        <h1>{{ announcement.title }}</h1>
        <div class="detail-meta">
          <span v-if="announcement.club" class="community">
            <i class="fas fa-users"></i> {{ announcement.club.name }}
          </span>
          <span class="date">
            <i class="fas fa-calendar"></i> {{ formatDate(announcement.createdAt) }}
          </span>
          <span v-if="announcement.updatedAt" class="date">
             | Güncellenme: {{ formatDate(announcement.updatedAt) }}
          </span>
        </div>
      </div>
      <div class="detail-content">
        <div class="description" v-html="announcement.content"></div>
      </div>
    </div>
     <div v-else class="error">Duyuru bulunamadı.</div>
  </div>
</template>

<script>
import { announcementService } from '@/services'; // Servisi import et

export default {
  name: 'AdminAnnouncementDetail',
  data() {
    return {
      announcement: null, // Başlangıçta null
      loading: true,
      error: null
    }
  },
  async created() {
    await this.fetchAnnouncementDetails();
  },
  methods: {
    async fetchAnnouncementDetails() {
      const announcementId = this.$route.params.id;
      if (!announcementId) {
        this.error = 'Duyuru ID\'si bulunamadı.';
        this.loading = false;
        return;
      }

      this.loading = true;
      this.error = null;
      try {
        const response = await announcementService.getAnnouncementById(announcementId);
        this.announcement = response.data; // API'dan gelen veriyi ata
         // API base URL'sini resim URL'sinin başına ekle, eğer tam URL gelmiyorsa
         if (this.announcement.imageUrl && !this.announcement.imageUrl.startsWith('http')) {
           const apiBaseUrl = announcementService.apiBaseUrl || 'https://localhost:7274'; // api.js veya config dosyasından alınabilir
           this.announcement.imageUrl = `${apiBaseUrl}${this.announcement.imageUrl}`;
         }

      } catch (err) {
        console.error('Duyuru detayları getirilirken hata:', err);
        this.error = 'Duyuru detayları yüklenirken bir hata oluştu.';
         if (err.response && err.response.status === 404) {
          this.error = 'Duyuru bulunamadı.';
        }
      } finally {
        this.loading = false;
      }
    },
    formatDate(dateString) {
      if (!dateString) return '';
      const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
      return new Date(dateString).toLocaleDateString('tr-TR', options);
    }
  }
}
</script>

<style scoped>
.announcement-detail {
  /* max-width: 800px; */
  max-width: 1140px; /* İçeriği daha geniş yapmak için artırıldı */
  margin: 0 auto;
  padding: 1rem;
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
.detail-header h1 {
  margin-bottom: 0.5rem;
  color: #2c3e50;
  text-align: center; /* Başlığı ortala */
}
.detail-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #7f8c8d;
  flex-wrap: wrap; /* Küçük ekranlarda alt alta gelmesi için */
  justify-content: center; /* Meta verileri ortala */
}
.detail-content {
  margin-top: 2rem;
}
.detail-image {
  width: 100%;
  max-width: 600px; /* Resmin çok büyümesini engelle */
  height: auto; /* Oranı koru */
  border-radius: 8px;
  display: block; /* Ortalamak için */
  margin-left: auto;
  margin-right: auto;
}
.description {
  font-size: 1.1rem;
  line-height: 1.6;
  color: #444;
   white-space: pre-wrap; /* Satır sonlarını korumak için */
   text-align: center; /* İçeriği ortala */
}
.loading, .error {
  text-align: center;
  padding: 2rem;
  font-size: 1.2rem;
}
.error {
  color: red;
}

/* Stillerde v-html ile gelen içeriğe stil vermek için >>> veya ::v-deep kullanabilirsiniz */
.description ::v-deep p {
  margin-bottom: 1em; /* Paragraflar arasına boşluk */
}
.description ::v-deep ul,
.description ::v-deep ol {
  padding-left: 20px; /* Listeleri içerden başlat */
  margin-bottom: 1em;
}
.description ::v-deep li {
  margin-bottom: 0.5em; /* Liste elemanları arasına boşluk */
}

</style>
  