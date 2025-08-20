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
        <h2>Sistem Bilgileri</h2>
        <div class="info-grid">
          <div class="info-item">
            <label>Toplam Topluluk</label>
            <p>{{ systemStats.communities }}</p>
          </div>
          <div class="info-item">
            <label>Toplam Etkinlik</label>
            <p>{{ systemStats.events }}</p>
          </div>
          <div class="info-item">
            <label>Toplam Kullanıcı</label>
            <p>{{ systemStats.users }}</p>
          </div>
          <div class="info-item">
            <label>Toplam Duyuru</label>
            <p>{{ systemStats.announcements }}</p>
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
import { authService, clubService, eventService, announcementService } from '@/services';
import api from '@/services/api';

export default {
  name: 'AdminProfile',
  data() {
    return {
      loading: false,
      error: null,
      user: {
        name: 'Admin Kullanıcı',
        email: 'admin@elasoft.edu.tr',
        phone: '',
        studentId: '',
        department: '',
        departmentId: null,
        role: 'admin',
        avatar: 'https://randomuser.me/api/portraits/men/75.jpg',
        joinDate: new Date().toLocaleDateString('tr-TR'),
        twoFactorEnabled: false,
        lastPasswordChange: new Date().toLocaleDateString('tr-TR')
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
      }
    }
  },
  created() {
    // Component oluşturulduğunda kullanıcı bilgilerini ve istatistikleri getir
    this.fetchUserProfile();
    this.fetchSystemStats();
  },
  methods: {
    async fetchUserProfile() {
      this.loading = true;
      this.error = null;

      // localStorage'dan mevcut kullanıcı bilgilerini al
      const localUser = authService.getUser();

      try {
        // Önce kullanıcı listesinden bilgileri almayı deneyelim (telefon numarası için)
        try {
          const usersResponse = await api.get('/users');
          if (usersResponse && usersResponse.data && usersResponse.data.items &&
            Array.isArray(usersResponse.data.items) && usersResponse.data.items.length > 0) {

            // Kullanıcı ID'sine göre filtreleme yapalım
            const userId = localUser?.id;
            if (userId) {
              const userInfo = usersResponse.data.items.find(u => u.id === userId);
              if (userInfo) {
                // Telefon numarasını ve diğer bilgileri alalım
                this.user = {
                  ...this.user,
                  id: userInfo.id,
                  name: userInfo.name || localUser?.name || 'Admin Kullanıcı',
                  email: userInfo.email || localUser?.email || 'admin@elasoft.edu.tr',
                  phone: userInfo.phone || localUser?.phone || '-',
                  studentId: userInfo.studentId || localUser?.studentId || '',
                  department: userInfo.department || localUser?.department || '',
                  departmentId: userInfo.departmentId || localUser?.departmentId || null,
                  avatar: localUser?.avatar || 'https://randomuser.me/api/portraits/men/75.jpg',
                  joinDate: userInfo.joinDate
                    ? new Date(userInfo.joinDate).toLocaleDateString('tr-TR')
                    : new Date().toLocaleDateString('tr-TR'),
                  lastPasswordChange: localUser?.passwordUpdatedAt
                    ? new Date(localUser.passwordUpdatedAt).toLocaleDateString('tr-TR')
                    : new Date().toLocaleDateString('tr-TR'),
                  twoFactorEnabled: localUser?.twoFactorEnabled || false,
                  role: userInfo.role || localUser?.role || 'admin'
                };

                // Kullanıcı bilgilerini yerel depolamaya kaydet
                authService.saveUser({
                  ...localUser,
                  name: this.user.name,
                  email: this.user.email,
                  phone: this.user.phone,
                  studentId: this.user.studentId,
                  departmentId: this.user.departmentId,
                  role: this.user.role,
                  avatar: this.user.avatar,
                });

                // Edit formunu güncelle
                this.updateEditForm();
                this.loading = false;
                return;
              }
            }
          }
        } catch (userListError) {
          console.error('Kullanıcı listesi alınırken hata:', userListError);
          // Hata durumunda diğer yaklaşımla devam edilecek
        }

        // Kullanıcı listesinden bilgi alamazsak, whoami ile devam et
        const response = await authService.getCurrentUser();

        if (response && response.data) {
          // API yanıtını işle ve kullanıcı verilerini güncelle
          // AuthController'ın whoami endpointi userId, email ve role bilgilerini döndürüyor
          const userData = response.data;

          // Eğer UserId varsa, detaylı kullanıcı bilgilerini getir
          if (userData.UserId) {
            try {
              // Telefon ve diğer detaylı bilgileri almak için users/:id endpoint'ini kullan
              const userDetails = await api.get(`/users/${userData.UserId}`);
              if (userDetails && userDetails.data) {
                this.user = {
                  id: userData.UserId,
                  name: userDetails.data.name || localUser?.name || 'Admin Kullanıcı',
                  email: userData.Email || userDetails.data.email || localUser?.email || 'admin@elasoft.edu.tr',
                  phone: userDetails.data.phone || localUser?.phone || '-',
                  studentId: userDetails.data.studentId || localUser?.studentId || '-',
                  department: userDetails.data.department || localUser?.department || '',
                  departmentId: userDetails.data.departmentId || localUser?.departmentId || null,
                  // Avatarı kontrol et ve varsayılan avatar ekle
                  avatar: userDetails.data.avatar || localUser?.avatar || 'https://randomuser.me/api/portraits/men/75.jpg',
                  // Tarih formatını düzenleme
                  joinDate: userDetails.data.joinDate || localUser?.joinDate
                    ? new Date(userDetails.data.joinDate || localUser.joinDate).toLocaleDateString('tr-TR')
                    : new Date().toLocaleDateString('tr-TR'),
                  lastPasswordChange: userDetails.data.passwordUpdatedAt || localUser?.passwordUpdatedAt
                    ? new Date(userDetails.data.passwordUpdatedAt || localUser.passwordUpdatedAt).toLocaleDateString('tr-TR')
                    : new Date().toLocaleDateString('tr-TR'),
                  twoFactorEnabled: userDetails.data.twoFactorEnabled || localUser?.twoFactorEnabled || false,
                  role: userDetails.data.role || userData.Role || localUser?.role || 'admin'
                };

                // Kullanıcı bilgilerini yerel depolamaya kaydet
                authService.saveUser({
                  ...localUser,
                  name: this.user.name,
                  email: this.user.email,
                  phone: this.user.phone,
                  studentId: this.user.studentId,
                  departmentId: this.user.departmentId,
                  role: this.user.role,
                  avatar: this.user.avatar,
                });
              }
            } catch (detailError) {
              console.error('Kullanıcı detayları alınırken hata:', detailError);
              // Hata durumunda whoami yanıtı ve localStorage ile devam et
              this.setUserFromBasicData(userData, localUser);
            }
          } else {
            // UserId yoksa temel veriler ve localStorage'dan al
            this.setUserFromBasicData(userData, localUser);
          }

          // Edit formunu güncelle
          this.updateEditForm();
        } else {
          // Veri yoksa localUser veya test verisi kullan
          console.warn('API kullanıcı verisi dönmedi, localStorage veya test verisi kullanılıyor');
          if (localUser) {
            this.user = {
              ...localUser,
              phone: localUser.phone || '',
              studentId: localUser.studentId || '',
              departmentId: localUser.departmentId || null,
              department: localUser.department || '',
              avatar: localUser.avatar || 'https://randomuser.me/api/portraits/men/75.jpg',
              joinDate: localUser.joinDate
                ? new Date(localUser.joinDate).toLocaleDateString('tr-TR')
                : new Date().toLocaleDateString('tr-TR'),
              lastPasswordChange: localUser.passwordUpdatedAt
                ? new Date(localUser.passwordUpdatedAt).toLocaleDateString('tr-TR')
                : new Date().toLocaleDateString('tr-TR'),
              twoFactorEnabled: localUser.twoFactorEnabled || false,
              role: localUser.role || 'admin'
            };

            // Telefon numarası lokalda yoksa ekle
            if (!localUser.phone) {
              localUser.phone = this.user.phone;
              authService.saveUser(localUser);
            }

            if (localUser.departmentId === undefined) {
              localUser.departmentId = this.user.departmentId;
            }
            if (localUser.studentId === undefined) {
              localUser.studentId = this.user.studentId;
            }
            if (localUser.role === undefined) {
              localUser.role = this.user.role;
            }
            if (localUser.name === undefined) {
              localUser.name = this.user.name;
            }
            authService.saveUser(localUser);
          }
        }
      } catch (error) {
        console.error('Kullanıcı bilgileri alınırken hata:', error);
        this.error = 'Kullanıcı bilgileri alınamadı. Lütfen daha sonra tekrar deneyin.';

        // API bağlantısı yoksa localStorage'daki kullanıcı bilgilerini göster
        if (localUser) {
          this.user = {
            ...localUser,
            phone: localUser.phone || '',
            studentId: localUser.studentId || '',
            departmentId: localUser.departmentId || null,
            department: localUser.department || '',
            avatar: localUser.avatar || 'https://randomuser.me/api/portraits/men/75.jpg',
            joinDate: localUser.joinDate
              ? new Date(localUser.joinDate).toLocaleDateString('tr-TR')
              : new Date().toLocaleDateString('tr-TR'),
            lastPasswordChange: localUser.passwordUpdatedAt
              ? new Date(localUser.passwordUpdatedAt).toLocaleDateString('tr-TR')
              : new Date().toLocaleDateString('tr-TR'),
            twoFactorEnabled: localUser.twoFactorEnabled || false,
            role: localUser.role || 'admin'
          };

          this.error = null;

          // Telefon numarası lokalda yoksa ekle
          if (!localUser.phone) {
            localUser.phone = this.user.phone;
            authService.saveUser(localUser);
          }

          if (localUser.departmentId === undefined) {
            localUser.departmentId = this.user.departmentId;
          }
          if (localUser.studentId === undefined) {
            localUser.studentId = this.user.studentId;
          }
          if (localUser.role === undefined) {
            localUser.role = this.user.role;
          }
          if (localUser.name === undefined) {
            localUser.name = this.user.name;
          }
          authService.saveUser(localUser);
        }
      } finally {
        this.loading = false;
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
        department: this.user.department || '',
        password: ''
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
    }
  }
}
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

@media (max-width: 992px) {
  .info-grid {
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  }
}

@media (max-width: 768px) {
  .profile-page {
    padding: 15px;
  }
  .profile-header {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
  .profile-content {
    gap: 1.5rem;
  }
  .profile-section {
    padding: 15px;
  }
  .info-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  .security-options {
    gap: 1rem;
  }
  .security-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  .security-item .btn {
    width: 100%;
    text-align: center;
  }
  .modal-content {
    max-width: 95%;
  }
  .modal-body form .form-group {
    margin-bottom: 1rem;
  }
  .modal-body .form-row {
    flex-direction: column;
    gap: 0;
    grid-template-columns: 1fr;
  }
  .modal-body .form-row .form-group {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .profile-avatar {
    margin-bottom: 1rem;
  }
  .profile-info h1 {
    font-size: 1.8rem;
  }
  .profile-section h2 {
    font-size: 1.3rem;
  }
  .modal-footer {
    flex-direction: column;
    gap: 10px;
  }
  .modal-footer button {
    width: 100%;
  }
}
</style>
