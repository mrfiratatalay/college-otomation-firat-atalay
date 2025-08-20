<template>
  <div class="advisor-communities">
    <div class="page-header">
      <h1>Sorumlu Topluluklar</h1>
    </div>
    <div v-if="loading" class="loading-container">
      <p>Yükleniyor...</p>
    </div>
    <div v-else-if="error" class="error-container">
      <p>{{ error }}</p>
    </div>
    <div v-else>
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
        <div v-for="community in filteredCommunities" :key="community.clubId" class="community-card">
          <div class="community-image">
            <img :src="community.imageUrl || 'https://via.placeholder.com/300x200?text=Topluluk'" :alt="community.name">
            <div class="status-badge" :class="community.status.toLowerCase()">
              {{ getStatusText(community.status) }}
            </div>
          </div>
          <div class="community-content">
            <h3>{{ community.name }}</h3>
            <p class="category">{{ community.categoryName || 'Kategori Belirtilmemiş' }}</p>
            <div class="community-stats">
              <span>
                <i class="fas fa-users"></i>
                {{ community.memberCount || 0 }} Üye
              </span>
              <span>
                <i class="fas fa-calendar-check"></i>
                {{ community.eventCount || 0 }} Etkinlik
              </span>
            </div>
            <p class="description">{{ community.description || 'Açıklama bulunmuyor.' }}</p>
            <div class="community-actions">
              <template v-if="community.status.toLowerCase() === 'pending'">
                <button @click="approveCommunity(community)" class="btn-approve">
                  <i class="fas fa-check"></i>
                  Onayla
                </button>
                <button @click="rejectCommunity(community.clubId)" class="btn-reject">
                  <i class="fas fa-trash"></i>
                  Reddet
                </button>
              </template>
              <template v-if="community.status.toLowerCase() === 'active'">
                <button class="edit-btn" @click="goToManagement(community.clubId)">
                  <i class="fas fa-edit"></i>
                  Yönet
                </button>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import advisorService from '@/services/advisorService';
import clubService from '@/services/clubService';
import categoryService from '@/services/categoryService';
import authService from '@/services/authService';

