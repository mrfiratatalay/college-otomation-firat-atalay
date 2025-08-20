<template>
  <div class="admin-communities">
    <div class="page-header">
      <h1>Topluluklar</h1>
      <div class="button-group">
        <button @click="showAddModal = true" class="add-btn">
          <i class="fas fa-plus"></i> Yeni Topluluk
        </button>
        <button @click="showListModal = true" class="add-btn" style="background-color: #3498db;">
          <i class="fas fa-list"></i> Tüm Toplulukları Listele
        </button>
        <button @click="showDateModal = true" class="save-btn" style="margin-left: 8px;">
          <i class="fas fa-calendar-alt"></i> Topluluk Oluşturma Tarihlerini Ayarla
        </button>
      </div>
    </div>
    <div v-if="showDateModal" class="modal" @click.self="showDateModal = false">
      <div class="modal-content" style="max-width:400px; margin:auto;">
        <div class="modal-header">
          <h2>Topluluk Oluşturma Tarihleri</h2>
          <button @click="showDateModal = false" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Başlangıç Tarihi</label>
            <input type="date" v-model="clubCreateStartDate">
          </div>
          <div class="form-group">
            <label>Bitiş Tarihi</label>
            <input type="date" v-model="clubCreateEndDate">
          </div>
        </div>
        <div class="modal-footer" style="display:flex; justify-content:flex-end; gap:8px;">
          <button @click="showDateModal = false" class="cancel-btn">İptal</button>
          <button @click="saveClubCreateDates" class="save-btn">Kaydet</button>
        </div>
      </div>
    </div>

    <div class="filters">
      <div class="search-box">
        <i class="fas fa-search"></i>

        <input type="text" v-model="searchQuery" placeholder="Topluluk ara..." @input="filterCommunities">
      </div>
      <div class="filter-group">
        <select v-model="statusFilter" @change="filterCommunities">
          <option value="all">Tüm Durumlar</option>
          <option value="active">Aktif</option>
          <option value="inactive">Pasif</option>
          <option value="pending">Onay Bekleyen</option>
        </select>
        <select v-model="categoryFilter" @change="filterCommunities">
          <option value="all">Tüm Kategoriler</option>
          <option v-for="category in categories" :key="category.id" :value="category.id">
            {{ category.name }}
          </option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Topluluklar yükleniyor...</p>
    </div>

    <div v-if="error" class="error-message">
      <p>{{ error }}</p>
      <button @click="fetchCommunities" class="retry-btn">
        <i class="fas fa-sync"></i> Tekrar Dene
      </button>
    </div>

    <div v-if="!loading && !error && communities.length === 0" class="empty-state">
      <p>Hiç topluluk bulunamadı.</p>
      <button @click="showAddModal = true" class="add-btn">
        <i class="fas fa-plus"></i> Yeni Topluluk Oluştur
      </button>
    </div>

    <div v-if="notification.show" class="notification" :class="`notification-${notification.type}`">
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


    <div v-if="!loading && communities.length > 0" class="communities-grid">
      <div v-for="community in filteredCommunities" :key="community.id" class="community-card">
        <div class="community-image">
          <img :src="community.image" :alt="community.name">
          <div class="status-badge" :class="community.status">
            {{ getStatusText(community.status) }}
          </div>
        </div>
        <div class="community-content flex-col-stretch">
          <div>
            <h3>{{ community.name }}</h3>
            <p class="category">{{ community.category }}</p>
            <div class="community-stats">
              <span>
                <i class="fas fa-users"></i>
                {{ community.memberCount }} Üye
              </span>
              <span>
                <i class="fas fa-calendar-check"></i>
                {{ community.eventCount }} Etkinlik
              </span>
            </div>
            <p class="description">{{ community.description }}</p>
          </div>
          <div class="community-actions fixed-bottom">
            <template v-if="community.status === 'pending'">
              <button @click="approveCommunity(community.id)" class="approve-btn">
                <i class="fas fa-check"></i>
                Onayla
              </button>
              <button @click="rejectCommunity(community.id)" class="reject-btn">
                <i class="fas fa-times"></i>
                Reddet
              </button>
            </template>
            <template v-else>
              <button @click="editCommunity(community)" class="edit-btn">
                <i class="fas fa-edit"></i>
                Düzenle
              </button>
              <button @click="deleteCommunity(community.id)" class="delete-btn">
                <i class="fas fa-trash"></i>
                Sil
              </button>
              <button @click="goToManagement(community.id)" class="manage-btn">
                <i class="fa fa-info-circle" aria-hidden="true"></i>
                Yönet
              </button>
            </template>
          </div>
        </div>
      </div>
    </div>
