<template>
  <div class="profile-page">
    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Profil bilgileri yükleniyor...</p>
    </div>

    <div v-if="error" class="error-message">
      <p>{{ error }}</p>
      <button @click="fetchUserProfile" class="retry-btn">
        <i class="fas fa-sync"></i> Tekrar Dene
      </button>
    </div>

    <div v-if="!loading && !error" class="profile-header">
      <div class="profile-avatar">
        <img :src="user.avatar || 'https://via.placeholder.com/150'" :alt="user.name">
        <button class="btn btn-primary" @click="changeAvatar">
          <i class="fas fa-camera"></i> Fotoğraf Değiştir
        </button>
      </div>
      <div class="profile-info">
        <h1>{{ user.name }}</h1>
        <p class="role">Topluluk Danışmanı</p>
        <p class="department">{{ user.department }}</p>
      </div>
    </div>

    <div class="profile-content">
      <div class="profile-section">
        <h2>Kişisel Bilgiler</h2>
        <div class="info-grid">
          <div class="info-item">
            <label>Ad Soyad</label>
            <p>{{ user.name }}</p>
          </div>
          <div class="info-item">
            <label>E-posta</label>
            <p>{{ user.email }}</p>
          </div>
          <div class="info-item">
            <label>Telefon</label>
            <p>{{ user.phone }}</p>
          </div>
          <div class="info-item">
            <label>Bölüm</label>
            <p>{{ user.department }}</p>
          </div>
          <div class="info-item">
            <label>Katılım Tarihi</label>
            <p>{{ formatDate(user.joinDate) }}</p>
          </div>
        </div>
        <button class="btn btn-primary" @click="editProfile">
          <i class="fas fa-edit"></i> Profili Düzenle
        </button>
      </div>

      <div class="profile-section">
        <h2>Güvenlik Ayarları</h2>
        <div class="security-settings">
          <div class="security-item">
            <div class="security-info">
              <h3>İki Faktörlü Kimlik Doğrulama</h3>
              <p>{{ twoFactorEnabled ? 'Aktif' : 'Pasif' }}</p>
            </div>
            <button class="btn btn-primary" @click="toggleTwoFactor">
              {{ twoFactorEnabled ? 'Devre Dışı Bırak' : 'Aktifleştir' }}
            </button>
          </div>
          <div class="security-item">
            <div class="security-info">
              <h3>Şifre Değiştir</h3>
              <p>En son değiştirilme: {{ formatDate(lastPasswordChange) }}</p>
            </div>
            <button class="btn btn-primary" @click="showPasswordModal = true">
              Şifre Değiştir
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Profili Düzenle Modal -->
    <div v-if="showEditModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Profili Düzenle</h2>
          <button @click="closeEditModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveProfile">
            <div class="form-group">
              <label>Ad Soyad</label>
              <input type="text" v-model="editForm.name" required />
            </div>
            <div class="form-group">
              <label>E-posta</label>
              <input type="email" v-model="editForm.email" required />
            </div>
            <div class="form-group">
              <label>Telefon</label>
              <input type="tel" v-model="editForm.phone" required />
            </div>
            <div class="form-group">
              <label>Bölüm</label>
              <input type="text" v-model="editForm.department" required />
            </div>
            <!-- Şifre alanı kaldırıldı -->
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="closeEditModal">İptal</button>
              <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
                {{ isSubmitting ? 'Kaydediliyor...' : 'Kaydet' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Şifre Değiştir Modal -->
    <div v-if="showPasswordModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Şifre Değiştir</h2>
          <button @click="showPasswordModal = false" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="updatePassword">
            <div class="form-group">
              <label>Mevcut Şifre</label>
              <input type="password" v-model="passwordForm.currentPassword" required />
            </div>
            <div class="form-group">
              <label>Yeni Şifre</label>
              <input type="password" v-model="passwordForm.newPassword" required />
            </div>
            <div class="form-group">
              <label>Şifre Tekrar</label>
              <input type="password" v-model="passwordForm.confirmPassword" required />
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showPasswordModal = false">İptal</button>
              <button type="submit" class="btn btn-primary">Şifreyi Güncelle</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '@/services/api';
import { authService } from '@/services';

export default {
  name: 'AdvisorProfile',
  data() {
    return {
      loading: false,
      error: null,
      showEditModal: false,
      showPasswordModal: false,
      isSubmitting: false,
      twoFactorEnabled: false,
      user: {
        id: null,
        name: '',
        email: '',
        phone: '',
        department: '',
        avatar: '',
        joinDate: new Date(),
      },
      editForm: {
        name: '',
        email: '',
        phone: '',
        department: ''
      },
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      lastPasswordChange: new Date()
    };
  },
  created() {
    this.fetchUserProfile();
  },
  methods: {
    formatDate(date) {
      return new Date(date).toLocaleDateString('tr-TR');
    },
    async fetchUserProfile() {
      this.loading = true;
      try {
        const localUser = authService.getUser();
        const response = await api.get(`/users/${localUser.id}`);
        const data = response.data;
        this.user = {
          id: data.id,
          name: data.name,
          email: data.email,
          phone: data.phone,
          department: data.department,
          avatar: data.avatar,
          joinDate: data.joinDate || new Date()
        };
        this.twoFactorEnabled = data.twoFactorEnabled || false;
        this.lastPasswordChange = data.passwordUpdatedAt || new Date();
      } catch (err) {
        console.error('Kullanıcı bilgisi alınamadı:', err);
        this.error = 'Profil bilgileri yüklenemedi.';
      } finally {
        this.loading = false;
      }
    },
    editProfile() {
      this.editForm = {
        name: this.user.name,
        email: this.user.email,
        phone: this.user.phone,
        department: this.user.department,
      };
      this.showEditModal = true;
    },
    closeEditModal() {
      this.showEditModal = false;
      this.editForm.password = '';
    },
    async saveProfile() {
  this.isSubmitting = true;
  try {
    const nameParts = this.editForm.name.split(' ');
    const formFirstName = nameParts[0] || '';
    const formLastName = nameParts.slice(1).join(' ') || '';

    const currentUser = authService.getUser();

    const updateData = {
      FirstName: formFirstName,
      LastName: formLastName,
      Email: this.editForm.email,
      Phone: this.editForm.phone || '',
      Department: this.editForm.department,
      StudentId: currentUser.studentId || '',
      Password: this.editForm.password?.trim() || '',
      Role: currentUser.role || 'advisor',
    };

    // Şifreyi boş gönderme
    if (!updateData.Password) delete updateData.Password;

    await api.put(`/users/${currentUser.id}`, updateData);

    this.user = {
      ...this.user,
      name: this.editForm.name,
      email: this.editForm.email,
      phone: this.editForm.phone,
      department: this.editForm.department
    };

    authService.saveUser({ ...currentUser, ...this.user });
    this.showEditModal = false;
    alert('Profil başarıyla güncellendi.');
  } catch (err) {
    console.error('Profil güncelleme hatası:', err);
    alert('Profil güncellenirken bir hata oluştu.');
  } finally {
    this.isSubmitting = false;
  }
},
async updatePassword() {
  if (!this.passwordForm.currentPassword) {
    alert('Mevcut şifrenizi girmeniz gerekiyor.');
    return;
  }

  if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
    alert('Yeni şifreler uyuşmuyor.');
    return;
  }

  if (this.passwordForm.newPassword.length < 6) {
    alert('Yeni şifre en az 6 karakter olmalı.');
    return;
  }

  try {
    const currentUser = authService.getUser();

    const updateData = {
      FirstName: this.user.name.split(' ')[0],
      LastName: this.user.name.split(' ').slice(1).join(' ') || '',
      Email: this.user.email,
      Phone: this.user.phone,
      Department: this.user.department,
      StudentId: currentUser.studentId || '',
      Role: currentUser.role || 'advisor',
      Password: this.passwordForm.newPassword
    };
    
    // Add CurrentPassword to the payload for the backend
    const payload = {
        ...updateData,
        CurrentPassword: this.passwordForm.currentPassword
    };

    const response = await api.put(`/users/${currentUser.id}`, payload); // Changed updateData to payload

    if (response && (response.status === 200 || response.status === 204 || response.data.success)) { // Added response.data.success as a possible check
        this.lastPasswordChange = new Date();
        currentUser.passwordUpdatedAt = this.lastPasswordChange.toISOString();
        authService.saveUser(currentUser);

        this.passwordForm = {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
        };
        this.showPasswordModal = false;
        alert('Şifren başarıyla güncellendi.');
    } else {
        alert('Şifre güncellenemedi: ' + (response?.data?.message || 'Form verilerinde veya mevcut şifrede sorun olabilir.'));
    }
  } catch (err) {
    console.error('Şifre değiştirilirken hata:', err);
    // Detailed error message from backend
    const message = err.response?.data?.message || err.response?.data?.error || 'Şifre güncellenemedi. Form verilerinde veya mevcut şifrede sorun olabilir.';
    alert(message);
  }
},

    toggleTwoFactor() {
      this.twoFactorEnabled = !this.twoFactorEnabled;
      alert('2FA özelliği test modunda güncellendi.');
    },
    changeAvatar() {
      alert('Avatar değiştirme özelliği yakında eklenecek.');
    }
  }
};
</script>


<style scoped>
.profile-page {
  padding: 2rem;
}

.profile-header {
  display: flex;
  gap: 2rem;
  margin-bottom: 2rem;
  padding: 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.profile-avatar {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.profile-avatar img {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
}

.profile-info {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.profile-info h1 {
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.profile-info .role {
  font-size: 1.1rem;
  margin: 0;
  color: #3498db;
  font-weight: 500;
}

.profile-info .department {
  font-size: 1rem;
  margin: 0.5rem 0 0 0;
  color: #666;
}

.profile-content {
  display: grid;
  gap: 2rem;
}

.profile-section {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.profile-section h2 {
  margin: 0 0 1.5rem 0;
  color: #2c3e50;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.info-item label {
  display: block;
  color: #666;
  margin-bottom: 0.5rem;
}

.info-item p {
  margin: 0;
  color: #2c3e50;
  font-weight: 500;
}

.security-settings {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.security-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.security-info h3 {
  margin: 0 0 0.25rem 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.security-info p {
  margin: 0;
  color: #666;
}

.loading-indicator,
.error-message {
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

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
}

.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 {
  margin: 0;
  color: #2c3e50;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #666;
  cursor: pointer;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
}

.form-group input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1.5rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-primary:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #95a5a6;
  color: white;
}

@media (max-width: 768px) {
  .profile-header {
    flex-direction: column;
    text-align: center;
    padding: 1rem;
  }

  .profile-avatar {
    margin-bottom: 1rem;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }

  .security-item {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}
</style> 