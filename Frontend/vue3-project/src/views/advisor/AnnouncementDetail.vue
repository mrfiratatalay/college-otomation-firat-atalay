<template>
  <div class="announcement-detail container py-4">
    <!-- Geri Dön Butonu -->
    <button class="back-btn" @click="$router.back()">
      <i class="fas fa-arrow-left"></i> Geri Dön
    </button>

    <!-- Duyuru Başlık ve Üst Meta Alanı -->
    <div class="detail-header">
      <h1>{{ announcement.title }}</h1>
      <!-- META BİLGİLERİ GÖRSELLEŞTİRİLMİŞ HALDE -->
      <div class="meta-bar">
        <div class="meta-item" v-if="announcement.community">
          <i class="fas fa-users"></i>
          <span>{{ announcement.community }}</span>
        </div>
        <div class="meta-item" v-if="announcement.type">
          <i class="fas fa-tag"></i>
          <span>{{ announcement.type }}</span>
        </div>
        <div class="meta-item" v-if="announcement.date">
          <i class="fas fa-calendar"></i>
          <span>{{ formatDate(announcement.date) }}</span>
        </div>
      </div>
    </div>

    <!-- Duyuru Görseli -->
    <div class="detail-image" v-if="announcement.image">
      <img :src="announcement.image" :alt="announcement.title">
    </div>

    <!-- Duyuru Açıklama ve İstatistikler -->
    <div class="detail-content">
      <p class="description">{{ announcement.content }}</p>
      <div class="detail-stats">
        <span>
          <i class="fas fa-eye"></i> {{ announcement.views }} Görüntülenme
        </span>
        <span>
          <i class="fas fa-comment"></i> {{ announcement.comments }} Yorum
        </span>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AdvisorAnnouncementDetail',
  data() {
    return {
      announcement: {
        id: null,
        title: '',
        community: '',
        type: '',
        content: '',
        image: '',
        date: '',
        views: 0,
        comments: 0
      }
    }
  },
  created() {
    // Rota parametresinden duyuru ID'sini alıyoruz
    const announcementId = this.$route.params.id;
    
    // Demo amaçlı statik veri, gerçek uygulamada API çağrısı yapılmalı
    const demoAnnouncements = [
      {
        id: 1,
        title: 'Web Geliştirme Workshop Kayıtları Başladı',
        community: 'Teknoloji Topluluğu',
        type: 'Etkinlik',
        content: 'Modern web teknolojileri hakkında pratik bir workshop düzenliyoruz. Katılım için kayıt olun!',
        image: '/images/web-workshop.jpg',
        date: '2024-03-15',
        views: 150,
        comments: 12,
        status: 'active'
      },
      {
        id: 2,
        title: 'Fotoğraf Sergisi Açılışı',
        community: 'Fotoğrafçılık Kulübü',
        type: 'Önemli',
        content: 'Kulüp üyelerimizin fotoğraflarının sergilendiği sergimizin açılışına davetlisiniz.',
        image: '/images/photo-exhibition.jpg',
        date: '2024-03-10',
        views: 200,
        comments: 8,
        status: 'active'
      }
    ];

    // ID'ye göre duyuruyu buluyoruz
    const found = demoAnnouncements.find(a => a.id == announcementId);
    if (found) {
      this.announcement = found;
    }
  },
  methods: {
    formatDate(dateString) {
      const options = { year: 'numeric', month: 'long', day: 'numeric' }
      return new Date(dateString).toLocaleDateString('tr-TR', options)
    }
  }
}
</script>

<style scoped>
.announcement-detail {
  max-width: 900px; /* Genişliği artırarak daha ferah görünüm */
  margin: 0 auto;
  padding: 1rem;
  background: #fdfdfd; /* Hafif beyaz arkaplan */
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
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
  margin-bottom: 1.5rem;
}

/* Duyuru Başlığı */
.detail-header h1 {
  margin: 0 0 1rem;
  color: #2c3e50;
  font-size: 2rem;
  font-weight: 600;
}

/* Meta Bar: topluluk, tip, tarih */
.meta-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  background: #f4f6f7;
  border-radius: 6px;
  padding: 0.75rem 1rem;
  box-shadow: inset 0 1px 3px rgba(0,0,0,0.06);
}

/* Her meta öğesi */
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
}

.detail-content {
  margin-top: 1rem;
}

/* Açıklama */
.description {
  font-size: 1.1rem;
  line-height: 1.6;
  color: #444;
  margin-bottom: 1.5rem;
}

/* İstatistikler: Görüntülenme, Yorum */
.detail-stats {
  display: flex;
  gap: 2rem;
  font-size: 1rem;
  color: #7f8c8d;
}

.detail-stats span {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
}

.detail-stats i {
  color: #3498db;
}
</style>