<!-- Toplulukları Listele Modali -->
<div v-if="showListModal" class="modal" @click="showListModal = false">
  <div class="modal-content" @click.stop>
    <div class="modal-header">
      <h2>Topluluk Listesi</h2>
      <button @click="showListModal = false" class="close-btn">
        <i class="fas fa-times"></i>
      </button>
    </div>

    <div class="modal-body">
      <div class="form-group">
        <input type="text" v-model="communitySearch" placeholder="Topluluk ara..." />
      </div>

      <div class="form-group">
        <select v-model="categoryFilterForModal">
          <option value="all">Tüm Kategoriler</option>
          <option v-for="category in categories" :key="category.id" :value="category.id">
            {{ category.name }}
          </option>
        </select>
      </div>
      <div class="form-group">
  <label>Şu tarihten sonra oluşturulanlar:</label>
  <input type="date" v-model="createdAfter" />
</div>


      <table style="width: 100%; border-collapse: collapse; margin-top: 1rem;">
        <thead>
          <tr>
            <th style="text-align: left; padding: 0.5rem;">Topluluk</th>
            <th style="text-align: left; padding: 0.5rem;">Kategori</th>
            <th style="text-align: left; padding: 0.5rem;">Üye Sayısı</th>
            <th style="text-align: left; padding: 0.5rem;">Oluşturulma Tarihi</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in filteredCommunitiesForModal" :key="c.id">
            <td style="padding: 0.5rem;">{{ c.name }}</td>
            <td style="padding: 0.5rem;">{{ getCategoryName(c.category) }}</td>
            <td style="padding: 0.5rem;">{{ c.memberCount }}</td>
            <td style="padding: 0.5rem;">{{ formatDateOnly(c.createdAt) }}</td>

          </tr>
        </tbody>
      </table>
    </div>
  </div>