export default {
  name: 'AdvisorCommunities',
  data() {
    return {
      searchQuery: '',
      statusFilter: 'all',
      categoryFilter: 'all',
      categories: [],
      communities: [],
      rawCommunities: [],
      advisorId: null,
      loading: false,
      error: null
    }
  },
  async created() {
    try {
      this.loading = true;
      // Kullanıcı bilgilerini al
      const user = authService.getUser();
      if (!user) {
        this.$router.push('/login');
        return;
      }
      
      // Kullanıcı advisor değilse ana sayfaya yönlendir
      if (user.userType !== 'advisor') {
        this.$router.push('/');
        return;
      }
      
      this.advisorId = user.id;
      
      // Kategorileri yükle
      try {
        const categoriesResponse = await categoryService.getAllCategories();
        this.categories = categoriesResponse.data.categories || [];
        console.log('Kategoriler yüklendi:', this.categories);
      } catch (error) {
        console.error('Kategoriler yüklenirken hata:', error);
      }
      
      // Danışmanın sorumlu olduğu kulüpleri almak için iki yöntem kullanabiliriz:
      // 1. advisorService.getAdvisorClubs - danışmana özel API endpointi
      // 2. getAllClubs ve client tarafında filtreleme
      
      // Özel API endpointi ile danışmanın kulüplerini doğrudan getirelim
      try {
        const response = await clubService.getAdvisorMyClubs();
        console.log('Danışman kulüpleri yüklendi:', response.data);
        
        if (response.data && Array.isArray(response.data)) {
          this.communities = response.data.map(club => ({
            ...club,
            clubId: club.clubId,
            name: club.name,
            description: club.description,
            status: club.status || 'active',
            memberCount: club.memberCount || 0,
            eventCount: club.eventCount || 0,
            categoryId: club.categoryId,
            categoryName: this.getCategoryName(club.categoryId),
            imageUrl: club.image ? `data:image/jpeg;base64,${club.image}` : null
          }));
            
          console.log('Danışman kulüpleri:', this.communities);
        } else {
          console.warn('Kulüp verisi beklenen formatta değil:', response.data);
          this.communities = [];
        }
      } catch (error) {
        console.error('Kulüpler yüklenirken hata:', error);
        this.error = 'Topluluklar yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
      }
    } catch (error) {
      console.error('Veriler yüklenirken hata:', error);
      this.error = 'Veriler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
    } finally {
      this.loading = false;
    }
  },
  computed: {
    filteredCommunities() {
      if (!this.communities || !this.communities.length) return [];
      
      return this.communities.filter(community => {
        const matchesSearch = community.name.toLowerCase().includes(this.searchQuery.toLowerCase());
        const matchesStatus = this.statusFilter === 'all' || 
                            (community.status && community.status.toLowerCase() === this.statusFilter);
        const matchesCategory = this.categoryFilter === 'all' || 
                              (community.categoryId && community.categoryId.toString() === this.categoryFilter.toString());
        return matchesSearch && matchesStatus && matchesCategory;
      });
    }
  },
  methods: {
    getCategoryName(categoryId) {
      if (!categoryId || !this.categories.length) return 'Kategori Belirtilmemiş';
      const category = this.categories.find(cat => cat.id === parseInt(categoryId));
      return category ? category.name : 'Kategori Belirtilmemiş';
    },
    async approveCommunity(community) {
      try {
        this.loading = true;
        console.log('Topluluk onaylanıyor:', community.clubId);
        
        // updateClubStatus yerine activateClub metodunu kullan
        await clubService.activateClub(community.clubId);
        
        // Kulübün durumunu yerel olarak güncelle
        const index = this.communities.findIndex(c => c.clubId === community.clubId);
        if (index !== -1) {
          this.communities[index].status = 'active';
        }
        
        this.$notify({
          type: 'success',
          message: 'Topluluk başarıyla onaylandı'
        });
      } catch (error) {
        console.error('Topluluk onaylanırken hata:', error);
        this.$notify({
          type: 'error',
          message: 'Topluluk onaylanırken bir hata oluştu'
        });
      } finally {
        this.loading = false;
      }
    },
    async rejectCommunity(clubId) {
      try {
        this.loading = true;
        console.log('Topluluk reddediliyor:', clubId);
        
        // updateClubStatus yerine deleteClub metodunu kullan
        await clubService.deleteClub(clubId);
        
        // Kulübü listeden kaldır
        this.communities = this.communities.filter(c => c.clubId !== clubId);
        
        this.$notify({
          type: 'success',
          message: 'Topluluk reddedildi'
        });
      } catch (error) {
        console.error('Topluluk reddedilirken hata:', error);
        this.$notify({
          type: 'error',
          message: 'Topluluk reddedilirken bir hata oluştu'
        });
      } finally {
        this.loading = false;
      }
    },
    goToManagement(clubId) {
      console.log('Yönetim sayfasına yönlendiriliyor:', clubId);
      this.$router.push(`/advisor/community-management/${clubId}`);
    },
    filterCommunities() {
      // Filter is handled by computed property
      console.log('Filtreleme yapılıyor:', {
        search: this.searchQuery,
        status: this.statusFilter,
        category: this.categoryFilter
      });
    },
    getStatusText(status) {
      if (!status) return 'Belirsiz';
      
      const statusMap = {
        'active': 'Aktif',
        'inactive': 'Pasif',
        'pending': 'Onay Bekliyor',
        'ACTIVE': 'Aktif',
        'INACTIVE': 'Pasif',
        'PENDING': 'Onay Bekliyor'
      };
      return statusMap[status.toLowerCase()] || status;
    }
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
  justify-content: space-between;
  gap: 0.5rem;
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

.loading-container, 
.error-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  margin: 1rem 0;
  padding: 2rem;
}

.error-container {
  color: #e74c3c;
}
</style>