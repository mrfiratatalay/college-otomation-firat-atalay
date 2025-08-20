<template>
  <div class="profile-page">
    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Kullanıcı bilgileri yükleniyor...</p>
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
        <p class="role">Sistem Yöneticisi</p>
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
            <p>{{ user.phone || '-' }}</p>
          </div>
          <div class="info-item">
            <label>Öğrenci No</label>
            <p>{{ user.studentId || '-' }}</p>
          </div>
          <div class="info-item">
            <label>Bölüm</label>
            <p>{{ user.department || '-' }}</p>
          </div>
          <div class="info-item">
            <label>Rol</label>
            <p>{{ user.role }}</p>
          </div>
          <div class="info-item">
            <label>Katılım Tarihi</label>
            <p>{{ user.joinDate }}</p>
          </div>
        </div>
        <button class="btn btn-primary" @click="editProfile">
          <i class="fas fa-edit"></i> Profili Düzenle
        </button>
      </div>

      <div class="profile-section">
        <h2>Katıldığım Topluluklar</h2>
        <div v-if="userCommunities.length === 0" class="empty-state">
          <p>Henüz herhangi bir topluluğa katılmadınız.</p>
          <router-link to="/student/communities" class="btn btn-primary">
            Toplulukları Keşfet
          </router-link>
        </div>
        <div v-else class="communities-grid">
          <div v-for="community in userCommunities" :key="community.id" class="community-card">
            <div class="community-image">
              <div class="image-placeholder mb-3">
                <img v-if="community.image" :src="'data:image/jpeg;base64,' + community.image" alt="Topluluk Resmi" class="img-fluid">
                <span v-else>Resim Yok</span>
              </div>
              <div class="status-badge" :class="community.status ? community.status.toLowerCase() : 'active'">
                {{ community.status || 'Aktif' }}
              </div>
            </div>
            <div class="community-info">
              <h3>{{ community.name }}</h3>
              <p class="category">{{ community.categoryName || community.category }}</p>
              <div class="stats">
                <span><i class="fas fa-users"></i> {{ community.memberCount || 0 }} Üye</span>
                <span><i class="fas fa-calendar"></i> {{ community.eventCount || 0 }} Etkinlik</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="profile-section">
        <h2>Katıldığım Etkinlikler</h2>
        <div v-if="userEvents.length === 0" class="empty-state">
          <p>Henüz herhangi bir etkinliğe katılmadınız.</p>
          <router-link to="/student/events" class="btn btn-primary">
            Etkinlikleri Keşfet
          </router-link>
        </div>
        <div v-else class="events-grid">
          <div v-for="event in userEvents" :key="event.eventId" class="event-card">
            <div class="event-date">
              <span class="day">{{ formatDate(event.startDate, 'day') }}</span>
              <span class="month">{{ formatDate(event.startDate, 'month') }}</span>
            </div>
            <div class="event-info">
              <h3>{{ event.name }}</h3>
              <p>{{ event.description }}</p>
              <div class="event-meta">
                <span><i class="fas fa-clock"></i> {{ formatTime(event.startDate) }}</span>
                <span><i class="fas fa-map-marker-alt"></i> {{ event.location || 'Konum belirtilmemiş' }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="profile-section">
        <h2>Güvenlik Ayarları</h2>
        <div class="security-options">
          <div class="security-item">
            <div class="security-info">
              <h3>İki Faktörlü Kimlik Doğrulama</h3>
              <p>{{ user.twoFactorEnabled ? 'Aktif' : 'Pasif' }}</p>
            </div>
            <router-link to="/two-factor-setup" class="btn"
              :class="user.twoFactorEnabled ? 'btn-danger' : 'btn-primary'">
              {{ user.twoFactorEnabled ? 'Devre Dışı Bırak' : 'Aktifleştir' }}
            </router-link>
          </div>
          <div class="security-item">
            <div class="security-info">
              <h3>Şifre Değiştir</h3>
              <p>En son değiştirilme: {{ user.lastPasswordChange }}</p>
            </div>
            <button class="btn btn-primary" @click="changePassword">
              Şifre Değiştir
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Profil Düzenleme Modalı -->
    <div v-if="showEditModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Profili Düzenle</h2>
          <button class="close-btn" @click="closeEditModal">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveProfile">
            <div class="form-group">
              <label>Ad Soyad</label>
              <input type="text" v-model="editForm.name" required>
            </div>
            <div class="form-group">
              <label>E-posta</label>
              <input type="email" v-model="editForm.email" required>
            </div>
            <div class="form-group">
              <label>Telefon</label>
              <input type="tel" v-model="editForm.phone" required>
            </div>
            <div class="form-group">
              <label>Bölüm</label>
              <select v-model="editForm.departmentId" :disabled="departmentLoading" class="form-control">
                <option :value="null">-- Bölüm Seçiniz --</option>
                <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                  {{ dept.name }}
                </option>
              </select>
              <small v-if="departmentLoading" class="form-text text-muted">Bölümler yükleniyor...</small>
            </div>
            <!-- Şifre alanı kaldırıldı -->
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="closeEditModal">İptal</button>
              <button type="submit" class="btn btn-primary">Kaydet</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Şifre Değiştirme Modalı -->
    <div v-if="showPasswordModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Şifre Değiştir</h2>
          <button class="close-btn" @click="closePasswordModal">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="updatePassword">
            <div class="form-group">
              <label>Mevcut Şifre</label>
              <input type="password" v-model="passwordForm.currentPassword" required>
            </div>
            <div class="form-group">
              <label>Yeni Şifre</label>
              <input type="password" v-model="passwordForm.newPassword" required>
            </div>
            <div class="form-group">
              <label>Şifre Tekrar</label>
              <input type="password" v-model="passwordForm.confirmPassword" required>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="closePasswordModal">İptal</button>
              <button type="submit" class="btn btn-primary">Şifreyi Güncelle</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { authService, clubService, eventService, announcementService, studentService, dashboardService } from '@/services';
import api from '@/services/api';

export default {
  name: 'StudentProfile',
  data() {
    return {
      loading: false,
      error: null,
      user: {
        name: '',
        email: '',
        phone: '',
        studentId: '',
        department: '',
        departmentId: null,
        role: '',
        avatar: '',
        joinDate: '',
        twoFactorEnabled: false,
        lastPasswordChange: ''
      },
      departments: [],
      departmentLoading: false,
      systemStats: {
        communities: 0,
        events: 0,
        users: 0,
        announcements: 0
      },
      showEditModal: false,
      showPasswordModal: false,
      editForm: {
        name: '',
        email: '',
        phone: '',
        departmentId: null
      },
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      studentData: {
        upcomingEvents: [],
        joinedClubs: [],
        totalEvents: 0
      },
      activeEvents: []
    }
  },
  async created() {
    await this.fetchUserProfile();
    await this.fetchUserData();
  },
  methods: {
    async fetchUserProfile() {
      this.loading = true;
      this.error = null;
      const localUser = authService.getUser();

      try {
        const response = await api.get(`/users/${localUser.id}`);
        this.user = response.data;
      } catch (err) {
        this.error = 'Kullanıcı bilgileri yüklenirken bir hata oluştu.';
        console.error('Kullanıcı bilgileri yüklenirken hata:', err);
      } finally {
        this.loading = false;
      }
    },

    async fetchUserData() {
      try {
        // Dashboard servisinden kullanıcı verilerini al
        const dashboardResponse = await dashboardService.getStudentDashboard();
        console.log('Dashboard verisi:', dashboardResponse.data);

        if (dashboardResponse.data) {
          // Önce topluluk ID'lerini al
          const joinedClubs = dashboardResponse.data.joinedClubs || [];
          
          // Her bir topluluk için detaylı bilgileri getir
          const clubDetailsPromises = joinedClubs.map(async (club) => {
            try {
              // Topluluk detaylarını getir
              const response = await studentService.getClubDetails(club.clubId);
              const clubData = response.data.club; // API yanıtından club nesnesini al
              
              // API'den gelen verileri doğrudan kullan
              return {
                id: clubData.clubId,
                name: clubData.name,
                description: clubData.description,
                image: clubData.image,
                categoryId: clubData.categoryId,
                categoryName: clubData.categoryName,
                status: clubData.status,
                memberCount: clubData.memberCount || 0,
                eventCount: clubData.eventCount || 0,
                advisorName: clubData.advisorFullName,
                createdAt: clubData.createdAt
              };
            } catch (error) {
              console.error(`Topluluk detayları alınamadı (ID: ${club.clubId}):`, error);
              return {
                id: club.clubId,
                name: club.name || 'İsimsiz Topluluk',
                description: '',
                status: 'active',
                memberCount: 0,
                eventCount: 0,
                categoryName: ''
              };
            }
          });

          // Tüm topluluk detaylarını bekle
          const clubsWithDetails = await Promise.all(clubDetailsPromises);
          console.log('İşlenmiş topluluk detayları:', clubsWithDetails);
          this.studentData.joinedClubs = clubsWithDetails;
        }

        // Etkinlikleri getir
        const eventsResponse = await studentService.getEvents();
        if (eventsResponse.data) {
          this.activeEvents = eventsResponse.data.filter(event => event.status === 'approved');
          this.studentData.upcomingEvents = this.activeEvents;
          console.log('Aktif etkinlikler:', this.activeEvents);
        }
      } catch (err) {
        console.error('Kullanıcı verileri yüklenirken hata:', err);
      }
    },

    async fetchDepartments() {
      if (this.departments.length > 0) return;
      this.departmentLoading = true;
      try {
        console.log('Departmanlar getiriliyor...');
        const response = await api.get('/users/departments');
        this.departments = response.data || [];
        console.log('Departmanlar:', this.departments);
      } catch (error) {
        console.error('Bölümler getirilirken hata:', error);
        alert('Departman listesi yüklenirken bir hata oluştu.');
      } finally {
        this.departmentLoading = false;
      }
    },

    // Edit formunu güncel kullanıcı bilgileri ile doldur
    updateEditForm() {
      this.editForm = {
        name: this.user.name || '',
        email: this.user.email || '',
        phone: this.user.phone || '',
        department: this.user.department || ''
      };
    },

    async fetchSystemStats() {
      // İstatistikleri başlangıçta sıfırla veya varsayılan bir değere ayarla
      this.systemStats = {
        communities: 0,
        events: 0,
        users: 0,
        announcements: 0
      };

      // Topluluk sayısını al (Ayrı try-catch)
      try {
        const communitiesResponse = await clubService.getAllClubs();
        this.systemStats.communities = communitiesResponse.data?.length || 0;
      } catch (error) {
        console.error('Topluluk sayısı alınırken hata:', error);
        this.systemStats.communities = 0; // Hata durumunda 0 ayarla
      }

      // Etkinlik sayısını al (Ayrı try-catch)
      try {
        const eventsResponse = await eventService.getAllEvents();
        this.systemStats.events = eventsResponse.data?.length || 0;
      } catch (error) {
        console.error('Etkinlik sayısı alınırken hata:', error);
        this.systemStats.events = 0; // Hata durumunda 0 ayarla
      }

      // Duyuru sayısını al (Ayrı try-catch)
      try {
        const announcementsResponse = await announcementService.getAllAnnouncements();
        this.systemStats.announcements = announcementsResponse.data?.length || 0;
      } catch (error) {
        console.error('Duyuru sayısı alınırken hata:', error);
        this.systemStats.announcements = 0; // Hata durumunda 0 ayarla
      }

      // Kullanıcı sayısını al (Ayrı try-catch)
      try {
        const usersResponse = await api.get('/users');
        this.systemStats.users = usersResponse.data?.totalItems || 0;
      } catch (error) {
        console.error('Kullanıcı sayısı alınamadı:', error);
        this.systemStats.users = 0; // Hata durumunda 0 ayarla
      }
    },

    // Sistem istatistikleri API'den alınamazsa varsayılan değerler kullan
    trySystemStats() {
      // Varsayılan değerler
      this.systemStats = {
        communities: 0,
        events: 0,
        users: 0,
        announcements: 0
      };
    },
    editProfile() {
      this.fetchDepartments();
      this.editForm.name = this.user.name;
      this.editForm.email = this.user.email;
      this.editForm.phone = this.user.phone || '';
      this.editForm.department = this.user.department || '';
      this.editForm.departmentId = this.user.departmentId || null;
      this.editForm.password = '';
      this.showEditModal = true;
    },
    closeEditModal() {
      this.showEditModal = false;
    },
    async saveProfile() {
      try {
        // Ad ve soyad alanlarını ayır (formdan gelen)
        const nameParts = this.editForm.name.split(' ');
        const formFirstName = nameParts[0] || '';
        const formLastName = nameParts.slice(1).join(' ') || '';

        // Mevcut kullanıcı verileri
        const originalUser = this.user;

        // API'ye gönderilecek veri nesnesi - tüm gerekli alanları içerir
        const updateData = {
          FirstName: formFirstName || originalUser.name, // Form boşsa veya nullsa eski adı kullan
          LastName: formLastName || originalUser.surname, // Form boşsa veya nullsa eski soyadı kullan
          Email: this.editForm.email || originalUser.email, // Form boşsa veya nullsa eski epostayı kullan
          Phone: this.editForm.phone || originalUser.phone || "", // Form boşsa eskiyi kullan, o da yoksa boş string
          StudentId: originalUser.studentId || "", // Admin için StudentId genellikle boş olur
          Password: "", // Varsayılan olarak boş şifre gönder
          Role: originalUser.role || "admin", // Mevcut rolü veya varsayılanı gönder
          DepartmentId: this.editForm.departmentId // Formdaki seçili ID (null olabilir)
        };

        // Eğer formda yeni bir şifre girildiyse, onu kullan
        if (this.editForm.password && this.editForm.password.trim().length > 0) {
          updateData.Password = this.editForm.password;
        }

        console.log('Sending complete update data:', JSON.stringify(updateData, null, 2));

        const userId = originalUser.id;
        // Doğrudan api.put kullanıyoruz (authService.updateProfile yerine)
        const response = await api.put(`/users/${userId}`, updateData);

        console.log('Update response status:', response.status);

        // Başarılı güncelleme sonrası profili yeniden yükle
        await this.fetchUserProfile();

        this.closeEditModal();
        alert('Profil başarıyla güncellendi');
      } catch (error) {
        console.error('Profil güncellenirken hata:', error);

        if (error.response && error.response.data) {
          console.error('Error details:', error.response.data);
          if (error.response.data.errors) {
            console.error('Validation errors:', JSON.stringify(error.response.data.errors, null, 2));
            let errorMessage = 'Profil güncellenemedi:\n';
            for (const field in error.response.data.errors) {
              errorMessage += `${field}: ${error.response.data.errors[field].join(', ')}\n`;
            }
            alert(errorMessage);
            return;
          }
        }
        alert('Profil güncellenirken bir hata oluştu: ' + (error.response?.data?.message || error.message));
      }
    },
    async changeAvatar() {
      // Gizli dosya girişi oluştur
      const fileInput = document.createElement('input');
      fileInput.type = 'file';
      fileInput.accept = 'image/*';

      // Dosya seçme işlemini başlat
      fileInput.click();

      // Dosya seçildiğinde
      fileInput.onchange = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        // Dosya boyutu kontrol et (2MB)
        if (file.size > 2 * 1024 * 1024) {
          alert('Dosya boyutu 2MB\'dan küçük olmalıdır');
          return;
        }

        // Sadece görsel dosyaları kabul et
        if (!file.type.startsWith('image/')) {
          alert('Lütfen geçerli bir görsel dosyası seçin');
          return;
        }

        // Dosyayı form verisi olarak hazırla
        const formData = new FormData();
        formData.append('avatar', file);

        // Yüklemeyi başlat
        this.loading = true;

        // Dosyayı ön izleme
        const reader = new FileReader();
        reader.readAsDataURL(file);

        // Dosya okunduğunda ön izleme göster
        reader.onload = async (e) => {
          try {
            // Önce dosyayı ön izleme için göster
            const previewImage = e.target.result;
            this.user.avatar = previewImage;

            // Backend API çağrısı yap
            try {
              // Backend'e avatar yükleme işlemini gerçekleştir
              const response = await authService.updateAvatar(formData);

              if (response && response.data && response.data.avatarUrl) {
                // API'den dönen gerçek URL ile güncelle
                this.user.avatar = response.data.avatarUrl;

                // Kullanıcı bilgilerini yerel depolamada güncelle
                const currentUser = authService.getUser();
                if (currentUser) {
                  currentUser.avatar = response.data.avatarUrl;
                  authService.saveUser(currentUser);
                }

                alert('Profil fotoğrafı başarıyla güncellendi');
              } else if (response && response.data && response.data.avatar) {
                // API farklı bir alanda döndürüyorsa
                this.user.avatar = response.data.avatar;

                // Kullanıcı bilgilerini yerel depolamada güncelle
                const currentUser = authService.getUser();
                if (currentUser) {
                  currentUser.avatar = response.data.avatar;
                  authService.saveUser(currentUser);
                }

                alert('Profil fotoğrafı başarıyla güncellendi');
              } else {
                // API yanıtı yoksa ön izleme ile devam et
                console.warn('Avatar URL alınamadı, önizleme kullanılıyor');

                // Kullanıcı bilgilerini önizleme ile güncelle
                const currentUser = authService.getUser();
                if (currentUser) {
                  currentUser.avatar = previewImage;
                  authService.saveUser(currentUser);
                }

                alert('Profil fotoğrafı güncellendi');
              }
            } catch (error) {
              console.error('Avatar güncellenirken hata:', error);
              // Hata olduğunda, en azından ön izleme görüntüsünü göster
              alert('Profil fotoğrafı güncellenirken bir hata oluştu: ' + (error.response?.data?.message || error.message));
            }
          } catch (error) {
            console.error('Avatar işleme hatası:', error);
            alert('Profil fotoğrafı işlenirken bir hata oluştu.');
          } finally {
            this.loading = false;
          }
        };
      };
    },
    async toggleTwoFactor() {
      if (this.user.twoFactorEnabled) {
        // 2FA'yı devre dışı bırakma işlemi
        if (confirm('İki faktörlü kimlik doğrulama devre dışı bırakılacak. Onaylıyor musunuz?')) {
          try {
            // Backend API çağrısı
            const response = await authService.disable2FA();

            // Kullanıcı durumunu güncelle
            this.user.twoFactorEnabled = false;

            // Kullanıcı bilgisini local storage'da güncelle
            const currentUser = authService.getUser();
            if (currentUser) {
              currentUser.twoFactorEnabled = false;
              authService.saveUser(currentUser);
            }

            alert('İki faktörlü kimlik doğrulama devre dışı bırakıldı');
          } catch (error) {
            console.error('2FA devre dışı bırakılırken hata:', error);
            alert('İki faktörlü kimlik doğrulama devre dışı bırakılırken bir hata oluştu: ' + (error.response?.data?.message || error.message));
          }
        }
      } else {
        // 2FA'yı aktifleştirmek için kurulum sayfasına yönlendir veya modal göster
        try {
          const response = await authService.enable2FA();
          if (response && response.data) {
            // QR kodu ve manuel kodu içeren bir modal göster
            alert('2FA QR kodu oluşturuldu. Lütfen Google Authenticator uygulamasını kullanarak QR kodu taratın veya manuel kodu girin: ' +
              (response.data.manualCode || 'Kod bilgisi alınamadı'));

            // İdeal olarak burada QR kodu gösteren bir modal açılmalı
            // Şu an için basit bir alert kullanıyoruz

            // Doğrulama kodu girmesi için prompt göster
            const verificationCode = prompt('Lütfen doğrulama kodunu girin:');
            if (verificationCode) {
              const verifyResponse = await authService.verify2FA(verificationCode);
              if (verifyResponse && verifyResponse.data) {
                this.user.twoFactorEnabled = true;

                // Kullanıcı bilgisini local storage'da güncelle
                const currentUser = authService.getUser();
                if (currentUser) {
                  currentUser.twoFactorEnabled = true;
                  authService.saveUser(currentUser);
                }

                alert('İki faktörlü kimlik doğrulama başarıyla etkinleştirildi');
              }
            }
          }
        } catch (error) {
          console.error('2FA etkinleştirilirken hata:', error);
          alert('İki faktörlü kimlik doğrulama etkinleştirilirken bir hata oluştu: ' + (error.response?.data?.message || error.message));
        }
      }
    },
    changePassword() {
      this.passwordForm.currentPassword = '';
      this.passwordForm.newPassword = '';
      this.passwordForm.confirmPassword = '';
      this.showPasswordModal = true;
    },
    closePasswordModal() {
      this.showPasswordModal = false;
    },
    async updatePassword() {
      try {
        // Şifre kontrolleri
        if (!this.passwordForm.currentPassword) {
          alert('Mevcut şifrenizi girmelisiniz');
          return;
        }

        if (this.passwordForm.newPassword.length < 6) {
          alert('Yeni şifre en az 6 karakter olmalıdır');
          return;
        }

        if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
          alert('Yeni şifre ve şifre tekrarı eşleşmiyor');
          return;
        }

        // API'ye şifre değiştirme isteği gönder
        const response = await authService.changePassword({
          currentPassword: this.passwordForm.currentPassword,
          newPassword: this.passwordForm.newPassword
        });

        // API yanıtını kontrol et
        if (response && (response.status === 200 || response.status === 204)) {
          // Başarılı olduğunda kullanıcı bilgisini güncelle
          this.user.lastPasswordChange = new Date().toLocaleDateString('tr-TR');

          // Kullanıcı bilgisini local storage'da güncelle
          const currentUser = authService.getUser();
          if (currentUser) {
            currentUser.passwordUpdatedAt = new Date().toISOString();
            authService.saveUser(currentUser);
          }

          this.closePasswordModal();
          alert('Şifreniz başarıyla güncellendi');
        } else {
          // API'den gelen hata mesajını göster
          alert('Şifre güncellenemedi: ' + (response?.data?.message || 'Bilinmeyen bir hata oluştu.'));
        }
      } catch (error) {
        console.error('Şifre değiştirilirken hata:', error);
        alert('Şifre değiştirilirken bir hata oluştu: ' + (error.response?.data?.message || 'Mevcut şifrenizi kontrol edin.'));
      }
    },
    // Temel kullanıcı verilerini ayarlar
    setUserFromBasicData(userData, localUser) {
      this.user = {
        id: userData.UserId || localUser?.id,
        name: localUser?.name || 'Admin Kullanıcı',
        email: userData.Email || localUser?.email || 'admin@elasoft.edu.tr',
        phone: localUser?.phone || '',
        studentId: localUser?.studentId || '',
        department: localUser?.department || '',
        departmentId: localUser?.departmentId || null,
        department: localUser?.department || '',
        avatar: localUser?.avatar || 'https://randomuser.me/api/portraits/men/75.jpg',
        joinDate: localUser?.joinDate
          ? new Date(localUser.joinDate).toLocaleDateString('tr-TR')
          : new Date().toLocaleDateString('tr-TR'),
        lastPasswordChange: localUser?.passwordUpdatedAt
          ? new Date(localUser.passwordUpdatedAt).toLocaleDateString('tr-TR')
          : new Date().toLocaleDateString('tr-TR'),
        twoFactorEnabled: localUser?.twoFactorEnabled || false,
        role: userData.Role || localUser?.role || 'admin'
      };

      // Yerel depolamaya güncellenmiş veriyi kaydet
      if (localUser) {
        localUser.name = this.user.name;
        localUser.email = this.user.email;
        localUser.phone = this.user.phone;
        localUser.studentId = this.user.studentId;
        localUser.departmentId = this.user.departmentId;
        localUser.role = this.user.role;
        localUser.avatar = this.user.avatar;
        authService.saveUser(localUser);
      }
    },
    formatDate(dateStr, format) {
      if (!dateStr) return 'NaN';
      try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return 'NaN';
        
        if (format === 'day') {
          return date.getDate();
        } else if (format === 'month') {
          return date.toLocaleString('tr-TR', { month: 'short' });
        }
        return date.toLocaleDateString('tr-TR');
      } catch (error) {
        console.error('Tarih formatlanırken hata:', error);
        return 'NaN';
      }
    },
    formatTime(dateStr) {
      if (!dateStr) return '';
      try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return '';
        return date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
      } catch (error) {
        console.error('Saat formatlanırken hata:', error);
        return '';
      }
    }
  },
  computed: {
    userCommunities() {
      return this.studentData.joinedClubs || [];
    },
    userEvents() {
      return this.studentData.upcomingEvents || [];
    }
  }
}
</script>

