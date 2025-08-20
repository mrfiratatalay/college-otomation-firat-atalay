<template>
  <div class="advisor-communities">
    <div class="page-header">
      <h1>Sorumlu Topluluklar</h1>
      <button @click="showAddModal = true" class="add-btn">
        <i class="fas fa-plus"></i>
        Yeni Topluluk
      </button>
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

    <div class="communities-grid">
      <div v-for="community in filteredCommunities" :key="community.id" class="community-card">
        <div class="community-image">
          <img :src="community.image" :alt="community.name">
          <div class="status-badge" :class="community.status">
            {{ getStatusText(community.status) }}
          </div>
        </div>
        <div class="community-content">
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
          <div class="community-actions">
            <template v-if="community.status === 'pending'">
              <button @click="editCommunity(community)" class="edit-btn">
                <i class="fas fa-edit"></i>
                Düzenle
              </button>
              <button @click="deleteCommunity(community.id)" class="btn-reject">
                <i class="fas fa-trash"></i>
                Sil
              </button>
            </template>
            <template v-else-if="isClubLeader(community.id)">
              <button class="edit-btn" @click="goToManagement(community.id)">
                <i class="fa fa-info-circle" aria-hidden="true"></i>
                Yönet
              </button>
            </template>
            <template v-else-if="isPendingMember(community.id)">
              <button class="btn-approve" disabled>
                <i class="fas fa-clock"></i> Bekliyor
              </button>
            </template>
            <template v-else-if="!isClubMember(community.id) && community.status === 'active'">
              <button class="btn-approve" @click="joinClub(community.id)" :disabled="joinLoading[community.id]">
                <i class="fas fa-plus"></i>
                {{ joinLoading[community.id] ? 'İşleniyor...' : 'Katıl' }}
              </button>
            </template>
            <template v-else-if="!isClubMember(community.id) && community.status !== 'active'">
              <button class="btn-reject" disabled title="Bu topluluk şu anda aktif değil">
                <i class="fas fa-ban"></i> Katılıma Kapalı
              </button>
            </template>
            <template v-else>
              <button class="btn-reject" @click="leaveClub(community.id)" :disabled="leaveLoading[community.id]">
                <i class="fas fa-sign-out-alt"></i>
                {{ leaveLoading[community.id] ? 'İşleniyor...' : 'Ayrıl' }}
              </button>
            </template>
          </div>
        </div>
      </div>
    </div>

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
            <div class="form-group">
              <label>Topluluk Belgeleri</label>
              <input type="file" @change="handleDocumentUpload" multiple>
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
import { advisorService, clubService, categoryService } from '@/services';
import { reactive } from 'vue';
import { authService } from '@/services';

