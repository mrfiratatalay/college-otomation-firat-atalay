<template>
  <div class="admin-members">
    <div class="page-header">
      <h1>Üyeler</h1>
      <button @click="showAddModal = true" class="add-btn">
        <i class="fas fa-plus"></i>
        Yeni Üye
      </button>
    </div>

    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Üye ara..."
        >
      </div>
      <div class="filter-group">
        <select v-model="roleFilter">
          <option value="all">Tüm Roller</option>
          <option value="admin">Yönetici</option>
          <option value="advisor">Danışman</option>
          <option value="user">Üye</option>
        </select>
        <select v-model="statusFilter">
          <option value="all">Tüm Durumlar</option>
          <option value="active">Aktif</option>
          <option value="inactive">Pasif</option>
          <option value="pending">Onay Bekleyen</option>
        </select>
        <select v-model="departmentFilter">
          <option value="all">Tüm Bölümler</option>
          <option v-for="dept in departments" :key="dept.id" :value="dept.id">
            {{ dept.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- Yetkilendirme hatası mesajı -->
    <div v-if="authError" class="auth-error">
      <i class="fas fa-exclamation-triangle"></i>
      <p>{{ authErrorMessage }}</p>
      <button @click="navigateToLogin" class="login-btn">Giriş Yap</button>
    </div>

    <div v-else class="members-table">
      <div v-if="isLoading" class="loading-overlay">
        <div class="loading-spinner">
          <i class="fas fa-spinner fa-spin"></i>
          <span>Yükleniyor...</span>
        </div>
      </div>
      
      <table>
        <thead>
          <tr>
            <th>Üye</th>
            <th>E-posta</th>
            <th>Bölüm</th>
            <th>Rol</th>
            <th>Durum</th>
            <th>Kayıt Tarihi</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="paginatedMembers.length === 0 && !isLoading">
            <td colspan="7" class="no-data">Üye bulunamadı</td>
          </tr>
          <tr v-for="member in paginatedMembers" :key="member.id">
            <td data-label="Üye" class="member-info">
              <img :src="member.avatar || '/images/default-avatar.png'" :alt="member.name" class="member-avatar">
              <div class="member-details">
                <span class="member-name">{{ member.name }}</span>
                <span class="member-id">{{ member.studentId }}</span>
              </div>
            </td>
            <td data-label="E-posta">{{ member.email }}</td>
            <td data-label="Bölüm">{{ member.department }}</td>
            <td data-label="Rol">
              <span class="role-badge" :class="member.role">
                {{ getRoleText(member.role) }}
              </span>
            </td>
            <td data-label="Durum">
              <span class="status-badge" :class="member.status">
                {{ getStatusText(member.status) }}
              </span>
            </td>
            <td data-label="Kayıt Tarihi">{{ formatDate(member.joinDate) }}</td>
            <td data-label="İşlemler" class="actions">
              <button @click="editMember(member)" class="edit-btn" title="Düzenle">
                <i class="fas fa-edit"></i>
              </button>
              <button @click="viewDetails(member)" class="view-btn" title="Detaylar">
                <i class="fas fa-eye"></i>
              </button>
              <button @click="deleteMember(member.id)" class="delete-btn" title="Sil">
                <i class="fas fa-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination Controls -->
      <div class="pagination">
        <button 
          @click="currentPage--" 
          :disabled="currentPage === 1" 
          class="pagination-btn"
        >
          <i class="fas fa-chevron-left"></i>
        </button>
        
        <template v-if="totalPages <= 7">
          <button 
            v-for="page in totalPages" 
            :key="page" 
            @click="currentPage = page" 
            :class="['pagination-btn', { active: currentPage === page }]"
          >
            {{ page }}
          </button>
        </template>
        
        <template v-else>
          <!-- First page -->
          <button 
            @click="currentPage = 1" 
            :class="['pagination-btn', { active: currentPage === 1 }]"
          >
            1
          </button>
          
          <!-- Ellipsis for skipped pages at beginning -->
          <span v-if="currentPage > 3" class="pagination-ellipsis">...</span>
          
          <!-- Pages around current page -->
          <template v-for="page in totalPages" :key="page">
            <button 
              v-if="page >= currentPage - 1 && page <= currentPage + 1 && page > 1 && page < totalPages"
              @click="currentPage = page" 
              :class="['pagination-btn', { active: currentPage === page }]"
            >
              {{ page }}
            </button>
          </template>
          
          <!-- Ellipsis for skipped pages at end -->
          <span v-if="currentPage < totalPages - 2" class="pagination-ellipsis">...</span>
          
          <!-- Last page -->
          <button 
            @click="currentPage = totalPages" 
            :class="['pagination-btn', { active: currentPage === totalPages }]"
          >
            {{ totalPages }}
          </button>
        </template>
        
        <button 
          @click="currentPage++" 
          :disabled="currentPage === totalPages" 
          class="pagination-btn"
        >
          <i class="fas fa-chevron-right"></i>
        </button>
      </div>
    </div>

    <!-- Üye Ekleme/Düzenleme Modalı -->
    <div v-if="showAddModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ editingMember ? 'Üye Düzenle' : 'Yeni Üye' }}</h2>
          <button @click="closeModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveMember">
            <div class="form-row">
              <div class="form-group">
                <label>Ad</label>
                <input v-model="memberForm.firstName" type="text" required>
              </div>
              <div class="form-group">
                <label>Soyad</label>
                <input v-model="memberForm.lastName" type="text" required>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Öğrenci No</label>
                <input v-model="memberForm.studentId" type="text">
              </div>
              <div class="form-group">
                <label>E-posta</label>
                <input v-model="memberForm.email" type="email" required>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Telefon</label>
                <input v-model="memberForm.phone" type="tel" required>
              </div>
              <div class="form-group">
                <label>Şifre {{ editingMember ? '(şifre değiştirmek için boş bırakınız)' : '' }}</label>
                <input v-model="memberForm.password" type="password" :required="!editingMember">
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Bölüm</label>
                <select v-model="memberForm.departmentId" required>
                  <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                    {{ dept.name }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>Rol</label>
                <select v-model="memberForm.role" required>
                  <option value="admin">Yönetici</option>
                  <option value="advisor">Danışman</option>
                  <option value="user">Üye</option>
                </select>
              </div>
            </div>
            <div class="form-group">
              <label>Profil Fotoğrafı</label>
              <div class="image-upload">
                <input type="file" @change="handleImageUpload" accept="image/*">
                <div class="upload-preview" v-if="memberForm.avatar">
                  <img :src="memberForm.avatar" alt="Preview">
                </div>
              </div>
            </div>
            <div class="form-actions">
              <button type="button" @click="closeModal" class="cancel-btn">İptal</button>
              <button type="submit" class="save-btn" :disabled="isLoading">
                <span v-if="isLoading"><i class="fas fa-spinner fa-spin"></i> Kaydediliyor...</span>
                <span v-else>Kaydet</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Üye Detay Modalı -->
    <div v-if="selectedMember" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Üye Detayları</h2>
          <button @click="selectedMember = null" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="member-profile">
            <div class="profile-header">
              <img :src="selectedMember.avatar" :alt="selectedMember.name" class="profile-avatar">
              <div class="profile-info">
                <h3>{{ selectedMember.name }}</h3>
                <p class="student-id">{{ selectedMember.studentId }}</p>
              </div>
            </div>
            <div class="profile-details">
              <div class="detail-group">
                <label>E-posta</label>
                <p>{{ selectedMember.email }}</p>
              </div>
              <div class="detail-group">
                <label>Telefon</label>
                <p>{{ selectedMember.phone }}</p>
              </div>
              <div class="detail-group">
                <label>Bölüm</label>
                <p>{{ selectedMember.department }}</p>
              </div>
              <div class="detail-group">
                <label>Rol</label>
                <p>
                  <span class="role-badge" :class="selectedMember.role">
                    {{ getRoleText(selectedMember.role) }}
                  </span>
                </p>
              </div>
              <div class="detail-group">
                <label>Durum</label>
                <p>
                  <span class="status-badge" :class="selectedMember.status">
                    {{ getStatusText(selectedMember.status) }}
                  </span>
                </p>
              </div>
              <div class="detail-group">
                <label>Kayıt Tarihi</label>
                <p>{{ formatDate(selectedMember.joinDate) }}</p>
              </div>
            </div>
            <div class="profile-stats">
              <div class="stat-item">
                <i class="fas fa-users"></i>
                <span>{{ selectedMember.communityCount }} Topluluk</span>
              </div>
              <div class="stat-item">
                <i class="fas fa-calendar-check"></i>
                <span>{{ selectedMember.eventCount }} Etkinlik</span>
              </div>
              <div class="stat-item">
                <i class="fas fa-comment"></i>
                <span>{{ selectedMember.commentCount }} Yorum</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import { debounce } from 'lodash'
import api from '@/services/api' // API servisini import ediyoruz

export default {
  name: 'AdminMembers',
  data() {
    return {
      searchQuery: '',
      roleFilter: 'all',
      statusFilter: 'all',
      departmentFilter: 'all',
      showAddModal: false,
      editingMember: null,
      selectedMember: null,
      isLoading: false,
      currentPage: 1,
      itemsPerPage: 10,
      totalItems: 0,
      totalPages: 1,
      departments: [],
      memberForm: {
        firstName: '',
        lastName: '',
        studentId: '',
        email: '',
        phone: '',
        departmentId: '',
        role: 'member',
        password: '',
        avatar: null
      },
      members: [],
      authError: false,
      authErrorMessage: ''
    }
  },
  computed: {
    paginatedMembers() {
      return this.members;
    }
  },
  watch: {
    searchQuery() {
      this.debouncedSearch();
    },
    roleFilter() {
      this.currentPage = 1;
      this.fetchMembers();
    },
    statusFilter() {
      this.currentPage = 1;
      this.fetchMembers();
    },
    departmentFilter() {
      this.currentPage = 1;
      this.fetchMembers();
    },
    currentPage() {
      this.fetchMembers();
    }
  },
  created() {
    this.debouncedSearch = debounce(() => {
      this.currentPage = 1;
      this.fetchMembers();
    }, 500);
    
    this.fetchMembers();
    this.fetchDepartments();
  },
  methods: {
    async fetchDepartments() {
      try {
        const response = await axios.get('/api/Users/departments');
        this.departments = response.data;
      } catch (error) {
        console.error('Error fetching departments:', error);
        // Fallback to static data if API fails
        this.departments = [
          { id: 1, name: 'Bilgisayar Mühendisliği' },
          { id: 2, name: 'Elektrik-Elektronik Mühendisliği' },
          { id: 3, name: 'Makine Mühendisliği' },
          { id: 4, name: 'Endüstri Mühendisliği' }
        ];
      }
    },
    async fetchMembers() {
      this.isLoading = true;
      this.authError = false; // Reset auth error state
      
      try {
        console.log('API isteği yapılıyor...');
        
        // Token alınması ve isteğe eklenmesi
        const token = localStorage.getItem('token');
        
        if (!token) {
          console.error('Kimlik doğrulama token\'ı bulunamadı. Lütfen giriş yapın.');
          this.authError = true;
          this.authErrorMessage = 'Bu sayfayı görüntülemek için giriş yapmanız gerekiyor.';
          this.isLoading = false;
          return;
        }
        
        // Backend API endpoint'e authorization header ekliyoruz
        const response = await axios.get('/api/Users', {
          params: {
            search: this.searchQuery,
            role: this.roleFilter,
            status: this.statusFilter,
            page: this.currentPage,
            pageSize: this.itemsPerPage
          },
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        
        console.log('API yanıtı:', response.data);
        
        if (response.data && response.data.items) {
          this.members = response.data.items.map(user => {
            // Debug için rol bilgisini konsola yazdır
            console.log(`Kullanıcı ${user.name}, rol: ${user.role}, departman: ${user.department}, departmanId: ${user.departmentId}`);
            
            return {
              id: user.id,
              name: user.name,
              email: user.email,
              studentId: user.studentId,
              phone: user.phone,
              department: user.department,
              departmentId: user.departmentId,
              role: user.role,
              status: user.status,
              avatar: user.avatar || '/images/default-avatar.png',
              joinDate: user.joinDate
            };
          });
          
          this.totalItems = response.data.totalItems;
          this.totalPages = response.data.totalPages;
        } else {
          console.error('API yanıtı beklenen formatta değil:', response.data);
          this.members = [];
          this.totalItems = 0;
          this.totalPages = 1;
        }
      } catch (error) {
        console.error('Backend bağlantı hatası:', error);
        
        // Hata detaylarını gösterelim
        if (error.response) {
          // Sunucudan yanıt geldi ama hata kodu ile
          console.error('Hata yanıtı:', error.response.data);
          console.error('Hata durumu:', error.response.status);
          console.error('Hata başlıkları:', error.response.headers);
          
          // 401 Unauthorized veya 403 Forbidden durumlarında özel hata mesajı
          if (error.response.status === 401) {
            this.authError = true;
            this.authErrorMessage = 'Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.';
          } else if (error.response.status === 403) {
            this.authError = true;
            this.authErrorMessage = 'Bu sayfaya erişim yetkiniz bulunmuyor. Sadece yöneticiler erişebilir.';
          }
        } else if (error.request) {
          // İstek yapıldı ama yanıt alınamadı
          console.error('Yanıt alınamadı:', error.request);
        } else {
          // İstek kurulurken bir şeyler ters gitti
          console.error('İstek hatası:', error.message);
        }
        
        this.members = [];
        this.totalItems = 0;
        this.totalPages = 1;
      } finally {
        this.isLoading = false;
      }
    },
    getRoleText(role) {
      console.log('Role text requested for:', role);
      const roleMap = {
        admin: 'Yönetici',
        user: 'Üye',
        advisor: 'Danışman' 
      }
      const result = roleMap[role] || 'Üye' // Bilinmeyen roller için 'Üye' göster
      console.log('Returning role text:', result);
      return result;
    },
    getStatusText(status) {
      const statusMap = {
        active: 'Aktif',
        inactive: 'Pasif',
        pending: 'Onay Bekleyen'
      }
      return statusMap[status] || status
    },
    formatDate(dateString) {
      const options = { year: 'numeric', month: 'long', day: 'numeric' }
      return new Date(dateString).toLocaleDateString('tr-TR', options)
    },
    filterMembers() {
      // Filtering is now handled by API calls via watchers
      this.currentPage = 1;
      this.fetchMembers();
    },
    async viewDetails(member) {
      this.isLoading = true;
      try {
        // Fetch detailed member information
        const response = await axios.get(`/api/Users/${member.id}`);
        this.selectedMember = response.data;
      } catch (error) {
        console.error('Error fetching member details:', error);
        // Fallback to basic info if API call fails
        this.selectedMember = {
          ...member,
          communityCount: member.communityCount || 0,
          eventCount: member.eventCount || 0,
          commentCount: member.commentCount || 0
        };
      } finally {
        this.isLoading = false;
      }
    },
    async editMember(member) {
      // Split the name into first name and last name
      const nameParts = member.name.split(' ');
      const firstName = nameParts.shift(); // First part is first name
      const lastName = nameParts.join(' '); // Rest is last name
      
      this.editingMember = member;
      this.memberForm = {
        firstName: firstName,
        lastName: lastName,
        studentId: member.studentId,
        email: member.email,
        phone: member.phone,
        departmentId: member.departmentId,
        role: member.role,
        avatar: member.avatar,
        password: '' // Don't pre-fill password
      };
      this.showAddModal = true;
    },
    async deleteMember(id) {
      if (confirm('Bu üyeyi silmek istediğinizden emin misiniz?')) {
        this.isLoading = true;
        try {
          // Token alınması
          const token = localStorage.getItem('token');
          
          if (!token) {
            alert('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
            this.isLoading = false;
            return;
          }
          
          // Authorization header'ı ile delete isteği
          await axios.delete(`/api/Users/${id}`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
          
          this.fetchMembers(); // Refresh list after delete
        } catch (error) {
          console.error('Error deleting member:', error);
          alert('Üye silinemedi. Lütfen tekrar deneyiniz.');
        } finally {
          this.isLoading = false;
        }
      }
    },
    handleImageUpload(event) {
      const file = event.target.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = e => {
          this.memberForm.avatar = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    },
    async saveMember() {
      this.isLoading = true;
      try {
        // Token alınması
        const token = localStorage.getItem('token');
        
        if (!token) {
          alert('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
          this.isLoading = false;
          return;
        }
        
        // Log the editing member to see what data we're updating
        console.log("Editing member:", this.editingMember);
        
        if (this.editingMember) {
          // Danışman profilinde çalışan yaklaşımı kullanarak güncelleme
          console.log("Düzenleme öncesi debugger çıktısı:", this.editingMember, this.memberForm);
          
          // Danışman profilinde olduğu gibi tüm gerekli alanları ekleyelim
          const updateData = {
            FirstName: this.memberForm.firstName,
            LastName: this.memberForm.lastName,
            Phone: this.memberForm.phone,
            StudentId: this.memberForm.studentId || '',
            Role: this.memberForm.role || 'user',
            DepartmentId: this.memberForm.departmentId ? parseInt(this.memberForm.departmentId) : null
          };
          
          // Only include password if it's not empty
          if (this.memberForm.password && this.memberForm.password.trim() !== '') {
            updateData.Password = this.memberForm.password.trim();
          }
          
          // Debug bilgisi
          console.log("Gönderilen form verisi:", updateData);
          
          try {
            // api servisini kullanarak istek gönder
            // başarılı response için direkt axios kullanarak deneme yapalım
            const response = await axios({
              method: 'PUT',
              url: `https://localhost:7274/api/Users/${this.editingMember.id}`,
              headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                'Accept': 'application/json'
              },
              data: updateData,
              validateStatus: function (status) {
                // Tüm durum kodlarını kabul et, böylece detaylı hata incelemesi yapabiliriz
                return true;
              }
            });
            
            console.log("Tam API cevabı:", response);
            
            if (response.status >= 200 && response.status < 300) {
              this.closeModal();
              this.fetchMembers(); // Refresh the list
            } else {
              console.error("HTTP Hata Detayı:", response.status, response.statusText, response.data);
              alert(`Üye güncellenirken hata oluştu (HTTP ${response.status}): ${response.data?.message || response.statusText}`);
            }
          } catch (error) {
            console.error("Update error:", error);
            if (error.response) {
              console.log("Hata detayları:", error.response.status, error.response.data);
              const errorDetails = error.response.data?.errors ? JSON.stringify(error.response.data.errors) : '';
              alert(`Üye güncellenirken hata oluştu (${error.response.status}): ${error.response.data?.message || error.message}\n${errorDetails}`);
            } else {
              alert(`Üye güncellenirken hata oluştu: ${error.message}`);
            }
          }
        } else {
          // For new user creation
          const createData = {
            FirstName: this.memberForm.firstName,
            LastName: this.memberForm.lastName,
            Email: this.memberForm.email,
            Phone: this.memberForm.phone,
            Password: this.memberForm.password,
            Role: this.memberForm.role
          };
          
          // Only include optional fields if they have values
          if (this.memberForm.studentId) {
            createData.StudentId = this.memberForm.studentId;
          }
          
          if (this.memberForm.departmentId) {
            createData.DepartmentId = parseInt(this.memberForm.departmentId);
          }
          
          // For new user, password is required
          if (!createData.Password) {
            alert('Şifre zorunludur');
            this.isLoading = false;
            return;
          }
          
          console.log("Creating new user with data:", createData);
          
          try {
            // api servisini kullanarak istek gönder
            const response = await api.post('/Users', createData);
            
            console.log("Create response:", response);
            
            this.closeModal();
            this.fetchMembers(); // Refresh the list
          } catch (error) {
            console.error("Create error:", error);
            const errorMessage = error.response?.data?.message || error.message || 'Üye eklenirken hata oluştu';
            alert(`Üye eklenirken hata oluştu: ${errorMessage}`);
          }
        }
      } catch (error) {
        console.error('Error saving member:', error);
      } finally {
        this.isLoading = false;
      }
    },
    closeModal() {
      this.showAddModal = false;
      this.editingMember = null;
      this.memberForm = {
        firstName: '',
        lastName: '',
        studentId: '',
        email: '',
        phone: '',
        departmentId: '',
        role: 'member',
        password: '',
        avatar: null
      };
    },
    // Kullanıcıyı giriş sayfasına yönlendirme
    navigateToLogin() {
      window.location.href = '/login';
    }
  }
}
</script>

<style scoped>
.admin-members {
  padding: 1rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
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

.members-table {
  position: relative;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
}

.member-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.member-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}

.member-details {
  display: flex;
  flex-direction: column;
}

.member-name {
  font-weight: 500;
  color: #2c3e50;
}

.member-id {
  font-size: 0.9rem;
  color: #7f8c8d;
}

.role-badge,
.status-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  color: white;
  display: inline-block;
}

.role-badge.admin {
  background: #e74c3c;
}

.role-badge.user {
  background: #3498db;
}

.role-badge.advisor {
  background: #9b59b6;
}

.status-badge.active {
  background: #2ecc71;
}

.status-badge.inactive {
  background: #95a5a6;
}

.status-badge.pending {
  background: #f39c12;
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.edit-btn,
.view-btn,
.delete-btn {
  padding: 0.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.edit-btn {
  background: #3498db;
  color: white;
}

.edit-btn:hover {
  background: #2980b9;
}

.view-btn {
  background: #2ecc71;
  color: white;
}

.view-btn:hover {
  background: #27ae60;
}

.delete-btn {
  background: #e74c3c;
  color: white;
}

.delete-btn:hover {
  background: #c0392b;
}

/* Modal Styles */
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
.form-group select {
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
  width: 100px;
  height: 100px;
  border-radius: 50%;
  overflow: hidden;
}

.upload-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
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

/* Member Profile Styles */
.member-profile {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #eee;
}

.profile-avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
}

.profile-info h3 {
  margin: 0 0 0.5rem;
  color: #2c3e50;
}

.student-id {
  color: #7f8c8d;
  margin: 0;
}

.profile-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.detail-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.detail-group p {
  margin: 0;
  color: #2c3e50;
}

.profile-stats {
  display: flex;
  gap: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #eee;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #2c3e50;
}

.stat-item i {
  color: #7f8c8d;
}

/* Pagination Styles */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 1.5rem;
  gap: 0.5rem;
}

.pagination-btn {
  min-width: 2.5rem;
  height: 2.5rem;
  display: flex;
  justify-content: center;
  align-items: center;
  border: 1px solid #ddd;
  background-color: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.pagination-btn:hover:not(:disabled) {
  background-color: #f0f0f0;
  border-color: #ccc;
}

.pagination-btn.active {
  background-color: #3498db;
  color: white;
  border-color: #3498db;
}

.pagination-btn:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.pagination-ellipsis {
  padding: 0 0.5rem;
}

/* Loading state */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 5;
}

.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  color: #3498db;
}

.loading-spinner i {
  font-size: 2rem;
}

.members-table {
  position: relative;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow-x: auto;
}

.no-data {
  text-align: center;
  padding: 2rem;
  color: #7f8c8d;
}

/* Auth Error Styles */
.auth-error {
  background-color: #fff8f8;
  border: 1px solid #ffcdd2;
  border-radius: 8px;
  padding: 2rem;
  margin: 2rem 0;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.auth-error i {
  font-size: 3rem;
  color: #e53935;
}

.auth-error p {
  color: #d32f2f;
  font-size: 1.2rem;
  margin: 0;
}

.login-btn {
  background-color: #2196f3;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.3s;
}

.login-btn:hover {
  background-color: #1976d2;
}

/* Responsive Styles */
@media (max-width: 992px) {
  /* Adjust filters slightly earlier if needed */
  .filters {
    flex-wrap: wrap;
  }
}

@media (max-width: 768px) {
  .admin-members {
    padding: 15px;
  }
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
    margin-bottom: 15px;
  }
  .filters {
    flex-direction: column;
    gap: 10px;
  }
  .filter-group {
    flex-direction: column;
    width: 100%;
    gap: 10px;
  }
  .search-box {
    width: 100%;
  }

  /* Responsive Table */
  .members-table table {
    border: 0;
  }
  .members-table thead {
    border: none;
    clip: rect(0 0 0 0);
    height: 1px;
    margin: -1px;
    overflow: hidden;
    padding: 0;
    position: absolute;
    width: 1px;
  }
  .members-table tr {
    border-bottom: 3px solid #ddd;
    display: block;
    margin-bottom: 1rem;
    background-color: #fff; /* Add background to stacked rows */
    box-shadow: 0 1px 3px rgba(0,0,0,0.1); /* Add subtle shadow */
    border-radius: 4px; /* Optional: round corners */
  }
  .members-table td {
    border-bottom: 1px solid #eee;
    display: block;
    font-size: .9em;
    text-align: right; /* Align content to the right */
    padding: 10px; /* Adjust padding */
    padding-left: 50%; /* Make space for label */
    position: relative; /* Needed for pseudo-element positioning */
  }
  .members-table td::before {
    content: attr(data-label);
    float: left;
    font-weight: bold;
    text-transform: uppercase;
    font-size: 0.8em;
    color: #666; /* Label color */
    position: absolute; /* Position label */
    left: 10px; /* Adjust left padding */
    width: 45%; /* Adjust width of label area */
    padding-right: 10px;
    white-space: nowrap;
    text-align: left; /* Align label text to the left */
  }
   .members-table td:last-child {
    border-bottom: 0;
  }
  .member-info,
  .actions {
    text-align: left; /* Override text-align for specific cells if needed */
  }
  .actions button {
    margin: 2px; /* Add small margin between buttons */
  }

  /* Adjust pagination */
  .pagination {
    flex-wrap: wrap;
    justify-content: center;
    gap: 5px;
  }

  /* Modal responsiveness */
  .modal-content {
    max-width: 95%;
    max-height: 90vh;
  }
  .modal-body {
    max-height: calc(85vh - 120px);
    overflow-y: auto;
  }
  .form-row {
    flex-direction: column;
    gap: 0;
    grid-template-columns: 1fr; /* Override grid for stacking */
  }
  .form-row .form-group {
     width: 100%;
  }
}

@media (max-width: 480px) {
   .actions button {
     padding: 5px 8px; /* Smaller buttons */
     font-size: 0.8rem;
   }
   .pagination-btn {
     padding: 5px 10px; /* Smaller pagination buttons */
   }
    .form-actions {
      flex-direction: column;
      gap: 10px;
    }
    .form-actions button {
      width: 100%;
    }
}
</style> 