<style scoped>
.profile-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.profile-header {
  display: flex;
  gap: 2rem;
  margin-bottom: 2rem;
  padding: 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.profile-avatar {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

/* Loading States */
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
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top: 4px solid #3498db;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
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
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
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

.activities-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.activity-date {
  color: #666;
  font-size: 0.9rem;
  min-width: 80px;
}

.activity-info {
  flex: 1;
}

.activity-info h3 {
  margin: 0 0 0.25rem 0;
  color: #2c3e50;
  font-size: 1rem;
}

.activity-info p {
  margin: 0;
  color: #666;
}

.activity-status {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
  text-transform: capitalize;
}

.activity-status.create {
  background-color: #2ecc71;
  color: white;
}

.activity-status.update {
  background-color: #3498db;
  color: white;
}

.activity-status.delete {
  background-color: #e74c3c;
  color: white;
}

.activity-status.system {
  background-color: #f39c12;
  color: white;
}

.security-options {
  display: flex;
  flex-direction: column;
  gap: 1rem;
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
  font-size: 1rem;
}

.security-info p {
  margin: 0;
  color: #666;
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

.btn-danger {
  background-color: #e74c3c;
  color: white;
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
    text-align: center;
    gap: 1rem;
  }

  .activity-item {
    flex-direction: column;
    text-align: center;
  }

  .activity-status {
    margin-top: 0.5rem;
  }
}

.communities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
}

.community-card {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.community-card:hover {
  transform: translateY(-5px);
}

.community-image {
  position: relative;
  overflow: hidden;
}

.image-placeholder {
  height: 150px;
  background-color: #e9ecef;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #6c757d;
  font-weight: bold;
  overflow: hidden;
}

.image-placeholder img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.status-badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  color: white;
  font-size: 0.8rem;
}

.status-badge.active {
  background: #2ecc71;
}

.status-badge.inactive {
  background: #95a5a6;
}

.community-info {
  padding: 1.5rem;
}

.community-info h3 {
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
  font-size: 1.2rem;
}

.category {
  color: #666;
  font-size: 0.9rem;
  margin: 5px 0;
}

.stats {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
  color: #666;
  font-size: 0.9rem;
}

.stats span {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.stats i {
  color: #3498db;
}

.events-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-top: 1rem;
}

.event-card {
  background: white;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  gap: 1rem;
  transition: transform 0.2s;
}

.event-card:hover {
  transform: translateY(-5px);
}

.event-date {
  background: #f0f0f0;
  border-radius: 8px;
  padding: 0.5rem;
  text-align: center;
  min-width: 60px;
}

.event-date .day {
  display: block;
  font-size: 1.5rem;
  font-weight: bold;
}

.event-date .month {
  display: block;
  font-size: 0.8rem;
  color: #666;
}

.event-info {
  flex: 1;
}

.event-info h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
}

.event-info p {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.event-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.8rem;
  color: #666;
}

.event-meta i {
  margin-right: 0.3rem;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  background: #f8f9fa;
  border-radius: 8px;
  margin-top: 1rem;
}

.empty-state p {
  color: #666;
  margin-bottom: 1rem;
}
</style>