export default {
  name: 'LeaderCommunities',
  data() {
    return {
      searchQuery: '',
      statusFilter: 'all',
      categoryFilter: 'all',
      showAddModal: false,
      categories: [],
      advisors: [],
      communityForm: {
        name: '',
        category: '',
        description: '',
        image: null,
        imagePreview: null,
        advisorId: null,
        documents: []
      },
      communities: [],
      userMemberships: [],
      pendingMemberships: [],
      userRoles: {},
      joinLoading: reactive({}),
      leaveLoading: reactive({}),
      loading: false,
      error: null,
      editingCommunity: null
    }
  },
  computed: {
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
  methods: {
    getStatusText(status) {
      const statusMap = {
        active: 'Aktif',
        inactive: 'Pasif',
        pending: 'Onay Bekliyor'
      };
      return statusMap[status] || status;
    },
    filterCommunities() {
      // Filtering is handled by computed property.
    },
    editCommunity(community) {
      this.editingCommunity = community;
      this.communityForm = { ...community };
      this.showAddModal = true;
    },
    deleteCommunity(id) {
      if (confirm('Bu topluluğu silmek istediğinizden emin misiniz?')) {
        this.communities = this.communities.filter(c => c.id !== id);
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
      const files = Array.from(event.target.files);
      this.communityForm.documents = files;
    },
    async saveCommunity() {
      try {
        const formData = new FormData();
        formData.append('name', this.communityForm.name);
        formData.append('categoryId', this.communityForm.category);
        formData.append('description', this.communityForm.description);
        formData.append('advisorId', this.communityForm.advisorId);

        if (this.communityForm.documents && this.communityForm.documents.length > 0) {
          this.communityForm.documents.forEach(doc => {
            formData.append('documents', doc);
          });
        }

        if (this.communityForm.image && this.communityForm.image instanceof File) {
          formData.append('image', this.communityForm.image);
        }

        if (this.editingCommunity) {
          // Mevcut topluluğu güncelle
          await clubService.updateClub(this.editingCommunity.id, formData);
          const index = this.communities.findIndex(c => c.id === this.editingCommunity.id);
          if (index !== -1) {
            this.communities[index] = { ...this.editingCommunity, ...this.communityForm };
          }
        } else {
          // Yeni topluluk oluştur
          await clubService.createClub(formData);
          const newCommunity = {
            id: this.communities.length + 1,
            ...this.communityForm,
            status: 'pending',
            memberCount: 0,
            eventCount: 0
          };
          this.communities.push(newCommunity);
        }
        this.closeModal();
      } catch (error) {
        console.error('Topluluk kaydedilirken hata:', error);
        alert('Topluluk kaydedilirken bir hata oluştu.');
      }
    },
    closeModal() {
      this.showAddModal = false;
      this.editingCommunity = null;
      this.communityForm = {
        name: '',
        category: '',
        description: '',
        image: null,
        imagePreview: null,
        advisorId: null,
        documents: []
      };
    },
    goToManagement(communityId) {
      this.$router.push(`/leader/community-management/${communityId}`)
    },
    filterCommunities() {
      // Filter is handled by computed property
    },
    isPendingMember(clubId) {
      return this.pendingMemberships.some(m =>
        (m.clubId == clubId || m.club?.id == clubId || m.club?.clubId == clubId) &&
        (m.status.toLowerCase() === "pending")
      );
    },

    isClubMember(clubId) {
      return this.userMemberships.some(m =>
        (m.clubId == clubId || m.club?.id == clubId || m.club?.clubId == clubId) &&
        (m.status.toLowerCase() === "approved")
      );
    },

    isClubLeader(clubId) {
      const membership = this.userMemberships.find(m =>
        m.clubId == clubId || m.club?.id == clubId || m.club?.clubId == clubId
      );

      if (!membership) return false;
      
      // Başkan rolünün farklı yazılışlarını kontrol et
      const role = (membership.role || '').toLowerCase();
      return role === 'başkan' || 
             role === 'leader' || 
             role === 'president' || 
             role === 'yönetici' || 
             role === 'admin' || 
             membership.isLeader === true;
    },
    async joinClub(clubId) {
      if (!clubId) {
        console.error('joinClub: clubId parametresi undefined veya null');
        alert('Geçersiz topluluk kimliği');
        return;
      }

      this.joinLoading[clubId] = true;

      try {
        const user = authService.getUser();

        if (!user) {
          console.error('Kullanıcı bilgisi bulunamadı');
          alert('Kullanıcı bilgilerinize erişilemiyor. Lütfen tekrar giriş yapın.');
          return;
        }

        const requestData = {
          clubId: clubId,
          userId: user.id
        };

        await clubService.applyToClub(requestData);

        // Kullanıcının üyeliklerini güncelle
        await this.fetchUserMemberships();

        alert(`Topluluğa başvurunuz başarıyla iletildi. Topluluk lideri onayı bekleniyor.`);
      } catch (error) {
        console.error('Topluluğa katılma hatası:', error);

        if (error.response && error.response.data) {
          alert(`Hata: ${error.response.data}`);
        } else {
          alert('Topluluğa katılırken bir hata oluştu!');
        }
      } finally {
        this.joinLoading[clubId] = false;
      }
    },
    async leaveClub(clubId) {
      if (!clubId) {
        console.error('leaveClub: clubId parametresi undefined veya null');
        alert('Geçersiz topluluk kimliği');
        return;
      }

      if (!confirm('Bu topluluktan ayrılmak istediğinize emin misiniz?')) {
        return;
      }

      this.leaveLoading[clubId] = true;

      try {
        // İlgili topluluk üyeliğini bul
        const membership = this.userMemberships.find(m =>
          m.clubId == clubId || m.club?.id == clubId || m.club?.clubId == clubId
        );

        if (!membership) {
          console.error(`Üyelik bulunamadı: clubId=${clubId}`);
          alert('Üyelik bilgisi bulunamadı.');
          return;
        }

        // API'nin beklediği membershipId değerini kullan
        const membershipId = membership.membershipId || membership.id;

        if (!membershipId) {
          console.error('Üyelik ID bulunamadı:', membership);
          alert('Üyelik kimliği bulunamadı.');
          return;
        }

        await clubService.leaveClub(membershipId);

        // Üyelik bilgilerini güncelle
        await this.fetchUserMemberships();

        alert('Topluluktan başarıyla ayrıldınız.');
      } catch (error) {
        console.error('Topluluktan ayrılma hatası:', error);

        if (error.response && error.response.data) {
          alert(`Hata: ${error.response.data}`);
        } else {
          alert('Topluluktan ayrılırken bir hata oluştu!');
        }
      } finally {
        this.leaveLoading[clubId] = false;
      }
    },
    async fetchUserMemberships() {
      try {
        // Onaylı ve bekleyen üyelikleri tek bir API çağrısında al
        const response = await clubService.getMyMemberships();
        console.log('Alınan üyelik bilgileri:', response.data);

        if (response.data && response.data.memberships) {
          this.userMemberships = response.data.memberships;
          console.log('Kullanıcı üyelikleri:', this.userMemberships);

          // Her üyeliğin rolünü kontrol et ve logla
          this.userMemberships.forEach(membership => {
            console.log(`Üyelik ID: ${membership.membershipId || membership.id}, ClubId: ${membership.clubId}, Role: ${membership.role}, Status: ${membership.status}`);
          });
        } else {
          this.userMemberships = [];
        }

        // Bekleyen üyelikleri API yanıtından al
        if (response.data && response.data.pendingMemberships) {
          this.pendingMemberships = response.data.pendingMemberships;
          console.log('Bekleyen üyelikler:', this.pendingMemberships);
        } else {
          this.pendingMemberships = [];
        }

        // Kullanıcının rolleri (başkan, üye, vb.)
        this.userRoles = this.userMemberships.reduce((acc, membership) => {
          acc[membership.clubId] = membership.role || 'üye';
          return acc;
        }, {});

        console.log('User roles:', this.userRoles);

      } catch (error) {
        console.error('Kullanıcı üyelikleri getirilirken hata:', error);
        alert('Üyelik bilgileriniz yüklenirken bir sorun oluştu.');
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
            image: club.imageUrl || (club.image ? `data:image/jpeg;base64,${club.image}` : '/assets/default-image.png'),
            status: club.status || 'inactive',
            memberCount: club.memberCount || 0,
            eventCount: club.eventCount || 0
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
  },
  mounted() {
    this.fetchAdvisors();
    this.fetchCategories();
    this.fetchCommunities();
    this.fetchUserMemberships();
  }
}
</script>

<style scoped>
.advisor-communities {
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
  background-color: #27ae60;
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

.communities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.community-card {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

}

.community-image {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.community-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  position: absolute;
  top: 0;
  left: 0;
}

.status-badge {
  position: absolute;
  top: 10px;
  right: 10px;
  padding: 6px 12px;
  border-radius: 15px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  z-index: 2;
}

.status-badge.active {
  background-color: #27ae60;
  color: white;
}

.status-badge.inactive {
  background-color: #e74c3c;
  color: white;
}

.status-badge.pending {
  background-color: #f39c12;
  color: white;
}

.community-content {
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

.community-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: space-between;
}

.edit-btn,
.btn-approve,
.btn-reject {
  flex: 1;
  min-width: 100px;
  padding: 8px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: background-color 0.3s;
  height: 38px;
  font-size: 14px;
}

.edit-btn {
  background-color: #3498db;
  color: white;
}

.edit-btn:hover {
  background-color: #2980b9;
}

.btn-approve {
  background-color: #27ae60;
  color: white;
}

.btn-approve:hover {
  background-color: #219a52;
}

.btn-reject {
  background-color: #e74c3c;
  color: white;
}

.btn-reject:hover {
  background-color: #c0392b;
}

.btn-approve:disabled, .btn-reject:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
  opacity: 0.7;
}

.btn-manage {
  display: inline-block;
  padding: 0.5rem 1rem;
  background-color: #3498db;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.btn-manage:hover {
  background-color: #2980b9;
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
}
</style>