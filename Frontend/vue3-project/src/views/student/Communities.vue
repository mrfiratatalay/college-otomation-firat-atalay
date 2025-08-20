<template>
  <div class="communities-page">
    <div class="page-header">
      <h1>Topluluklar</h1>
      <div class="search-box">
        <input type="text" v-model="searchQuery" placeholder="Topluluk ara...">
        <i class="fas fa-search"></i>
      </div>
    </div>

    <div class="filters">
      <select v-model="categoryFilter">
        <option value="">Tüm Kategoriler</option>
        <option v-for="category in categories" :key="category" :value="category">
          {{ category }}
        </option>
      </select>
      <select v-model="statusFilter">
        <option value="">Tüm Durumlar</option>
        <option value="active">Aktif</option>
        <option value="inactive">Pasif</option>
      </select>
    </div>

    <div v-if="loading" class="loading-container">
      <p>Yükleniyor...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p>{{ error }}</p>
      <button @click="fetchClubs" class="btn btn-primary">Tekrar Dene</button>
    </div>

    <div v-else class="communities-grid">
      <div v-for="club in filteredClubs" :key="club.clubId || club.id" class="community-card">
        <div class="community-image">
          <div class="image-placeholder mb-3">
            <img v-if="club.image" :src="'data:image/jpeg;base64,' + club.image" alt="Topluluk Resmi" class="img-fluid">
            <span v-else>Resim Yok</span>
          </div>
          <div class="status-badge" :class="club.status.toLowerCase()">
            {{ club.status }}
          </div>
        </div>
        <div class="community-info card-flex-content">
          <h3>{{ club.name }}</h3>
          <p class="category">{{ club.categoryName }}</p>
          <div class="stats">
            <span><i class="fas fa-users"></i> {{ club.memberCount || 0 }} Üye</span>
            <span><i class="fas fa-calendar"></i> {{ club.eventCount || 0 }} Etkinlik</span>
          </div>
          <div class="card-spacer"></div>
          <div class="actions" v-if="!membershipsLoading">
            <!-- Debug Log: Show calculated status and club name -->
            <!-- {{ console.log(`[Template Debug] Club: ${club.name} (ID: ${club.clubId || club.id}), Calculated Status: ${getMembershipStatus(club.clubId || club.id)}`) }} -->

            <button v-if="isClubLeader(club.clubId || club.id)" 
              class="edit-btn"
              @click="manageClub(club.clubId || club.id)">
              <i class="fas fa-cog"></i> Yönet
            </button>
            <div v-else-if="isClubMember(club.clubId || club.id)" class="member-actions">
              <span class="member-status">
                <i class="fas fa-check-circle"></i> Aktif Üye
              </span>
              <button 
                class="leave-btn" 
                @click="leaveClub(club.clubId || club.id)"
                :disabled="leaveLoading[club.clubId || club.id]">
                <i class="fas fa-sign-out-alt"></i> 
                {{ leaveLoading[club.clubId || club.id] ? 'Ayrılıyor...' : 'Ayrıl' }}
              </button>
            </div>
            <button v-else-if="hasAppliedToClub(club.clubId || club.id)"
              class="status-btn status-pending" 
              disabled>
              <i class="fas fa-clock"></i> Onay Bekleniyor
            </button>
            <button v-else-if="club.status === 'active'"
              class="btn btn-primary" 
              @click="joinClub(club.clubId || club.id)"
              :disabled="joinLoading[club.clubId || club.id]">
              <i class="fas fa-plus"></i> {{ joinLoading[club.clubId || club.id] ? 'İşleniyor...' : 'Katıl' }}
            </button>
          </div>
          <div class="actions-loading" v-else>
             <small>Butonlar yükleniyor...</small>
          </div>
        </div>
      </div>

      <div v-if="filteredClubs.length === 0" class="no-results">
        <p>Arama kriterlerinize uygun topluluk bulunamadı.</p>
      </div>
    </div>

    <!-- Bildirim bileşeni - sağ üst köşeye taşınıyor -->
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

    <!-- Global error dialog -->
    <div v-if="errorDialog.show" class="error-dialog" :class="errorDialog.type">
      <div class="error-dialog-content">
        <i v-if="errorDialog.type === 'error'" class="fas fa-exclamation-circle"></i>
        <i v-else-if="errorDialog.type === 'warning'" class="fas fa-exclamation-triangle"></i>
        <i v-else-if="errorDialog.type === 'success'" class="fas fa-check-circle"></i>
        <span>{{ errorDialog.message }}</span>
        <button @click="errorDialog.show = false" class="close-btn">
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { studentService } from '@/services';
import { reactive } from 'vue';