</div>

    <!-- Topluluk Ekleme/Düzenleme Modalı -->
    <div v-if="showAddModal" class="modal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingCommunity ? 'Topluluk Düzenle' : 'Yeni Topluluk' }}</h2>
          <button @click="closeModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveCommunity">
            <div class="form-group">
              <label>Topluluk Adı</label>
              <input v-model="communityForm.name" type="text" required>
            </div>
            <div class="form-group">
              <label>Kategori</label>
              <select v-model="communityForm.category" required>
                <option v-if="categories.length === 0" value="" disabled>Kategori bulunamadı</option>
                <option v-for="category in categories" :key="category.id" :value="category.id">
                  {{ category.name }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Açıklama</label>
              <textarea v-model="communityForm.description" rows="4" required></textarea>
            </div>
            <div class="form-group">
              <label>Danışman</label>
              <select v-model="communityForm.advisorId" required>
                <option v-if="advisors.length === 0" value="" disabled>Danışman bulunamadı</option>
                <option v-for="advisor in advisors" :key="advisor.advisorId" :value="advisor.advisorId">
                  {{ advisor.fullName }}
                </option>
              </select>
            </div>
            <div class="form-group" v-if="editingCommunity">
              <label>Topluluk Başkanı</label>
              <select v-model="communityForm.presidentId">
                <option value="" disabled>Başkan seçiniz...</option>
                <option v-if="clubMembers.length === 0" value="" disabled>Bu toplulukta öğrenci üye yok</option>
                <option v-for="member in studentMembers" :key="member.userId" :value="member.userId">
                  {{ member.userName }} ({{ member.role }})
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Topluluk Belgesi</label>
              <input type="file" @change="handleDocumentUpload" accept=".pdf,.doc,.docx">
              <div v-if="communityForm.document" class="file-name">
                {{ communityForm.document.name }}
              </div>
            </div>
            <div class="form-group">
              <label>Topluluk Görseli</label>
              <div class="image-upload">
                <input type="file" @change="handleImageUpload" accept="image/*">
                <div class="upload-preview" v-if="communityForm.imagePreview">
                  <img :src="communityForm.imagePreview" alt="Preview">
                </div>
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
import { clubService, advisorService, categoryService } from '@/services';

export default {
  name: 'AdminCommunities',
  data() {
    return {
      showListModal: false,
communitySearch: '',
categoryFilterForModal: 'all',
createdAfter: '', 


      searchQuery: '',
      statusFilter: 'all',
      categoryFilter: 'all',
      showAddModal: false,
      editingCommunity: null,
      loading: false,
      error: null,
      categories: [],
      advisors: [],
      notification: {
        show: false,
        type: 'info',
        message: '',
        timeout: null,
        duration: 3000
      },
      communityForm: {
        name: '',
        category: '',
        description: '',
        advisorId: null,
        presidentId: null,
        document: null,
        image: null,
        imagePreview: null
      },
      clubMembers: [],
      communities: [],
      clubCreateStartDate: '',
      clubCreateEndDate: '',
      showDateModal: false,
    }
  },
  computed: {
    studentMembers() {
      // Sadece 'student' rolündeki üyeleri döndür
      return this.clubMembers.filter(member => 
        member.role?.toLowerCase() === 'başkan' ||
        member.role?.toLowerCase() === 'member'
      );
    },

    filteredCommunitiesForModal() {
  let list = this.communities;

  if (this.communitySearch) {
    const q = this.communitySearch.toLowerCase();
    list = list.filter(c => c.name.toLowerCase().includes(q));
  }

  if (this.categoryFilterForModal !== 'all') {
    list = list.filter(c => c.category == this.categoryFilterForModal);
  }
  if (this.createdAfter) {
    const selectedDate = new Date(this.createdAfter);
    list = list.filter(c => new Date(c.createdAt) >= selectedDate);
  }
  return list;
},

    filteredCommunities() {
      let filtered = this.communities;

      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(community =>
          community.name.toLowerCase().includes(query) ||
          community.description.toLowerCase().includes(query)
        );
      }

      if (this.statusFilter !== 'all') {
        filtered = filtered.filter(community => community.status === this.statusFilter);
      }

      if (this.categoryFilter !== 'all') {
        filtered = filtered.filter(community => community.category === this.categoryFilter);
      }

      return filtered;
    }
  },
  created() {
    // Component oluşturulduğunda tüm verileri getir
    this.fetchCommunities();
    this.fetchCategories();
    this.fetchAdvisors();
    this.fetchClubCreateDates();
  },
  methods: {
    formatDateOnly(dateStr) {
  if (!dateStr) return '-';
  const date = new Date(dateStr);
  return date.toISOString().split('T')[0];
},

  
    async fetchCommunities() {
      try {
        this.loading = true;
        this.error = null;

        // Backend API'dan topluluk verilerini al
        const response = await clubService.getAllClubs();

        if (response && response.data) {
          this.communities = response.data.map(club => ({
            id: club.clubId,
            name: club.name,
            category: club.categoryId,
            description: club.description,
            advisorId: club.advisorId,
            // API'den gelen veriye göre document ve image alanlarını ayarla
            document: club.documentUrl ? { name: club.documentUrl.split('/').pop() } : null,
            image: club.imageUrl || (club.image ? `data:image/jpeg;base64,${club.image}` : '/assets/logo.png'),
            status: club.status || 'inactive',
            memberCount: club.memberCount || 0,
            eventCount: club.eventCount || 0,
            createdAt: club.createdAt || null
          }));
        } else {
          // API veri dönmezse boş dizi kullan
          this.communities = [];
          console.warn('API\'den topluluk verisi alınamadı');
        }
      } catch (error) {
        console.error('Topluluklar yüklenirken hata:', error);
        this.error = 'Topluluklar yüklenirken bir hata oluştu.';

        // Hata durumunda toplulukları temizle
        this.communities = [];
      } finally {
        this.loading = false;
      }
    },

    async fetchCategories() {
      try {
        const response = await categoryService.getAllCategories();
        if (response && response.data) {
          this.categories = response.data;
        } else {
          this.categories = [];
          console.warn('API\'den kategori verisi alınamadı');
        }
      } catch (error) {
        console.error('Kategoriler yüklenirken hata:', error);
        this.categories = [];
      }
    },

    async fetchAdvisors() {
      try {
        const response = await advisorService.getAllAdvisors();
        if (response && response.data && response.data.advisors) {
          this.advisors = response.data.advisors;
        } else {
          this.advisors = [];
          console.warn('API\'den danışman verisi alınamadı');
        }
      } catch (error) {
        console.error('Danışmanlar yüklenirken hata:', error);
        this.advisors = [];
      }
    },

    async fetchClubMembers(clubId) {
      try {
        const response = await clubService.getClubMembers(clubId);
        if (response && response.data) {
          this.clubMembers = response.data;
          console.log('Topluluk üyeleri:', this.clubMembers);
        } else {
          this.clubMembers = [];
        }
      } catch (error) {
        console.error('Topluluk üyeleri yüklenirken hata:', error);
        this.clubMembers = [];
      }
    },
    getCategoryName(id) {
  const category = this.categories.find(cat => cat.id == id);
  return category ? category.name : 'Bilinmiyor';
},

    getStatusText(status) {
      const statusMap = {
        active: 'Aktif',
        inactive: 'Pasif',
        pending: 'Onay Bekleyen'
      };
      return statusMap[status] || status;
    },
    filterCommunities() {
      // Filtering is handled by computed property.
    },
    async approveCommunity(id) {
      try {
        // Backend API'ye onay gönder
        await clubService.activateClub(id);
        this.showNotification('success', 'Topluluk onaylandı.');

        // UI güncellemesi
        const community = this.communities.find(c => c.id === id);
        if (community) {
          community.status = 'active';
        }
      } catch (error) {
        console.error('Topluluk onaylanırken hata:', error);
        this.showNotification('error', 'Topluluk onaylanırken bir hata oluştu.');
      }
    },
    async rejectCommunity(id) {
      if (confirm('Bu topluluğu reddetmek istediğinizden emin misiniz?')) {
        try {
          // Backend API'ye ret gönder
          await clubService.deleteClub(id);
          this.showNotification('success', 'Topluluk reddedildi.');

          // UI güncellemesi - topluluk listeden kaldırılacak
          this.communities = this.communities.filter(c => c.id !== id);
        } catch (error) {
          console.error('Topluluk reddedilirken hata:', error);
          // Hata durumunda kullanıcıya bilgi ver
          this.showNotification('error', 'Topluluk reddedilirken bir hata oluştu.');
        }
      }
    },
    showNotification(type, message, duration = 3000) {
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
    async editCommunity(community) {
      this.editingCommunity = community;
      this.communityForm = { ...community };
      
      // Topluluk üyelerini yükle
      if (community.id) {
        await this.fetchClubMembers(community.id);
        
        // Mevcut başkanı bul ve seç
        const currentPresident = this.clubMembers.find(member => 
          member.role?.toLowerCase() === 'başkan'
        );
        if (currentPresident) {
          this.communityForm.presidentId = currentPresident.userId;
        }
      }
      
      this.showAddModal = true;
    },
    async deleteCommunity(id) {
      if (confirm('Bu topluluğu silmek istediğinizden emin misiniz?')) {
        try {
          // Backend API'de silme işlemi
          await clubService.deleteClub(id);

          // UI güncellemesi
          this.communities = this.communities.filter(c => c.id !== id);
        } catch (error) {
          console.error('Topluluk silinirken hata:', error);
          this.showNotification('error', 'Topluluk silinirken bir hata oluştu.');
        }
      }
    },
    handleImageUpload(event) {
      const file = event.target.files[0];
      if (file) {
        // Önizleme için base64'e dönüştür
        const reader = new FileReader();
        reader.onload = e => {
          this.communityForm.imagePreview = e.target.result; // Önizleme için base64
        };
        reader.readAsDataURL(file);

        // Gerçek dosyayı doğrudan sakla - formData ile göndermek için
        this.communityForm.image = file;
      }
    },
    handleDocumentUpload(event) {
      const file = event.target.files[0];
      if (file) {
        this.communityForm.document = file;
      }
    },
    async saveCommunity() {
      try {
        const formData = new FormData();
        formData.append('name', this.communityForm.name);

        // Kategori değerini ve tipini logla
        console.log('Kategori değeri:', this.communityForm.category);
        console.log('Kategori tipi:', typeof this.communityForm.category);

        formData.append('categoryId', this.communityForm.category);
        formData.append('description', this.communityForm.description);

        // Danışman değerini ve tipini logla
        console.log('Danışman ID değeri:', this.communityForm.advisorId);
        console.log('Danışman ID tipi:', typeof this.communityForm.advisorId);

        formData.append('advisorId', this.communityForm.advisorId);

        // FormData içeriğini logla
        for (let pair of formData.entries()) {
          console.log(`${pair[0]}: ${pair[1]}`);
        }

        if (this.communityForm.document && this.communityForm.document instanceof File) {
          formData.append('document', this.communityForm.document);
        }

        // Image dosyası yükleme durumunu kontrol et
        if (this.communityForm.image && this.communityForm.image instanceof File) {
          formData.append('image', this.communityForm.image);
        }

        if (this.editingCommunity) {
          // Mevcut topluluğu güncelle
          await clubService.updateClub(this.editingCommunity.id, formData);

          // Başkan değiştirildi mi kontrol et
          if (this.communityForm.presidentId) {
            // Mevcut başkanı bul
            const currentPresident = this.clubMembers.find(member => 
              member.role?.toLowerCase() === 'başkan'
            );
            
            // Yeni başkan seçildi ve mevcut başkanla farklıysa değiştir
            if (!currentPresident || currentPresident.userId !== this.communityForm.presidentId) {
              try {
                await clubService.changeClubPresident(
                  this.editingCommunity.id, 
                  this.communityForm.presidentId
                );
                this.showNotification('success', 'Topluluk başkanı başarıyla değiştirildi.');
              } catch (error) {
                console.error('Başkan değiştirme hatası:', error);
                this.showNotification('error', 'Başkan değiştirilirken bir hata oluştu: ' + 
                  (error.response?.data?.message || error.message));
              }
            }
          }

          // UI güncellemesi (yeniden tüm veriyi çek)
          await this.fetchCommunities();
        } else {
          // Yeni topluluk oluştur
          const response = await clubService.createClub(formData);
          console.log('API yanıtı:', response);

          // UI güncellemesi (yeniden tüm veriyi çek)
          await this.fetchCommunities();
        }

        this.closeModal();
      } catch (error) {
        console.error('Topluluk kaydedilirken hata detayı:', error);
        if (error.response) {
          console.error('Hata yanıtı:', error.response.data);
          console.error('Hata durumu:', error.response.status);
          console.error('Hata başlıkları:', error.response.headers);

          // Errors detayını göster
          if (error.response.data && error.response.data.errors) {
            console.error('Validation errors:', JSON.stringify(error.response.data.errors, null, 2));
          }

          this.showNotification('error', 'Topluluk kaydedilirken bir hata oluştu: ' + error.response.data.message);
        } else {
          this.showNotification('error', 'Topluluk kaydedilirken bir hata oluştu: ' + error.message);
        }
      }
    },
    closeModal() {
      this.showAddModal = false;
      this.editingCommunity = null;
      this.clubMembers = [];
      this.communityForm = {
        name: '',
        category: '',
        description: '',
        advisorId: null,
        presidentId: null,
        document: null,
        image: null,
        imagePreview: null
      };
    },
    goToManagement(id) {
      this.$router.push(`/admin/community-management/${id}`);
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
    async saveClubCreateDates() {
      try {
        const token = localStorage.getItem('token');
        await fetch('/api/settings/club-create-dates', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({
            startDate: this.clubCreateStartDate,
            endDate: this.clubCreateEndDate
          })
        });
        this.notification.show = true;
        this.notification.type = 'success';
        this.notification.message = 'Tarih aralığı başarıyla kaydedildi.';
        this.showDateModal = false;
      } catch (e) {
        this.notification.show = true;
        this.notification.type = 'error';
        this.notification.message = 'Tarih aralığı kaydedilemedi!';
      }
    },
  }
}
</script>

<style scoped>
.admin-communities {
  padding: 1rem;
}
.button-group {
  display: flex;
  gap: 1rem;
  align-items: center;
}

/* Page Header */
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

/* Filters */
.filters {
  display: flex;
  flex-wrap: wrap;
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
  flex-wrap: wrap;
  gap: 1rem;
}

.filter-group select {
  padding: 0.75rem 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
}

/* Communities Grid */
.communities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

/* Community Card */
.community-card {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  height: 100%;
  padding-bottom: 12px;
}

.community-image {
  position: relative;
  height: 200px;
}

.community-image img {
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

.status-badge.pending {
  background: #f39c12;
}

.community-content.flex-col-stretch {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  flex: 1 1 auto;
  padding: 1.5rem;
}

.community-content h3 {
  margin: 0 0 0.5rem;
  color: #2c3e50;
}

.category {
  color: #7f8c8d;
  margin: 0 0 1rem;
  font-size: 0.9rem;
}

.community-stats {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.community-stats span {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.description {
  color: #34495e;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.community-actions.fixed-bottom {
  display: flex;
  gap: 0.5rem;
  justify-content: space-between;
  margin-top: auto;
  flex-wrap: wrap;
}

.edit-btn,
.delete-btn,
.approve-btn,
.reject-btn,
.manage-btn {
  flex: 1 1 0;
  min-width: 0;
  padding: 6px 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: background-color 0.3s;
  height: 38px;
  font-size: 0.95rem;
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

.approve-btn {
  background: #2ecc71;
  color: white;
}

.approve-btn:hover {
  background: #27ae60;
}

.reject-btn {
  background: #e74c3c;
  color: white;
}

.reject-btn:hover {
  background: #c0392b;
}

.manage-btn {
  background-color: #4a6cf7;
  color: white;
}

.manage-btn:hover {
  background-color: #3a5ce5;
}

.manage-btn i {
  margin-right: 5px;
}

/* Loading, Error, Empty States */
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
  max-width: 600px;
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

/* RESPONSIVE STYLES */

/* Tablets (max-width: 992px) */
@media (max-width: 992px) {
  .communities-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }

  .page-header h1 {
    font-size: 1.75rem;
  }

  .search-box,
  .filter-group select {
    max-width: 100%;
  }
}

/* Mobile (max-width: 576px) */
@media (max-width: 576px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .add-btn {
    margin-top: 1rem;
  }

  .filters {
    flex-direction: column;
    gap: 0.75rem;
  }

  .search-box {
    max-width: 100%;
  }

  .filter-group {
    flex-direction: column;
    gap: 0.75rem;
    width: 100%;
  }

  .communities-grid {
    grid-template-columns: 1fr;
  }

  .community-card {
    padding: 1rem;
  }

  .search-box input {
    padding: 0.75rem 1rem 0.75rem 2.5rem;
  }

  .community-actions.fixed-bottom {
    flex-direction: column;
    gap: 0.5rem;
    align-items: stretch;
  }
}

.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  max-width: 350px;
  background-color: white;
  border-radius: 6px;
  overflow: hidden;
  box-shadow: 0 3px 15px rgba(0, 0, 0, 0.2);
  z-index: 9999;
  animation: slideInDown 0.3s ease-out;
  transition: all 0.3s ease;
}

@keyframes slideInDown {
  from {
    transform: translateY(-50px);
    opacity: 0;
  }

  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.notification-success {
  border-left: 4px solid #2ecc71;
}

.notification-error {
  border-left: 4px solid #e74c3c;
}

.notification-warning {
  border-left: 4px solid #f39c12;
}

.notification-info {
  border-left: 4px solid #3498db;
}

.notification-content {
  display: flex;
  align-items: center;
  padding: 12px 15px;
  gap: 10px;
}

.notification-icon {
  font-size: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-success .notification-icon {
  color: #2ecc71;
}

.notification-error .notification-icon {
  color: #e74c3c;
}

.notification-warning .notification-icon {
  color: #f39c12;
}

.notification-info .notification-icon {
  color: #3498db;
}

.notification-message {
  color: #333;
  font-size: 0.95rem;
  font-weight: 500;
  flex-grow: 1;
}

.notification-close {
  background: transparent;
  border: none;
  color: #999;
  cursor: pointer;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  transition: all 0.2s;
}

.notification-close:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: #333;
}

.notification-progress {
  height: 3px;
  background: linear-gradient(to right, rgba(0, 0, 0, 0.1), rgba(0, 0, 0, 0.3));
  width: 100%;
  animation: progress-bar 3000ms linear forwards;
  transform-origin: left;
}

@keyframes progress-bar {
  from {
    transform: scaleX(1);
  }

  to {
    transform: scaleX(0);
  }
}
</style>
