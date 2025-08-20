<template>
  <div class="create-club-page">
    <div class="page-header">
      <h1>Yeni Topluluk Oluştur</h1>
    </div>
    
    <transition name="fade">
      <div v-if="showDateWarning" class="notification-modal" style="position:fixed; top:30%; left:50%; transform:translate(-50%,-50%); background:#fff; color:#a11a1a; border:2px solid #ffb3b3; border-radius:10px; box-shadow:0 4px 24px rgba(0,0,0,0.18); z-index:9999; min-width:320px; max-width:90vw; padding:32px 24px; display:flex; flex-direction:column; align-items:center; gap:12px;">
        <i class="fas fa-exclamation-triangle" style="font-size:2rem;"></i>
        <div style="font-size:1.1rem; text-align:center;">Topluluk oluşturma sadece belirlenen tarihler arasında yapılabilir!</div>
      </div>
    </transition>

    <div v-if="notification.show" :class="['notification', `notification-${notification.type}`, { show: notification.show }]">
      <div class="notification-content">
        <div class="notification-icon">
          <i v-if="notification.type === 'success'" class="fas fa-check-circle"></i>
          <i v-else-if="notification.type === 'error'" class="fas fa-exclamation-circle"></i>
          <i v-else-if="notification.type === 'warning'" class="fas fa-exclamation-triangle"></i>
          <i v-else class="fas fa-info-circle"></i>
        </div>
        <span class="notification-message">{{ notification.message }}</span>
        <button class="notification-close" @click="notification.show = false">
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="notification-progress" :style="{ animationDuration: notification.duration + 'ms' }"></div>
    </div>

    <div v-if="isInDateRange" class="form-container">
      <form @submit.prevent="submitForm" class="create-club-form">
        <div class="form-group">
          <label for="clubName">Topluluk Adı <span class="required">*</span></label>
          <input
            type="text"
            id="clubName"
            v-model="clubForm.name"
            required
            placeholder="Topluluğun adını girin"
          />
        </div>

        <div class="form-group">
          <label for="description">Açıklama <span class="required">*</span></label>
          <textarea
            id="description"
            v-model="clubForm.description"
            rows="4"
            required
            placeholder="Topluluğun amacı ve faaliyetleri hakkında bilgi verin"
          ></textarea>
        </div>

        <div class="form-group">
          <label for="category">Kategori <span class="required">*</span></label>
          <select id="category" v-model="clubForm.categoryId" required>
            <option value="" disabled>Kategori seçin</option>
            <option v-for="category in categories" :key="category.id" :value="category.id">
              {{ category.name }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="advisor">Danışman <span class="required">*</span></label>
          <select id="advisor" v-model="clubForm.advisorId" required>
            <option value="" disabled>Danışman seçin</option>
            <option v-for="advisor in advisors" :key="advisor.advisorId" :value="advisor.advisorId">
              {{ advisor.fullName }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="clubImage">Topluluk Logosu</label>
          <div class="file-upload">
            <input
              type="file"
              id="clubImage"
              @change="handleImageUpload"
              accept="image/*"
            />
            <div class="file-preview" v-if="imagePreview">
              <img :src="imagePreview" alt="Logo Önizleme" />
              <button type="button" class="remove-btn" @click="removeImage">
                <i class="fas fa-times"></i>
              </button>
            </div>
            <div class="upload-placeholder" v-else>
              <i class="fas fa-cloud-upload-alt"></i>
              <span>Görsel yüklemek için tıklayın veya sürükleyin</span>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label for="clubDocument">Topluluk Belgesi (PDF)</label>
          <div class="file-upload">
            <input
              type="file"
              id="clubDocument"
              @change="handleDocumentUpload"
              accept="application/pdf"
            />
            <div class="file-preview document" v-if="documentName">
              <i class="fas fa-file-pdf document-icon"></i>
              <span>{{ documentName }}</span>
              <button type="button" class="remove-btn" @click="removeDocument">
                <i class="fas fa-times"></i>
              </button>
            </div>
            <div class="upload-placeholder" v-else>
              <i class="fas fa-file-upload"></i>
              <span>Topluluk tüzüğü veya diğer belgeleri yükleyin</span>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="$router.push('/student/communities')">
            İptal
          </button>
          <button type="submit" class="btn btn-primary" :disabled="loading">
            {{ loading ? 'Gönderiliyor...' : 'Topluluk Oluştur' }}
          </button>
        </div>
      </form>
    </div>
    <div v-else class="alert alert-warning" style="margin-top:32px; text-align:center; background:#fff3cd; color:#856404; border:1px solid #ffeeba; border-radius:6px; padding:24px;">
      <i class="fas fa-exclamation-circle" style="font-size:2rem;"></i>
      <div style="margin-top:12px; font-size:1.1rem;">Topluluk oluşturma şu an kapalıdır. Lütfen <b>{{ formatDateTR(clubCreateStartDate) }}</b> - <b>{{ formatDateTR(clubCreateEndDate) }}</b> tarihleri arasında tekrar deneyin.</div>
    </div>
  </div>
</template>

<script>
import { clubService, categoryService } from '@/services';
import { authService } from '@/services';

export default {
  name: 'CreateClub',
  data() {
    return {
      clubForm: {
        name: '',
        description: '',
        categoryId: '',
        advisorId: '',
        image: null,
        document: null
      },
      categories: [],
      advisors: [],
      imagePreview: null,
      documentName: null,
      loading: false,
      error: null,
      clubCreateStartDate: '2025-07-14',
      clubCreateEndDate: '2025-07-28',
      showDateWarning: false,
      notification: {
        show: false,
        type: 'info',
        message: '',
        timeout: null,
        duration: 5000
      }
    };
  },
  computed: {
    isInDateRange() {
      if (!this.clubCreateStartDate || !this.clubCreateEndDate) return true;
      const today = new Date();
      const start = new Date(this.clubCreateStartDate);
      const end = new Date(this.clubCreateEndDate);
      return today >= start && today <= end;
    }
  },
  methods: {
    showNotification(type, message, duration = 5000) {
      this.notification.type = type;
      this.notification.message = message;
      this.notification.show = true;
      if (this.notification.timeout) {
        clearTimeout(this.notification.timeout);
      }
      this.notification.timeout = setTimeout(() => {
        this.notification.show = false;
      }, duration);
    },
    async fetchCategories() {
      try {
        const response = await categoryService.getAllCategories();
        this.categories = response.data;
      } catch (error) {
        console.error('Kategorileri getirme hatası:', error);
        this.error = 'Kategoriler yüklenirken bir hata oluştu.';
      }
    },
    async fetchAdvisors() {
      try {
        const response = await clubService.getAdvisors();
        console.log('Advisors API response:', response.data);
        this.advisors = response.data.advisors;
      } catch (error) {
        console.error('Danışmanları getirme hatası:', error);
        this.error = 'Danışmanlar yüklenirken bir hata oluştu.';
      }
    },
    handleImageUpload(event) {
      const file = event.target.files[0];
      if (!file) return;

      if (!file.type.includes('image/')) {
        alert('Lütfen geçerli bir görsel dosyası yükleyin.');
        return;
      }

      this.clubForm.image = file;
      const reader = new FileReader();
      reader.onload = e => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    },
    handleDocumentUpload(event) {
      const file = event.target.files[0];
      if (!file) return;

      if (file.type !== 'application/pdf') {
        alert('Lütfen PDF formatında bir belge yükleyin.');
        return;
      }

      this.clubForm.document = file;
      this.documentName = file.name;
    },
    removeImage() {
      this.clubForm.image = null;
      this.imagePreview = null;
      document.getElementById('clubImage').value = '';
    },
    removeDocument() {
      this.clubForm.document = null;
      this.documentName = null;
      document.getElementById('clubDocument').value = '';
    },
    async fetchClubCreateDates() {
      try {
        const response = await fetch('/api/settings/club-create-dates');
        const data = await response.json();
        this.clubCreateStartDate = data.start || '';
        this.clubCreateEndDate = data.end || '';
      } catch (e) {
        console.error('Tarih aralığı alınamadı', e);
      }
    },
    async submitForm() {
      // Tarih kontrolü
      const today = new Date().toISOString().slice(0, 10);
      if (
        (this.clubCreateStartDate && today < this.clubCreateStartDate) ||
        (this.clubCreateEndDate && today > this.clubCreateEndDate)
      ) {
        this.showDateWarning = true;
        setTimeout(() => { this.showDateWarning = false; }, 4000);
        return;
      }
      // Başkanlık kontrolü
      try {
        const whoamiRes = await this.$axios.get('/api/memberships/whoami');
        console.log('whoami yanıtı:', whoamiRes.data);
        if (whoamiRes.data && whoamiRes.data.isLeaderOfAnyClub) {
          this.showNotification('error', 'Aynı anda yalnızca bir kulübün başkanı olabilirsiniz. Önce mevcut başkanlığınızı bırakmalısınız.');
          return;
        }
      } catch (err) {
        // whoami endpointi başarısız olursa işlemi durdurma, sadece logla
        console.error('Başkanlık kontrolü yapılamadı:', err);
      }
      this.loading = true;
      this.error = null;

      try {
        const formData = new FormData();
        formData.append('name', this.clubForm.name);
        formData.append('description', this.clubForm.description);
        formData.append('categoryId', Number(this.clubForm.categoryId));
        formData.append('advisorId', Number(this.clubForm.advisorId));

        console.log('Form içeriği:', {
          name: this.clubForm.name,
          description: this.clubForm.description,
          categoryId: Number(this.clubForm.categoryId),
          advisorId: Number(this.clubForm.advisorId),
          formData: [...formData.entries()]
        });

        if (this.clubForm.image) {
          formData.append('image', this.clubForm.image);
        }

        if (this.clubForm.document) {
          formData.append('document', this.clubForm.document);
        }

        const user = authService.getUser();
        formData.append('userId', user.id);
        
        await clubService.createClub(formData);
        this.loading = false;
        this.showNotification('success', 'Topluluk oluşturma talebiniz başarıyla iletildi!');
        this.$router.push('/student/communities');
      } catch (error) {
        this.loading = false;
        let msg = 'Topluluk oluşturulurken bir hata oluştu.';
        if (error.response && error.response.data) {
          if (typeof error.response.data === 'string') {
            msg = error.response.data;
          } else if (error.response.data.message) {
            msg = error.response.data.message;
          }
        }
        this.showNotification('error', msg);
      }
    },
    formatDateTR(dateStr) {
      if (!dateStr) return '';
      const d = new Date(dateStr);
      const gun = String(d.getDate()).padStart(2, '0');
      const ay = String(d.getMonth() + 1).padStart(2, '0');
      const yil = d.getFullYear();
      return `${gun}.${ay}.${yil}`;
    }
  },
  mounted() {
    this.fetchClubCreateDates();
    this.fetchCategories();
    this.fetchAdvisors();
  }
};
</script>

<style scoped>
.create-club-page {
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.form-container {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  max-width: 800px;
  margin: 0 auto;
}

.create-club-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

label {
  font-weight: 500;
  color: #2c3e50;
}

.required {
  color: #e74c3c;
}

input, textarea, select {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

input:focus, textarea:focus, select:focus {
  border-color: #3498db;
  outline: none;
}

textarea {
  resize: vertical;
  min-height: 100px;
}

.file-upload {
  position: relative;
  border: 2px dashed #ddd;
  border-radius: 4px;
  padding: 1.5rem;
  text-align: center;
  transition: border-color 0.3s;
  cursor: pointer;
}

.file-upload:hover {
  border-color: #3498db;
}

.file-upload input[type="file"] {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  cursor: pointer;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  color: #7f8c8d;
}

.upload-placeholder i {
  font-size: 2rem;
}

.file-preview {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.file-preview img {
  max-width: 100px;
  max-height: 100px;
  border-radius: 4px;
}

.file-preview.document {
  justify-content: center;
}

.document-icon {
  font-size: 2rem;
  color: #e74c3c;
}

.remove-btn {
  background: #e74c3c;
  color: white;
  border: none;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.btn-primary:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #ecf0f1;
  color: #2c3e50;
}

.btn-secondary:hover {
  background-color: #bdc3c7;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.4s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* Notification Styles */
.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  background-color: #3498db;
  color: white;
  padding: 15px 25px;
  border-radius: 8px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  display: flex;
  align-items: center;
  gap: 10px;
  z-index: 10000;
  opacity: 0;
  transform: translateX(100%);
  transition: opacity 0.5s, transform 0.5s;
}
.notification.show {
  opacity: 1;
  transform: translateX(0);
}

.notification.notification-success {
  background-color: #2ecc71;
}

.notification.notification-error {
  background-color: #e74c3c;
}

.notification.notification-warning {
  background-color: #f39c12;
}

.notification.notification-info {
  background-color: #3498db;
}

.notification-content {
  display: flex;
  align-items: center;
  gap: 10px;
}

.notification-icon {
  font-size: 1.5rem;
}

.notification-message {
  font-size: 1rem;
  font-weight: 500;
}

.notification-close {
  background: none;
  border: none;
  color: white;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  transition: background-color 0.3s;
}

.notification-close:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.notification-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 4px;
  background-color: rgba(255, 255, 255, 0.5);
  border-radius: 2px;
  /* Animasyon süresi sadece inline style ile ayarlanacak */
}

@keyframes progress {
  from {
    width: 0%;
  }
  to {
    width: 100%;
  }
}
</style> 