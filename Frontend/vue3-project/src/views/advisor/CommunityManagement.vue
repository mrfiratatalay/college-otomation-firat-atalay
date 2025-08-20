<template>
    <div class="community-management">
      <!-- Üst Kısım: Topluluk Adı ve Açıklaması -->
      <div v-if="loading" class="loading-container">
        <p>Yükleniyor...</p>
      </div>
      <div v-else-if="error" class="error-container">
        <p>{{ error }}</p>
      </div>
      <div v-else>
        <div class="page-header">
          <div>
            <h1>{{ communityData.name }}</h1>
            <p class="community-description">{{ communityData.description }}</p>
          </div>
        </div>
    
        <!-- Orta Kısım: "Topluluk Bilgileri" Kartı -->
        <div class="info-section card">
          <div class="card-header">
            <h2>Topluluk Bilgileri</h2>
          </div>
          <div class="card-body">
            <div class="info-row">
              <strong>Oluşturulma Tarihi:</strong>
              <span>{{ formatDate(communityData.createdAt) }}</span>
            </div>
            <div class="info-row">
              <strong>Kategori:</strong>
              <span>{{ communityData.categoryName || 'Belirtilmemiş' }}</span>
            </div>
            <div class="info-row">
              <strong>Üye Sayısı:</strong>
              <span>{{ communityData.memberCount || 0 }}</span>
            </div>
            <div class="info-row">
              <strong>Toplam Etkinlik:</strong>
              <span>{{ communityData.eventCount || 0 }}</span>
            </div>
          </div>
        </div>
    
        <!-- Alt Kısım: Sekmeler (ÜYELER, DUYURULAR, ETKİNLİKLER) -->
        <div class="tabs-container">
          <button class="tab-btn" :class="{ active: activeTab === 'members' }" @click="activeTab = 'members'">
            ÜYELER
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 'memberRequests' }" @click="activeTab = 'memberRequests'">
            ÜYELİK İSTEKLERİ
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 'events' }" @click="activeTab = 'events'">
            ETKİNLİKLER
          </button>
          <button 
            class="tab-btn" 
            :class="{ active: activeTab === 'expenses' }"
            @click="activeTab = 'expenses'"
          >
            MALİYETLER
          </button>
        </div>
    
        <!-- Sekme İçerikleri -->
        <div class="tab-content">
          <!-- ÜYELER Sekmesi -->
          <div v-if="activeTab === 'members'" class="tab-pane fade show active">
            <h3>Üyeler</h3>
            <!-- Responsive grid for members -->
            <div v-if="members.length === 0" class="empty-state">
              <p>Bu toplulukta henüz üye bulunmuyor.</p>
            </div>
            <div v-else class="members-grid">
              <div class="member-card" v-for="member in members" :key="member.id">
                <img :src="member.imageUrl || '/images/default-avatar.png'" alt="Avatar" />
                <div class="member-info">
                  <h5>{{ member.name }}</h5>
                  <small>{{ member.role }}</small>
                </div>
              </div>
            </div>
          </div>
    
          <!-- ÜYELİK İSTEKLERİ Sekmesi -->
          <div v-else-if="activeTab === 'memberRequests'" class="tab-pane fade show active">
            <h3>Üyelik İstekleri</h3>
            <div v-if="pendingMembers.length === 0" class="empty-state">
              <p>Bekleyen üyelik isteği bulunmuyor.</p>
            </div>
            <div v-else class="member-requests-list">
              <div v-for="request in pendingMembers" :key="request.membershipId" class="member-request-card">
                <div class="member-request-info">
                  <div class="member-avatar">
                    <img :src="request.imageUrl || '/images/default-avatar.png'" alt="Avatar" />
                  </div>
                  <div class="member-details">
                    <h5>{{ request.name }}</h5>
                    <p>{{ formatDate(request.joinedAt) }} tarihinde katılmak istedi</p>
                  </div>
                </div>
                <div class="member-request-actions">
                  <button class="btn-approve" @click="approveMembership(request.membershipId)">
                    <i class="fas fa-check"></i> Onayla
                  </button>
                  <button class="btn-reject" @click="rejectMembership(request.membershipId)">
                    <i class="fas fa-times"></i> Reddet
                  </button>
                </div>
              </div>
            </div>
          </div>
    
          <!-- ETKİNLİKLER Sekmesi -->
          <div v-else-if="activeTab === 'events'" class="tab-pane fade show active">
            <h3>Etkinlikler</h3>
            <div v-if="events.length === 0" class="empty-state">
              <p>Bu toplulukta henüz etkinlik bulunmuyor.</p>
            </div>
            <div v-else class="event-list">
              <div v-for="event in events" :key="event.eventId" class="card mb-2 event-card">
                <div class="card-body">
                  <h6 class="card-title">{{ event.name }}</h6>
                  <p class="card-text">{{ event.description }}</p>
                  <div class="event-details">
                    <span><i class="fas fa-calendar"></i> {{ formatDate(event.startDate) }} - {{ formatDate(event.endDate) }}</span>
                    <span><i class="fas fa-users"></i> {{ event.participantCount || 0 }}/{{ event.maxParticipants }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Maliyetler Sekmesi İçeriği Eklendi -->
          <div v-else-if="activeTab === 'expenses'" class="tab-pane fade show active expenses-section"> 
            <div class="section-header">
              <h3>Maliyetler</h3>
              <button @click="openExpenseForm" class="primary-button">
                Yeni Maliyet Formu Oluştur
              </button> 
            </div>
          
            <div v-if="!expenses || !expenses.length" class="empty-state">
              <p>Bu topluluğa ait gider kaydı bulunmamaktadır.</p>
            </div>
          
            <div v-else class="table-responsive">
              <table class="custom-table expenses-table">
                <thead>
                  <tr>
                    <th>Tarih</th>
                    <th>Nakdi Yardım</th>
                    <th>Ayni Yardım</th>
                    <th>Açıklama</th>
                    <th>İşlemler</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="expense in expenses" :key="expense.id">
                    <td>{{ formatDate(expense.date) }}</td>
                    <td>{{ expense.cashSupport?.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' }) || '0,00 ₺' }}</td>
                    <td>{{ expense.inKindSupport?.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' }) || '0,00 ₺' }}</td>
                    <td class="description-cell">{{ expense.description }}</td>
                    <td class="actions-cell">
                      <button @click="openEditExpenseForm(expense)" class="btn btn-sm btn-primary" title="Düzenle">
                        <i class="fas fa-pencil-alt"></i>
                      </button>
                      <button @click="confirmDeleteExpense(expense.id)" class="btn btn-sm btn-danger" title="Sil">
                        <i class="fas fa-trash-alt"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div v-if="showExpenseForm" class="modal" @click="closeExpenseForm">
            <div class="modal-content" @click.stop>
              <div class="modal-header">
                <h2>{{ editMode ? 'Maliyet Düzenle' : 'Yeni Maliyet Formu' }}</h2>
                <button class="close-btn" @click="closeExpenseForm"><i class="fas fa-times"></i></button>
              </div>
              <div class="modal-body">
                <form @submit.prevent="submitExpenseForm">
                  <div class="form-group">
                    <label>Tarih</label>
                    <input type="date" v-model="newExpense.date" required>
                  </div>
                  <div class="form-group">
                    <label>Nakdi Yardım Tutarı (₺)</label>
                    <input type="number" step="0.01" v-model="newExpense.cashSupport" required>
                  </div>
                  <div class="form-group">
                    <label>Ayni Yardım Tutarı (₺)</label>
                    <input type="number" step="0.01" v-model="newExpense.inKindSupport" required>
                  </div>
                  <div class="form-group">
                    <label>Açıklama</label>
                    <textarea v-model="newExpense.description" required rows="3"></textarea>
                  </div>
                  <div class="form-actions">
                    <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
                      <i class="fas fa-save"></i> {{ isSubmitting ? 'Kaydediliyor...' : 'Kaydet' }}
                    </button>
                    <button type="button" class="btn btn-secondary" @click="closeExpenseForm" :disabled="isSubmitting">
                      <i class="fas fa-times"></i> İptal
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import clubService from '@/services/clubService';
  import authService from '@/services/authService';
  import categoryService from '@/services/categoryService';
  import api from '@/services/api';
  import { getExpensesByClub, addExpense, updateExpense, deleteExpense } from '@/services/expenseService';
  
  export default {
    name: 'AdvisorCommunityManagement',
    data() {
      return {
        activeTab: 'members',
        loading: false,
        error: null,
        isSubmitting: false,
        communityId: null,
        currentUserId: null,
        communityData: {
          clubId: null,
          name: '',
          description: '',
          createdAt: '',
          memberCount: 0,
          eventCount: 0,
          categoryId: null,
          categoryName: ''
        },
        members: [],
        pendingMembers: [],
        events: [],
        categories: [],
        expenses: [],
        showExpenseForm: false,
        newExpense: {
          date: '',
          cashSupport: 0,
          inKindSupport: 0,
          description: ''
        },
        editMode: false,
        currentExpenseId: null
      }
    },
    async created() {
      this.communityId = parseInt(this.$route.params.id);
      if (!this.communityId) {
        this.error = 'Geçersiz topluluk ID\'si';
        return;
      }
      
      try {
        this.loading = true;
        
        // Kullanıcı bilgilerini kontrol et
        const user = authService.getUser();
        console.log('Current user:', user);
        
        if (!user || user.userType !== 'advisor') {
          this.$router.push('/login');
          return;
        }
        
        // userId'yi doğru şekilde al
        this.currentUserId = parseInt(user.id || user.userId);
        console.log('Current userId:', this.currentUserId);

        // Kategori listesini al
        const categoriesResponse = await categoryService.getAllCategories();
        this.categories = categoriesResponse.data.categories || [];
        
        // Topluluk detaylarını ve üyeleri al
        await this.fetchClubDetailsAndMembers();
        
        // Etkinlikleri getir
        await this.fetchClubEvents();
        
        // Bekleyen üyelik isteklerini al
        await this.fetchPendingMemberships();

        // Maliyetleri getir (Added)
        await this.fetchExpenses(); 
        
      } catch (error) {
        console.error('Veri yüklenirken hata:', error);
        this.error = `Veriler yüklenirken hata: ${error.message || error}`;
      } finally {
        this.loading = false;
      }
    },
    methods: {
      formatDate(dateString) {
        if (!dateString) return '';
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        // Ensure dateString is treated correctly, maybe parse it first
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('tr-TR', options);
        } catch (e) {
            console.error("Error formatting date:", dateString, e);
            return dateString; // return original if parsing fails
        }
      },
      
      getCategoryName(categoryId) {
        if (!categoryId || !this.categories.length) return 'Kategori Belirtilmemiş';
        const category = this.categories.find(cat => cat.id === parseInt(categoryId));
        return category ? category.name : 'Kategori Belirtilmemiş';
      },
      
      async fetchPendingMemberships() {
        try {
          console.log('Bekleyen üyelik istekleri alınıyor');
          
          const response = await clubService.getPendingMemberships();
          console.log('Bekleyen üyelikler API yanıtı:', response);

          if (response.data && Array.isArray(response.data)) {
            // Seçili kulübe ait VE durumu 'Pending' olan başvuruları filtrele
            this.pendingMembers = response.data
              .filter(m => {
                const status = (m.status || m.Status || '').toLowerCase();
                const clubMatch = m.clubId === this.communityId;
                const isPending = status === 'pending' || status === 'bekliyor';
                return clubMatch && isPending;
              })
              .map(m => ({
                membershipId: m.membershipId || m.MembershipId,
                userId: m.userId || m.UserId,
                name: m.userName || m.UserName || m.Name || m.name || 'İsimsiz Üye',
                joinedAt: m.joinedAt || m.JoinedAt,
                status: m.status || m.Status
              }));
          } else {
            console.log('API yanıtında üyelik verileri bulunamadı');
            this.pendingMembers = [];
          }
          
          console.log('İşlenen bekleyen üyelikler:', this.pendingMembers);
        } catch (error) {
          console.error('Üyelik istekleri alınırken hata:', error);
          this.pendingMembers = [];
          this.$notify({
            type: 'error',
            message: 'Üyelik istekleri alınırken bir hata oluştu'
          });
        }
      },
      
      async approveMembership(membershipId) {
        try {
          this.loading = true;
          console.log('Üyelik onaylanıyor, membershipId:', membershipId);
          
          await clubService.approveMembership(membershipId);
          console.log('Üyelik başarıyla onaylandı');
          
          // Başarılı olunca listeyi güncelle
          this.pendingMembers = this.pendingMembers.filter(member => member.membershipId !== membershipId);
          
          // Üye sayısını güncelle
          this.communityData.memberCount += 1;
          
          // Başarı mesajı göster
          this.$notify({
            type: 'success',
            message: 'Üyelik başvurusu onaylandı'
          });
          
          // Üye listesini güncelle
          await this.refreshMembers();
        } catch (error) {
          console.error('Üyelik onaylanırken hata:', error);
          this.$notify({
            type: 'error',
            message: 'Üyelik onaylanırken bir hata oluştu'
          });
        } finally {
          this.loading = false;
        }
      },
      
      async rejectMembership(membershipId) {
        try {
          this.loading = true;
          console.log('Üyelik reddediliyor, membershipId:', membershipId);
          
          await clubService.rejectMembership(membershipId);
          console.log('Üyelik başarıyla reddedildi');
          
          // Başarılı olunca listeyi güncelle
          this.pendingMembers = this.pendingMembers.filter(member => member.membershipId !== membershipId);
          
          // Başarı mesajı göster
          this.$notify({
            type: 'success',
            message: 'Üyelik başvurusu reddedildi'
          });
        } catch (error) {
          console.error('Üyelik reddedilirken hata:', error);
          this.$notify({
            type: 'error',
            message: 'Üyelik reddedilirken bir hata oluştu'
          });
        } finally {
          this.loading = false;
        }
      },
      
      async fetchClubDetailsAndMembers() {
        try {
          // Get club details
          const clubResponse = await clubService.getClubById(this.communityId);
          console.log('Club details response:', clubResponse);

          if (clubResponse.data && clubResponse.data.club) {
            const clubData = clubResponse.data.club;
            
            // Update community data
            this.communityData = {
              ...clubData,
              categoryName: this.getCategoryName(clubData.categoryId)
            };

            // Get club members
            const membersResponse = await clubService.getClubMembers(this.communityId);
            console.log('Members response:', membersResponse);
            console.log('Current user ID for comparison:', this.currentUserId);

            if (membersResponse.data && Array.isArray(membersResponse.data)) {
              this.members = membersResponse.data.map(member => {
                console.log('Processing member:', member);
                const isSelf = member.userId === this.currentUserId;
                console.log('Is self?', isSelf, member.userId, this.currentUserId);
                
                return {
                  id: member.userId,
                  membershipId: member.membershipId,
                  name: isSelf ? `${member.name || member.userName} (Siz)` : (member.name || member.userName || 'Unnamed Member'),
                  role: member.role || 'member',
                  joinedAt: member.joinedAt,
                  imageUrl: member.imageUrl
                };
              });
              console.log('Processed members:', this.members);
            } else {
              console.log('No member data found or empty');
              this.members = [];
            }
          } else {
            throw new Error('Could not fetch club data');
          }
        } catch (error) {
          console.error('Error fetching club details:', error);
          this.$notify({
            type: 'error',
            message: 'Error loading club information'
          });
        }
      },

      // Üye listesini yenileme metodu
      async refreshMembers() {
        await this.fetchClubDetailsAndMembers();
      },

      async fetchClubEvents() {
        try {
          console.log('Kulüp etkinlikleri getiriliyor...');
          const response = await clubService.getClubEvents(this.communityId);
          
          if (response.data && Array.isArray(response.data)) {
            this.events = response.data.map(event => ({
              eventId: event.eventId || event.id,
              name: event.name || event.title || 'İsimsiz Etkinlik',
              description: event.description || '',
              startDate: event.startDate || event.startDateTime,
              endDate: event.endDate || event.endDateTime,
              location: event.location || '',
              maxParticipants: event.maxParticipants || event.capacity || 0,
              participantCount: event.participantCount || 0,
              status: event.status || 'active'
            }));
          } else {
            console.log('API yanıtında etkinlik verisi bulunamadı');
            this.events = [];
          }
          
          console.log('Getirilen etkinlikler:', this.events);
        } catch (error) {
          console.error('Etkinlikler getirilirken hata:', error);
          this.$notify({
            type: 'error',
            message: 'Etkinlikler yüklenirken bir hata oluştu'
          });
          this.events = [];
        }
      },

      // Added method to fetch expenses
      async fetchExpenses() {
        if (!this.communityId) return;
        console.log(`Fetching expenses for club ID: ${this.communityId}`);
        try {
          this.loading = true;
          const response = await getExpensesByClub(this.communityId); 
          console.log('API Response for expenses:', response); // Log the full response

          // Check if response and response.data exist
          if (response && response.data) {
            // Check if response.data is the array we expect
            if (Array.isArray(response.data)) {
              this.expenses = response.data;
              console.log('Successfully fetched expenses:', this.expenses);
            } else {
              // Handle cases where response.data is not an array (e.g., an object containing the array)
              // This part might need adjustment based on the actual API response structure
              console.warn('Expenses data received, but not in expected array format:', response.data);
              // Attempt to find the array if nested, e.g., response.data.expenses
              // If you know the structure, adjust here. Otherwise, default to empty.
              this.expenses = []; 
            }
          } else {
            console.warn('No data found in expenses API response.');
            this.expenses = [];
          }
        } catch (error) {
          console.error('Giderler getirilirken hata:', error);
          // Log the detailed error object if available
          if (error.response) {
            console.error('Error response data:', error.response.data);
            console.error('Error response status:', error.response.status);
          }
          this.error = 'Giderler yüklenirken bir hata oluştu.'; // Set error message
          this.expenses = []; // Clear expenses on error
        } finally {
          this.loading = false;
        }
      },

      openExpenseForm() {
        this.editMode = false; // Ensure it's in 'add' mode
        this.currentExpenseId = null;
        this.resetExpenseForm();
        this.showExpenseForm = true;
      },
      closeExpenseForm() {
        this.showExpenseForm = false;
        this.resetExpenseForm();
        this.editMode = false;
        this.currentExpenseId = null;
      },
      resetExpenseForm() {
        this.newExpense = {
          date: new Date().toISOString().slice(0, 10), // Default to today
          cashSupport: 0,
          inKindSupport: 0,
          description: ''
        };
      },
      // Method to open form for editing
      openEditExpenseForm(expense) {
        this.editMode = true;
        this.currentExpenseId = expense.id;
        // Format date correctly for the input type='date'
        const dateForInput = expense.date ? new Date(expense.date).toISOString().slice(0, 10) : '';
        this.newExpense = {
          date: dateForInput,
          cashSupport: expense.cashSupport,
          inKindSupport: expense.inKindSupport,
          description: expense.description
        };
        this.showExpenseForm = true;
      },
      async submitExpenseForm() {
        this.isSubmitting = true;
        try {
          const expenseData = {
            ...this.newExpense,
            clubId: this.communityId // Ensure clubId is included
          };

          if (this.editMode && this.currentExpenseId) {
            // Call update service
            await updateExpense(this.currentExpenseId, expenseData);
            alert('Gider başarıyla güncellendi.');
          } else {
            // Call add service
            await addExpense(expenseData);
            alert('Gider başarıyla eklendi.');
          }
          
          this.closeExpenseForm();
          await this.fetchExpenses(); // Refresh the list
        } catch (error) {
          console.error('Gider kaydedilirken hata:', error);
          alert(`Hata: ${error.response?.data?.message || error.message || 'Gider kaydedilemedi.'}`);
        } finally {
          this.isSubmitting = false;
        }
      },
      // Method to confirm and delete expense
      async confirmDeleteExpense(expenseId) {
        if (confirm('Bu gideri silmek istediğinize emin misiniz?')) {
          try {
            await deleteExpense(expenseId);
            alert('Gider başarıyla silindi.');
            await this.fetchExpenses(); // Refresh the list
          } catch (error) {
            console.error('Gider silinirken hata:', error);
            alert(`Hata: ${error.response?.data?.message || error.message || 'Gider silinemedi.'}`);
          }
        }
      }
    }
  }
  </script>
  
  <style scoped>
  .community-management {
    padding: 1rem;
  }
  
  /* Loading and Error States */
  .loading-container, 
  .error-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 200px;
    background-color: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    margin: 1rem 0;
    padding: 2rem;
  }
  
  .error-container {
    color: #e74c3c;
  }
  
  /* Page Header */
  .page-header {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    margin-bottom: 2rem;
  }
  .page-header h1 {
    margin: 0;
    color: #2c3e50;
  }
  .community-description {
    color: #7f8c8d;
  }
  
  /* Info Section */
  .info-section {
    margin-bottom: 2rem;
  }
  .info-section .card-header {
    background: #f8f9fa;
    border-bottom: 1px solid #e9ecef;
    padding: 1rem;
  }
  .info-section .card-header h2 {
    margin: 0;
    font-size: 1.25rem;
  }
  .info-section .card-body {
    padding: 1.5rem;
  }
  .info-row {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 0.5rem;
    font-size: 0.95rem;
  }
  
  /* Tabs Container */
  .tabs-container {
    display: flex;
    gap: 1rem;
    margin-bottom: 1rem;
    flex-wrap: wrap;
  }
  .tab-btn {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 4px;
    background: #ecf0f1;
    color: #2c3e50;
    cursor: pointer;
    font-weight: 600;
    transition: background 0.3s;
  }
  .tab-btn:hover {
    background: #dcdde1;
  }
  .tab-btn.active {
    background: #3498db;
    color: white;
  }
  
  /* Tab Content */
  .tab-content {
    background: white;
    padding: 1rem;
    border-radius: 4px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }
  
  /* Members Grid */
  .members-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1rem;
    margin-top: 1rem;
  }
  .member-card {
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    padding: 1rem;
    display: flex;
    align-items: center;
    gap: 1rem;
  }
  .member-card img {
    width: 55px;
    height: 55px;
    object-fit: cover;
    border-radius: 50%;
  }
  .member-info h5 {
    margin: 0;
    color: #2c3e50;
  }
  .member-info small {
    color: #7f8c8d;
  }
  
  /* Empty State */
  .empty-state {
    text-align: center;
    padding: 2rem;
    color: #7f8c8d;
  }
  
  /* Member Requests */
  .member-requests-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    margin-top: 1rem;
  }
  
  .member-request-card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    padding: 1rem;
  }
  
  .member-request-info {
    display: flex;
    align-items: center;
    gap: 1rem;
  }
  
  .member-avatar img {
    width: 50px;
    height: 50px;
    border-radius: 50%;
    object-fit: cover;
  }
  
  .member-details h5 {
    margin: 0;
    color: #2c3e50;
  }
  
  .member-details p {
    margin: 0.25rem 0 0;
    font-size: 0.85rem;
    color: #7f8c8d;
  }
  
  .member-request-actions {
    display: flex;
    gap: 0.5rem;
  }
  
  .btn-approve, .btn-reject {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .btn-approve {
    background: #2ecc71;
    color: white;
  }
  
  .btn-reject {
    background: #e74c3c;
    color: white;
  }
  
  /* Event Cards */
  .event-card {
    margin-bottom: 1rem;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }
  
  .event-details {
    display: flex;
    gap: 1rem;
    margin-top: 0.5rem;
    font-size: 0.85rem;
    color: #7f8c8d;
  }
  
  /* Responsive Styles */
  
  /* Tablets (max-width: 992px) */
  @media (max-width: 992px) {
    .community-management {
      padding: 1.5rem;
    }
    .page-header h1 {
      font-size: 1.75rem;
    }
    .tabs-container {
      flex-wrap: wrap;
    }
  }
  
  /* Mobile (max-width: 576px) */
  @media (max-width: 576px) {
    .page-header {
      align-items: flex-start;
    }
    .tabs-container {
      flex-direction: column;
      gap: 0.75rem;
      width: 100%;
    }
    .tab-btn {
      width: 100%;
      text-align: left;
      padding: 0.75rem 1rem;
    }
    .tab-content {
      padding: 0.75rem;
    }
    .members-grid {
      grid-template-columns: 1fr;
    }
    .member-request-card {
      flex-direction: column;
      align-items: flex-start;
    }
    .member-request-actions {
      margin-top: 1rem;
      width: 100%;
    }
    .btn-approve, .btn-reject {
      flex: 1;
      justify-content: center;
    }
  }

  /* Add styles for the expenses section if needed */
  .expenses-section .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }

  .expenses-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
  }

  /* Refined Table Styles */
  .expenses-table th,
  .expenses-table td {
    border: none; /* Remove default borders */
    border-bottom: 1px solid #e9ecef; /* Lighter bottom border for rows */
    padding: 1rem; /* Increase padding slightly */
    text-align: left;
    vertical-align: middle;
  }

  .expenses-table th {
    background-color: #f8f9fa;
    font-weight: 600; /* Slightly bolder header */
    color: #495057; /* Header text color */
    font-size: 0.9rem;
    border-bottom-width: 2px; /* Thicker bottom border for header */
  }

  /* Remove zebra striping for a cleaner look like the image */
  .expenses-table tbody tr:nth-child(even) {
    background-color: transparent; 
  }

  .expenses-table tbody tr:hover {
    background-color: #f1f3f5; /* Subtle hover effect */
  }

  /* Style for action buttons - Updated for image style */
  .expenses-table td.actions-cell {
    text-align: center;
    white-space: nowrap;
    width: auto; /* Let width be determined by content */
    padding-right: 1rem; /* Ensure some padding on the right */
  }

  .actions-cell .btn {
    width: 32px; /* Fixed width for square look */
    height: 32px; /* Fixed height for square look */
    padding: 0; /* Remove padding */
    font-size: 0.9rem; /* Adjust icon size if needed */
    margin-right: 6px; /* Space between buttons */
    border-radius: 0.25rem; /* Match image border radius */
  }
  .actions-cell .btn i {
    margin: 0; /* Remove default icon margin */
  }

  .expenses-table td.description-cell {
      max-width: 300px;
      white-space: normal; /* Allow text wrapping */
      word-wrap: break-word; /* Break long words */
  }

  /* Generic Button Styles are mostly kept, but we adjust .actions-cell .btn above */
  .btn {
    display: inline-flex; 
    align-items: center;
    justify-content: center;
    padding: 0.375rem 0.75rem;
    font-size: 0.9rem;
    font-weight: 400;
    line-height: 1.5;
    text-align: center;
    vertical-align: middle;
    cursor: pointer;
    border: 1px solid transparent;
    border-radius: 0.25rem;
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out, border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    /* Remove global margin-right if handled specifically */
    /* margin-right: 5px; */ 
  }
  .btn i {
    /* Adjust default icon margin for text buttons if needed */
    /* margin-right: 0.3em; */ 
  }

  /* Specific button colors */
  .btn-primary {
    color: #fff;
    background-color: #007bff;
    border-color: #007bff;
  }
  .btn-primary:hover {
    background-color: #0056b3;
    border-color: #0056b3;
  }
  .btn-secondary {
    color: #fff;
    background-color: #6c757d;
    border-color: #6c757d;
  }
  .btn-secondary:hover {
    background-color: #5a6268;
    border-color: #5a6268;
  }
  .btn-warning {
    color: #212529;
    background-color: #ffc107;
    border-color: #ffc107;
  }
  .btn-warning:hover {
    background-color: #e0a800;
    border-color: #e0a800;
  }
  .btn-danger {
    color: #fff;
    background-color: #dc3545;
    border-color: #dc3545;
  }
  .btn-danger:hover {
    background-color: #c82333;
    border-color: #c82333;
  }
  .btn:disabled {
     opacity: 0.65;
     cursor: not-allowed;
  }


  /* Modal Styling Improvements */
  .modal {
    position: fixed;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0,0,0,0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1050; 
    overflow-y: auto; /* Allow modal scroll if content overflows */
  }

  .modal-content {
    background: white;
    padding: 20px;
    border-radius: 8px;
    width: 90%;
    max-width: 500px; 
    box-shadow: 0 5px 15px rgba(0,0,0,0.2);
    margin: 1.75rem auto; /* Add margin for scroll */
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid #e9ecef;
    padding-bottom: 10px;
    margin-bottom: 15px;
  }

  .modal-header h2 {
    margin: 0;
    font-size: 1.25rem; /* Slightly smaller modal title */
  }

  .close-btn {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
  }
  
  .modal-body .form-group {
    margin-bottom: 1rem;
  }

  .modal-body label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 600; /* Slightly bolder labels */
    font-size: 0.9rem;
  }

  .modal-body input[type="text"],
  .modal-body input[type="number"],
  .modal-body input[type="date"],
  .modal-body textarea {
    width: 100%;
    padding: 0.5rem 0.75rem; /* Adjusted padding */
    border: 1px solid #ced4da;
    border-radius: 4px;
    font-size: 0.9rem;
    line-height: 1.5;
    box-sizing: border-box; /* Include padding and border in the element's total width and height */
  }
  .modal-body textarea {
      min-height: 80px; /* Give textarea a bit more height */
  }

  .form-actions {
    display: flex;
    justify-content: flex-end;
    margin-top: 1.5rem; 
  }
  /* Force margin-left specifically on the secondary button within form-actions */
  .form-actions .btn-secondary {
      margin-left: 15px !important; /* Use !important as a last resort if needed */
  }

  /* Styles for the primary button (add expense form) are kept */
  .primary-button {
    padding: 0.5rem 1rem;
    background-color: #007bff; 
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.3s ease;
  }

  .primary-button:hover {
    background-color: #0056b3;
  }

  .primary-button:disabled {
    background-color: #cccccc;
    cursor: not-allowed;
  }

  .submit-btn:disabled, .cancel-btn:disabled {
     opacity: 0.7;
     cursor: not-allowed;
  }
  </style>
  