export default {
  name: 'StudentCommunities',
  data() {
    return {
      searchQuery: '',
      categoryFilter: '',
      statusFilter: '',
      categories: ['Teknoloji', 'Spor', 'Sanat', 'Eğitim', 'Kültür', 'Sosyal'],
      clubs: [],
      loading: true,
      error: null,
      userApprovedMemberships: [],
      userPendingMemberships: [],
      myApplications: [],
      membershipsLoading: true,
      joinLoading: reactive({}),
      leaveLoading: reactive({}),
      notification: {
        show: false,
        type: 'info',
        message: '',
        timeout: null,
        duration: 3000
      },
      errorDialog: {
        show: false,
        type: 'error',
        message: ''
      },
    }
  },
  computed: {
    filteredClubs() {
      return this.clubs.filter(club => {
        const matchesSearch = club.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
          (club.description && club.description.toLowerCase().includes(this.searchQuery.toLowerCase()))
        const matchesCategory = !this.categoryFilter || club.category === this.categoryFilter
        const matchesStatus = !this.statusFilter || club.status === this.statusFilter
        return matchesSearch && matchesCategory && matchesStatus
      })
    }
  },
  async created() {
    await this.fetchUserMemberships();
    await this.fetchMyApplications();
    await this.fetchClubs();
  },
  methods: {
    async fetchClubs() {
      this.loading = true;
      this.error = null;

      try {
        const response = await studentService.getClubs();
        const clubs = response.data || [];
        
        // Her bir kulüp için detaylı bilgileri al
        const clubsWithDetails = await Promise.all(clubs.map(async club => {
          try {
            // Kulüp detaylarını al
            const members = await studentService.getMembersForMyLedClubs(club.clubId || club.id);
            
            return {
              ...club,
              memberCount: Array.isArray(members) ? members.length : 0,
              status: club.status ? club.status.toLowerCase() : 'inactive'
            };
          } catch (error) {
            console.error(`Kulüp detayları alınamadı (ID: ${club.clubId || club.id}):`, error);
            return {
              ...club,
              memberCount: 0,
              status: club.status ? club.status.toLowerCase() : 'inactive'
            };
          }
        }));

        console.log('Fetched clubs data with details:', JSON.stringify(clubsWithDetails, null, 2));
        this.clubs = clubsWithDetails;
      } catch (error) {
        console.error('Kulüpleri getirme hatası:', error);
        this.error = error.response?.data?.message || 'Kulüpler yüklenirken bir hata oluştu.';
        this.showNotification('error', this.error);
      } finally {
        this.loading = false;
      }
    },

    async fetchUserMemberships() {
      this.membershipsLoading = true;
      try {
        const response = await studentService.getMyMemberships();
        console.log('API Response:', response);
        console.log('API Response Type:', typeof response);
        console.log('API Response Data:', response.data);

        // API yanıtının yapısını kontrol et
        if (response && response.data) {
          console.log('Raw API Data:', JSON.stringify(response.data, null, 2));
          
          // API'den gelen veriyi doğrudan kullan
          this.userApprovedMemberships = response.data;
          this.userPendingMemberships = []; // Eğer pending başvurular ayrı bir endpoint'ten geliyorsa burası değişebilir
          
          console.log('Processed Approved Memberships:', JSON.stringify(this.userApprovedMemberships, null, 2));
          console.log('Membership Count:', this.userApprovedMemberships.length);
          
          // Her bir üyeliği kontrol et
          this.userApprovedMemberships.forEach((membership, index) => {
            console.log(`Membership ${index + 1}:`, {
              clubId: membership.clubId || membership.ClubId,
              role: membership.role || membership.Role,
              raw: membership
            });
          });
        } else {
          console.error('Invalid API response structure:', response);
          this.userApprovedMemberships = [];
          this.userPendingMemberships = [];
        }
      } catch (error) {
        console.error('Kullanıcı üyelikleri getirilirken hata:', error);
        this.userApprovedMemberships = [];
        this.userPendingMemberships = [];
        this.showNotification('error', 'Üyelik bilgileriniz yüklenirken bir sorun oluştu.');
      } finally {
        this.membershipsLoading = false;
      }
    },

    async fetchMyApplications() {
      try {
        const response = await studentService.getMyClubApplications();
        console.log('Başvurular:', response);
        this.myApplications = response || [];
      } catch (error) {
        console.error('Başvurular getirilirken hata:', error);
        this.myApplications = [];
      }
    },

    getMembershipStatus(clubId) {
      if (this.membershipsLoading) {
          console.warn(`[getMembershipStatus] Called for Club ID: ${clubId} while memberships are still loading. Returning 'none'.`);
          return 'none';
      }

      const idToCheck = parseInt(clubId, 10);
      console.log(`[getMembershipStatus] Checking Club ID: ${idToCheck} (Type: ${typeof idToCheck})`);
      console.log('Current approved memberships:', this.userApprovedMemberships);

      // Önce onaylı üyelikleri kontrol et
      const approved = this.userApprovedMemberships.find(m => {
          const currentClubId = parseInt(m.ClubId || m.clubId, 10);
          const isMatch = currentClubId === idToCheck;
          const role = (m.Role || m.role || '').toLowerCase().trim();
          console.log(`[getMembershipStatus] Checking membership:`, {
              currentClubId,
              idToCheck,
              isMatch,
              role,
              rawMembership: m
          });
          return isMatch;
      });

      if (approved) {
          console.log(`[getMembershipStatus] Found APPROVED membership:`, approved);
          const role = ((approved.Role || approved.role) || '').toLowerCase().trim();
          console.log(`[getMembershipStatus] Role found: '${role}' (Type: ${typeof role})`);
          
          if (role === 'başkan' || role === 'baskan' || role === 'leader') {
              console.log(`[getMembershipStatus] Role IS leader type. Returning 'leader'.`);
              return 'leader';
          } else {
              console.log(`[getMembershipStatus] Role is NOT leader type. Returning 'member'.`);
              return 'member';
          }
      }

      // Bekleyen başvuruları kontrol et
      const pending = this.userPendingMemberships.find(m => {
          const currentClubId = parseInt(m.ClubId || m.clubId, 10);
          const isMatch = currentClubId === idToCheck;
          console.log(`[getMembershipStatus] Checking pending membership:`, {
              currentClubId,
              idToCheck,
              isMatch,
              rawMembership: m
          });
          return isMatch;
      });

      if (pending) {
          console.log(`[getMembershipStatus] Found PENDING membership for Club ID: ${idToCheck}`);
          return 'pending';
      }

      console.log(`[getMembershipStatus] NO membership found for Club ID: ${idToCheck}. Returning 'none'.`);
      return 'none';
    },

    async joinClub(clubId) {
      if (this.joinLoading[clubId]) return;
      this.joinLoading[clubId] = true;
      try {
        await studentService.joinClub(clubId);
        this.showNotification('success', 'Başvurunuz başarıyla gönderildi.');
        
        // Başvuru yapıldıktan sonra hem üyelikleri hem de başvuruları yeniden yükle
        await Promise.all([
          this.fetchUserMemberships(),
          this.fetchMyApplications()
        ]);

        // Başvuruyu direkt olarak myApplications'a ekle
        const newApplication = {
          clubId: clubId,
          status: 'pending'
        };
        this.myApplications.push(newApplication);
      } catch (error) {
        console.error(`Kulübe (${clubId}) katılma hatası:`, error);
        this.showErrorDialog('error', error.response?.data?.message || 'Katılma işlemi sırasında bir hata oluştu.');
      } finally {
        this.joinLoading[clubId] = false;
      }
    },

    manageClub(clubId) {
      this.$router.push(`/student/community-management/${clubId}`);
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

    showErrorDialog(type, message) {
      this.errorDialog.type = type;
      this.errorDialog.message = message;
      this.errorDialog.show = true;
    },

    setDefaultImage(event) {
      event.target.src = 'https://placehold.co/300x200/3498db/ffffff?text=Resim+Yok';
    },

    isClubLeader(clubId) {
      if (this.membershipsLoading) return false;
      const membership = this.userApprovedMemberships.find(m => 
        parseInt(m.clubId || m.ClubId, 10) === parseInt(clubId, 10)
      );
      return membership && (membership.role || membership.Role || '').toLowerCase().trim() === 'başkan';
    },

    isClubMember(clubId) {
      if (this.membershipsLoading) return false;
      return this.userApprovedMemberships.some(m => 
        parseInt(m.clubId || m.ClubId, 10) === parseInt(clubId, 10)
      );
    },

    hasAppliedToClub(clubId) {
      return this.myApplications.some(app => 
        (app.clubId === clubId || app.ClubId === clubId) && 
        app.status === 'pending'
      );
    },

    async leaveClub(clubId) {
      try {
        this.leaveLoading[clubId] = true;
        
        // Kulübün membershipId'sini bul
        const membership = this.userApprovedMemberships.find(m => (m.clubId || m.ClubId) === clubId);
        
        if (!membership) {
          throw new Error('Üyelik bulunamadı');
        }

        await studentService.leaveClub(membership.membershipId || membership.MembershipId);
        
        // Üyelik listesini güncelle
        this.userApprovedMemberships = this.userApprovedMemberships.filter(m => 
          (m.clubId || m.ClubId) !== clubId
        );

        this.showNotification('success', 'Topluluktan başarıyla ayrıldınız.');
        
        // Kulüpleri yeniden yükle
        await this.fetchClubs();
      } catch (error) {
        console.error('Topluluktan ayrılma hatası:', error);
        this.showNotification('error', 'Topluluktan ayrılırken bir hata oluştu.');
      } finally {
        this.leaveLoading[clubId] = false;
      }
    },
  }
}
</script>

<style scoped>
.communities-page {
  padding: 2rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.search-box {
  position: relative;
  width: 300px;
}

.search-box input {
  width: 100%;
  padding: 0.5rem 1rem;
  padding-right: 2.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.search-box i {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #666;
}

.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
}

.filters select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
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
  display: flex;
  flex-direction: column;
  height: 100%;
}

.card-flex-content {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 1.5rem;
}

.actions, .member-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: auto;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.edit-btn, .btn.btn-primary, .leave-btn, .status-btn {
  min-width: 0;
  padding: 6px 10px;
  font-size: 0.95rem;
  flex: 1 1 0;
  white-space: nowrap;
}

.community-card:hover {
  transform: translateY(-5px);
}

.community-image img {
  width: 100%;
  height: 200px;
  object-fit: cover;
}

.community-info {
  padding: 1.5rem;
}

.community-info h3 {
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.category {
  color: #666;
  font-size: 0.9rem;
  margin: 5px 0;
}

.description {
  color: #666;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.stats {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  color: #666;
  font-size: 0.9rem;
}

.stats span {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.actions {
  display: flex;
  gap: 1rem;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 1rem;
  }

  .search-box {
    width: 100%;
  }

  .filters {
    flex-direction: column;
  }

  .communities-grid {
    grid-template-columns: 1fr;
  }

  .actions, .member-actions {
    flex-direction: column;
    align-items: stretch;
    gap: 0.5rem;
  }
}

.loading-container,
.error-container,
.no-results {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 2rem;
  text-align: center;
}

.error-container {
  color: #721c24;
  background-color: #f8d7da;
  border-radius: 4px;
  padding: 1rem;
}

.error-container button {
  margin-top: 1rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
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

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.edit-btn {
  flex: 1;
  padding: 8px 16px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.2s;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.edit-btn:hover {
  background-color: #2980b9;
}

.community-image {
  position: relative;
  overflow: hidden;
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

.status-btn {
  flex: 1;
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  font-weight: 500;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  cursor: default;
}

.status-btn i {
  font-size: 1rem;
}

.status-active {
  background-color: #ebfbf0;
  color: #1f9d55;
  border: 1px solid #c6f6d5;
}

.status-active i {
  color: #38a169;
}

.status-pending {
  background-color: #fff8e8;
  color: #f39c12;
  border: 1px solid #fce5c0;
}

.status-pending i {
  color: #f39c12;
}

/* Hata mesajları ve alertler için */
.alert {
  padding: 1rem;
  margin-bottom: 1rem;
  border-radius: 4px;
  color: white;
  text-align: center;
}

.alert-error {
  background-color: #e74c3c;
}

.alert-success {
  background-color: #2ecc71;
}

.alert-warning {
  background-color: #f39c12;
}

.alert-info {
  background-color: #3498db;
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

.error-dialog {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  background-color: #e74c3c;
  color: white;
  padding: 15px;
  z-index: 10000;
  text-align: center;
  animation: slideDown 0.3s ease-out;
}

.error-dialog.warning {
  background-color: #f39c12;
}

.error-dialog.success {
  background-color: #2ecc71;
}

.error-dialog-content {
  max-width: 800px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.error-dialog i {
  font-size: 1.2rem;
}

.error-dialog .close-btn {
  background: transparent;
  border: none;
  color: white;
  cursor: pointer;
  margin-left: 15px;
}

@keyframes slideDown {
  from {
    transform: translateY(-100%);
  }

  to {
    transform: translateY(0);
  }
}

.image-placeholder {
  height: 150px; /* Adjust height as needed */
  background-color: #e9ecef; /* Placeholder background */
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #6c757d;
  font-weight: bold;
  overflow: hidden; /* Ensure image doesn't overflow */
}

.image-placeholder img {
  max-width: 100%;
  max-height: 100%;
  object-fit: cover; /* Cover the area, might crop */
}

.btn-warning[disabled] {
  cursor: not-allowed;
  /* Add specific styles for pending button if needed */
}
.btn-secondary[disabled] {
    cursor: not-allowed;
    /* Add specific styles for already member or inactive button if needed */
}

.actions-loading {
    font-size: 0.8rem;
    color: #6c757d;
    padding: 0.5rem 0;
}

.member-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  gap: 10px;
}

.member-status {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 6px 12px;
  background-color: #ebfbf0;
  color: #1f9d55;
  border: 1px solid #c6f6d5;
  border-radius: 4px;
  font-size: 0.9em;
}

.member-status i {
  color: #38a169;
}

.leave-btn {
  padding: 6px 12px;
  background-color: #dc3545;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9em;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 5px;
}

.leave-btn:hover {
  background-color: #c82333;
}

.leave-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.leave-btn i {
  font-size: 0.9em;
}
</style>