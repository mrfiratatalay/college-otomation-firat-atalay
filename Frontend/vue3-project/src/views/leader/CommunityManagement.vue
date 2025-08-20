<template>
  <div class="community-management">
    <!-- Üst Kısım: Topluluk Bilgileri -->
    <div class="page-header">
      <div class="club-profile">
        <div class="club-image-container">
          <img 
            :src="clubDetails?.imageUrl || '/default-club.png'" 
            alt="Topluluk Fotoğrafı" 
            class="club-image"
          >
          <button v-if="!isEditing" @click="startEditing" class="edit-image-btn">
            <i class="fas fa-camera"></i>
          </button>
        </div>
        <div class="club-info">
          <div v-if="!isEditing" class="club-details">
            <h1>{{ clubDetails?.name || 'Topluluk Yönetimi' }}</h1>
            <p class="club-description">{{ clubDetails?.description }}</p>
            <button class="edit-btn" @click="startEditing">
              <i class="fas fa-edit"></i> Düzenle
            </button>
          </div>
          <div v-else class="club-edit-form">
            <div class="form-group">
              <label>Topluluk Adı</label>
              <input 
                type="text" 
                v-model="editedClub.name" 
                class="form-control"
                placeholder="Topluluk Adı"
              >
            </div>
            
            <div class="form-group">
              <label>Açıklama</label>
              <textarea 
                v-model="editedClub.description" 
                class="form-control"
                placeholder="Topluluk Açıklaması"
                rows="3"
              ></textarea>
            </div>

            <div class="form-group">
              <label>Topluluk Fotoğrafı</label>
              <div class="image-upload-container">
                <div class="image-upload-box" @click="triggerFileInput">
                  <input 
                    type="file" 
                    ref="imageInput" 
                    @change="handleImageChange" 
                    accept="image/*" 
                    style="display: none;"
                  >
                  <template v-if="!editedClub.imageUrl">
                    <div class="upload-icon">
                      <i class="fas fa-cloud-upload-alt"></i>
                    </div>
                    <div class="upload-text">
                      Görsel yüklemek için tıklayın veya sürükleyin
                    </div>
                  </template>
                  <div v-if="editedClub.imageUrl" class="preview-container">
                    <img :src="editedClub.imageUrl" alt="Topluluk Görseli">
                    <button class="remove-image" @click.stop="removeImage">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="edit-actions">
              <button class="btn btn-primary" @click="saveChanges">
                <i class="fas fa-save"></i> Kaydet
              </button>
              <button class="btn btn-secondary" @click="cancelEditing">
                <i class="fas fa-times"></i> İptal
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-container">
      <p>Yükleniyor...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p>{{ error }}</p>
      <button @click="loadData" class="btn btn-primary">Tekrar Dene</button>
    </div>

    <div v-else>
      <!-- Topluluk İstatistikleri -->
      <div class="stats-wrapper" v-if="clubDetails">
        <div class="stats-section">
          <div class="stat-card">
            <div class="stat-card-content">
              <i class="fas fa-users"></i>
              <div class="stat-info">
                <h3>{{ members.length }}</h3>
                <p>Toplam Üye</p>
              </div>
              <div style="flex-grow: 1; display: flex; justify-content: flex-end;">
                <button class="modern-button department-btn" @click="showDepartmentDistributionModal">
                  <i class="fas fa-chart-pie"></i> Bölüm/Program Dağılımı
                </button>
              </div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-card-content">
              <i class="fas fa-calendar"></i>
              <div class="stat-info">
                <h3>{{ clubDetails.eventCount || 0 }}</h3>
                <p>Toplam Etkinlik</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Sekmeler -->
      <div class="tabs-container">
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'applications' }"
          @click="activeTab = 'applications'"
        >
          <i class="fas fa-file-alt"></i> Başvurular
        </button>
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'members' }"
          @click="activeTab = 'members'"
        >
          <i class="fas fa-users"></i> Üyeler
        </button>
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'events' }"
          @click="activeTab = 'events'"
        >
          <i class="fas fa-calendar-alt"></i> Etkinlikler
        </button>
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'announcements' }"
          @click="activeTab = 'announcements'"
        >
          <i class="fas fa-bullhorn"></i> Duyurular
        </button>
        <!-- Maliyetler Sekmesi Butonu -->
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'expenses' }"
          @click="activeTab = 'expenses'"
        >
          <i class="fas fa-coins"></i> Maliyetler
        </button>
        <!-- Rezervasyon Sekmesi Butonu -->
         <button 
          class="tab-button" 
          :class="{ active: activeTab === 'reservation' }"
          @click="activeTab = 'reservation'"
        >
          <i class="fas fa-clock"></i> Rezervasyon
        </button>
      </div>


      <!-- Bekleyen Başvurular Sekmesi -->
      <div v-if="activeTab === 'applications'" class="tab-content">
      <div class="applications-section">
        <h2>Bekleyen Başvurular</h2>
        
        <div v-if="!applications.length" class="no-applications">
          <p>Bekleyen başvuru bulunmamaktadır.</p>
        </div>

        <div v-else class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Üye</th>
                <th>Rol</th>
                <th>Durum</th>
                <th>Kayıt Tarihi</th>
                <th>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="application in applications" :key="application.membershipId">
                <td class="user-info">
                  <img :src="application.profileImage || '/default-avatar.png'" alt="Profil" class="user-avatar">
                  <div>
                    <div class="user-name">{{ application.name }} {{ application.surname }}</div>
                    <div class="user-id">{{ application.schoolNumber }}</div>
                  </div>
                </td>
                <td><span class="role-badge">Üye</span></td>
                <td><span class="status-badge pending">Bekliyor</span></td>
                <td>{{ formatDate(application.joinedAt) }}</td>
                <td class="actions">
                  <button class="action-btn approve" @click="approveApplication(application.membershipId)" title="Onayla">
                    <i class="fas fa-check"></i>
                  </button>
                  <button class="action-btn reject" @click="rejectApplication(application.membershipId)" title="Reddet">
                    <i class="fas fa-times"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          </div>
        </div>
      </div>

      <!-- Topluluk Üyeleri Sekmesi -->
      <div v-if="activeTab === 'members'" class="tab-content">
      <div class="members-section">
        <h2>Topluluk Üyeleri</h2>
        
        <!-- Arama ve Filtreleme Bölümü -->
        <div class="filters-section">
          <div class="search-box">
            <input type="text" v-model="memberSearchQuery" placeholder="Üye ara...">
            <i class="fas fa-search"></i>
          </div>
          
          <div class="filter-group">
            <select v-model="departmentFilter">
              <option value="">Tüm Bölümler</option>
              <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                {{ dept.name }}
              </option>
            </select>
            <input 
              type="date" 
              v-model="dateFilter"
              class="date-filter"
              :max="getCurrentDate()"
            >
          </div>
        </div>

        <div v-if="!filteredMembers.length" class="no-members">
          <p>Üye bulunmamaktadır.</p>
        </div>

        <div v-else class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Üye Adı</th>
                <th>Bölüm</th>
                <th>Rol</th>
                <th>Durum</th>
                <th>Kayıt Tarihi</th>
                <th>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="member in filteredMembers" :key="member.membershipId">
                <td class="user-info">
                  <img :src="member.profileImage || '/default-avatar.png'" alt="Profil" class="user-avatar">
                  <div>
                    <div class="user-name">{{ member.name }} {{ member.surname }}</div>
                    <div class="user-id">{{ member.schoolNumber }}</div>
                  </div>
                </td>
                <td>{{ getDepartmentName(member.departmentId) }}</td>
                <td>
                  <span class="role-badge" :class="member.role.toLowerCase()">
                    {{ member.role }}
                  </span>
                </td>
                <td><span class="status-badge active">Aktif</span></td>
                <td>{{ formatDate(member.joinedAt) }}</td>
                <td class="actions">
                  <button class="action-btn info" @click="showMemberDetails(member)" title="Detaylar">
                    <i class="fas fa-info-circle"></i>
                  </button>
                  <button class="action-btn remove" @click="confirmRemoveMember(member)" title="Üyeyi Çıkar">
                    <i class="fas fa-user-minus"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          </div>
        </div>
      </div>

      <!-- Etkinlikler Sekmesi -->
      <div v-if="activeTab === 'events'" class="tab-content">
        <div class="events-header">
          <h2>Etkinlikler</h2>
          <div class="button-group">
            <button @click="showEventForm = true" class="primary-button">
              Yeni Etkinlik Oluştur
            </button>
            <button @click="showResultReportForm" class="secondary-button">
              Sonuç Bildiri Formu
            </button>
          </div>
        </div>

        <div class="events-container">
          <div class="events-list">
            <div v-for="event in events" :key="event.id" class="event-card" @click="openEventDetailModal(event)">
              <div class="event-header">
                <div class="event-status-badge" :class="getEventStatusClass(event.status)">
                  {{ getEventStatusText(event.status) }}
                </div>
                <div class="event-image-wrapper">
                  <img :src="event.image || '/default-event.png'" :alt="event.title" class="event-image">
                </div>
              </div>
              
              <div class="event-content">
                <div class="event-main-info">
                  <h3 class="event-title">{{ event.title }}</h3>
                  <p class="event-description">{{ event.description }}</p>
                </div>

                <div class="event-meta">
                  <div class="meta-group">
                    <div class="meta-item">
                      <i class="fas fa-calendar-alt"></i>
                      <div class="meta-text">
                        <span class="meta-label">Başlangıç</span>
                        <span class="meta-value">{{ formatDate(event.startDate) }}</span>
                        <span class="meta-time">{{ event.startTime }}</span>
                      </div>
                    </div>

                    <div class="meta-item">
                      <i class="fas fa-calendar-check"></i>
                      <div class="meta-text">
                        <span class="meta-label">Bitiş</span>
                        <span class="meta-value">{{ formatDate(event.endDate) }}</span>
                        <span class="meta-time">{{ event.endTime }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="participants-info">
                    <i class="fas fa-users"></i>
                    <div class="participants-text">
                      <span class="participants-count">{{ event.currentParticipants }}/{{ event.maxParticipants }}</span>
                      <span class="participants-label">Katılımcı</span>
                    </div>
                    <div class="participants-bar">
                      <div class="participants-progress" 
                           :style="{ width: (event.currentParticipants / event.maxParticipants * 100) + '%' }">
                      </div>
                    </div>
                  </div>
                </div>

                <div v-if="isAdvisor && event.status === 'pending'" class="event-actions">
                  <button @click="approveEvent(event.id)" class="action-button approve">
                    <i class="fas fa-check"></i> Onayla
                  </button>
                  <button @click="rejectEvent(event.id)" class="action-button reject">
                    <i class="fas fa-times"></i> Reddet
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Duyurular Sekmesi -->
      <div v-if="activeTab === 'announcements'" class="tab-content">
        <div class="events-header">
          <h2>Duyurular</h2>
          <button @click="showAnnouncementForm = true" class="primary-button">
            Yeni Duyuru Oluştur
          </button>
        </div>

        <div class="events-container">
          <div class="events-list">
            <div v-for="announcement in announcements" :key="announcement.id" class="event-card">
              <div class="event-header">
                <div class="event-status-badge" :class="getEventStatusClass(announcement.status)">
                  {{ getAnnouncementStatusText(announcement.status) }}
                </div>
                <div class="event-image-wrapper">
                  <img 
                    :src="getImageUrl(announcement.imageUrl)" 
                    :alt="announcement.title" 
                    class="event-image"
                    @error="handleImageError"
                  >
                </div>
              </div>
              
              <div class="event-content">
                <div class="event-main-info">
                  <h3 class="event-title">{{ announcement.title }}</h3>
                  <p class="event-description">{{ announcement.content }}</p>
                </div>

                <div class="event-meta">
                  <div class="meta-item">
                    <i class="fas fa-calendar-alt"></i>
                    <div class="meta-text">
                      <span class="meta-label">Oluşturulma Tarihi</span>
                      <span class="meta-value">{{ formatDate(announcement.createdAt) }}</span>
                    </div>
                  </div>
                </div>

                <div v-if="announcement.status === 'pending'" class="event-actions">
                  <button @click="approveAnnouncement(announcement.id)" class="action-button approve">
                    <i class="fas fa-check"></i> Onayla
                  </button>
                  <button @click="rejectAnnouncement(announcement.id)" class="action-button reject">
                    <i class="fas fa-times"></i> Reddet
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Duyuru Form Modal -->
        <div v-if="showAnnouncementForm" class="modal" @click="closeAnnouncementForm">
          <div class="modal-content" @click.stop>
            <div class="modal-header">
              <h2>Yeni Duyuru Başvurusu</h2>
              <button class="close-btn" @click="closeAnnouncementForm">
                <i class="fas fa-times"></i>
              </button>
            </div>
            <div class="modal-body">
              <form @submit.prevent="submitAnnouncementForm">
                <div class="form-group">
                  <label>Duyuru Başlığı</label>
                  <input type="text" v-model="newAnnouncement.title" required>
                </div>
                <div class="form-group">
                  <label>İçerik</label>
                  <textarea v-model="newAnnouncement.content" required rows="4"></textarea>
                </div>
                <div class="form-group">
                  <label>Duyuru Görseli</label>
                  <div class="image-upload-container">
                    <div class="image-upload-box" @click="triggerFileInput">
                      <input 
                        type="file" 
                        ref="fileInput" 
                        @change="handleAnnouncementImageUpload" 
                        accept="image/*" 
                        style="display: none;"
                      >
                      <div v-if="!newAnnouncement.imagePreview">
                        <div class="upload-icon">
                          <i class="fas fa-cloud-upload-alt"></i>
                        </div>
                        <div class="upload-text">
                          Görsel yüklemek için tıklayın veya sürükleyin
                        </div>
                      </div>
                      <div v-else class="preview-container">
                        <img :src="newAnnouncement.imagePreview" alt="Duyuru Görseli">
                      </div>
                    </div>
                  </div>
                </div>
                <div class="form-actions">
                  <button type="submit" class="submit-btn">Gönder</button>
                  <button type="button" class="cancel-btn" @click="closeAnnouncementForm">İptal</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Maliyetler Sekmesi -->
<div v-if="activeTab === 'expenses'" class="tab-content">
  <div class="events-header">
    <h2>Maliyetler</h2>
    <button @click="showExpenseForm = true" class="primary-button">
      Yeni Maliyet Formu Oluştur
    </button>
  </div>

  <div v-if="expenses && !expenses.length" class="no-content">
    Bu kulübe ait gider kaydı bulunmamaktadır.
  </div>

  <div v-else class="table-responsive">
    <table class="custom-table">
      <thead>
        <tr>
          <th>Tarih</th>
          <th>Nakdi Yardım</th>
          <th>Ayni Yardım</th>
          <th>Açıklama</th>
          <th>Belge</th> <!-- Belge sütunu -->
          <th>İşlemler</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="expense in expenses" :key="expense.id">
          <td>{{ formatDate(expense.date) }}</td>
          <td>{{ expense.cashSupport.toLocaleString('tr-TR') }} ₺</td>
          <td>{{ expense.inKindSupport.toLocaleString('tr-TR') }} ₺</td>
          <td>{{ expense.description }}</td>
          <td>
            <button 
              v-if="expense.dokumanUrl" 
              class="action-btn view" 
              @click="viewDocument(expense.dokumanUrl)" 
              title="Belgeyi Görüntüle">
              Görüntüle
            </button>
          </td>
          <td class="actions">
            <button 
              class="action-btn delete" 
              @click="deleteExpense(expense.id)" 
              title="Sil">
              <i class="fas fa-trash"></i>
            </button>
            <button 
              class="action-btn edit" 
              @click="editExpense(expense)" 
              title="Düzenle">
              <i class="fas fa-edit"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</div>

<!-- Rezervasyon Sekmesi -->
<div v-if="activeTab === 'reservation'" class="tab-content">
  <ReservationModal :inline="true" />
</div>

  <!-- Maliyet Formu Modal -->
<div v-if="showExpenseForm" class="modal" @click="closeExpenseForm">
  <div class="modal-content" @click.stop>
    <div class="modal-header">
      <h2>Yeni Maliyet Formu</h2>
      <button class="close-btn" @click="closeExpenseForm">
        <i class="fas fa-times"></i>
      </button>
    </div>
    <div class="modal-body">
      <form @submit.prevent="submitExpenseForm">
        <div class="form-group">
          <label>Tarih</label>
          <input type="date" v-model="newExpense.date" required :max="getCurrentDate()">
        </div>
        <div class="form-group">
          <label>Nakdi Yardım (₺)</label>
          <input type="number" v-model="newExpense.cashSupport" min="0" required>
        </div>
        <div class="form-group">
          <label>Ayni Yardım (₺)</label>
          <input type="number" v-model="newExpense.inKindSupport" min="0" required>
        </div>
        <div class="form-group">
          <label>Açıklama</label>
          <textarea v-model="newExpense.description" rows="3" required maxlength="500"></textarea>
        </div>
        <div class="form-group">
          <label>Belge</label>
          <div class="document-upload-container">
            <div class="document-upload-box" @click="triggerDocumentInput">
              <input 
                type="file" 
                ref="documentInput" 
                @change="handleDocumentUpload" 
                style="display: none;"
              >
              <div v-if="!newExpense.documentName">
                <div class="upload-icon">
                  <i class="fas fa-file-upload"></i>
                </div>
                <div class="upload-text">
                  Belge yüklemek için tıklayın
                </div>
              </div>
              <div v-else class="document-preview">
                <i class="fas fa-file-alt"></i>
                <span>{{ newExpense.documentName }}</span>
                <button 
                  type="button" 
                  class="remove-document" 
                  @click.stop="removeDocument"
                >
                  <i class="fas fa-times"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary">Gönder</button>
          <button type="button" class="btn btn-secondary" @click="closeExpenseForm">İptal</button>
        </div>
      </form>
    </div>
  </div>
</div>

<!--rezervasyon modal-->
  <ReservationModal 
    v-if="showReservationModal" 
    :event-id="selectedEvent.id" 
    @close="showReservationModal = false" 
  />

    <!-- Üye Detay Modal -->
    <div v-if="selectedMember" class="modal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Üye Detayları</h2>
          <button class="close-btn" @click="closeModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body" v-if="memberDetails">
          <div class="member-details">
            <div class="detail-row">
              <span class="label">Ad Soyad:</span>
              <span class="value">{{ memberDetails.name }} {{ memberDetails.surname }}</span>
            </div>
            <div class="detail-row">
              <span class="label">E-posta:</span>
              <span class="value">{{ memberDetails.email }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Telefon:</span>
              <span class="value">{{ memberDetails.phoneNumber }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Okul Numarası:</span>
              <span class="value">{{ memberDetails.schoolNumber }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Bölüm:</span>
              <span class="value">{{ getDepartmentName(memberDetails.departmentId) }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Üyelik Rolü:</span>
              <span class="value">{{ selectedMember.role }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Kayıt Tarihi:</span>
              <span class="value">{{ formatDate(selectedMember.joinedAt) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Üye Çıkarma Onay Modal -->
    <div v-if="showRemoveConfirm" class="modal" @click="showRemoveConfirm = false">
      <div class="modal-content confirmation-modal" @click.stop>
        <div class="modal-header">
          <h3>Üye Çıkarma Onayı</h3>
        </div>
        <div class="modal-body">
          <p>{{ memberToRemove?.fullName }} isimli üyeyi topluluktan çıkarmak istediğinize emin misiniz?</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showRemoveConfirm = false">İptal</button>
          <button class="btn btn-danger" @click="removeMember">Çıkar</button>
        </div>
      </div>
    </div>

    <!-- Bildirim bileşeni -->
    <div v-if="notification.show" class="notification" :class="`notification-${notification.type}`">
      <div class="notification-content">
        <span>{{ notification.message }}</span>
        <button @click="notification.show = false" class="notification-close">
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>

    <!-- Etkinlik Form Modal -->
    <div v-if="showEventForm" class="modal" @click="closeEventForm">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Yeni Etkinlik Başvurusu</h2>
          <button class="close-btn" @click="closeEventForm">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="submitEventForm">
            <div class="form-group">
              <label>Etkinlik Başlığı</label>
              <input type="text" v-model="newEvent.title" required>
            </div>
            <div class="form-group">
              <label>Açıklama</label>
              <textarea v-model="newEvent.description" rows="4" required></textarea>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Başlangıç Tarihi</label>
                <input type="date" v-model="newEvent.startDate" required>
              </div>
              <div class="form-group">
                <label>Bitiş Tarihi</label>
                <input type="date" v-model="newEvent.endDate" required>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Başlangıç Saati</label>
                <input type="time" v-model="newEvent.startTime" required>
              </div>
              <div class="form-group">
                <label>Bitiş Saati</label>
                <input type="time" v-model="newEvent.endTime" required>
              </div>
            </div>
            <div class="form-group">
              <label>Maksimum Katılımcı</label>
              <input type="number" v-model="newEvent.maxParticipants" required min="1">
            </div>
            <div class="form-group">
              <label>Etkinlik Görseli</label>
              <div class="image-upload-container">
                <div class="image-upload-box" @click="triggerFileInput">
                  <input 
                    type="file" 
                    ref="fileInput" 
                    @change="handleEventImageUpload" 
                    accept="image/*" 
                    style="display: none;"
                  >
                  <div class="upload-icon">
                    <i class="fas fa-cloud-upload-alt"></i>
                  </div>
                  <div class="upload-text">
                    Görsel yüklemek için tıklayın veya sürükleyin
                  </div>
                  <div v-if="newEvent.imagePreview" class="preview-container">
                    <img :src="newEvent.imagePreview" alt="Etkinlik Görseli">
                  </div>
                  <div v-else-if="newEvent.image && typeof newEvent.image === 'string'" class="preview-container">
                    <img :src="newEvent.image" alt="Etkinlik Görseli">
                  </div>
                </div>
              </div>
            </div>
            <div class="form-group">
              <label>Kategoriler</label>
              <div class="category-radio-group">
                <label v-for="category in eventCategories" :key="category" class="category-radio">
                  <input type="radio" :value="category" v-model="newEvent.category">
                  <span class="custom-radio"></span>
                  {{ category }}
                </label>
              </div>
            </div>
            
            <div class="form-group">
              <label>Etkinlik Başvuru Formu <span class="required">*</span></label>
              <div class="file-upload-container">
                <div class="file-upload-box" @click="triggerApplicationFormInput">
                  <input 
                    type="file" 
                    ref="applicationFormInput" 
                    @change="handleApplicationFormUpload" 
                    accept=".pdf" 
                    style="display: none;"
                  >
                  <div class="upload-icon">
                    <i class="fas fa-file-upload"></i>
                  </div>
                  <div class="upload-text">
                    Etkinlik başvuru formunu yüklemek için tıklayın
                  </div>
                  <div v-if="newEvent.applicationFormFile" class="file-info">
                    <i class="fas fa-file-alt"></i>
                    <span>{{ newEvent.applicationFormFile.name }}</span>
                    <button @click.stop="removeApplicationForm" class="remove-file">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="form-group">
              <label>Yönetim Kurulu Formu <span class="required">*</span></label>
              <div class="file-upload-container">
                <div class="file-upload-box" @click="triggerManagementFormInput">
                  <input 
                    type="file" 
                    ref="managementFormInput" 
                    @change="handleManagementFormUpload" 
                    accept=".pdf" 
                    style="display: none;"
                  >
                  <div class="upload-icon">
                    <i class="fas fa-file-upload"></i>
                  </div>
                  <div class="upload-text">
                    Yönetim kurulu formunu yüklemek için tıklayın
                  </div>
                  <div v-if="newEvent.managementBoardFormFile" class="file-info">
                    <i class="fas fa-file-alt"></i>
                    <span>{{ newEvent.managementBoardFormFile.name }}</span>
                    <button @click.stop="removeManagementForm" class="remove-file">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="form-actions">
              <button type="submit" class="submit-btn">Başvuru Gönder</button>
              <button type="button" class="cancel-btn" @click="closeEventForm">İptal</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Duyuru Form Modal -->
    <div v-if="showAnnouncementForm" class="modal" @click="closeAnnouncementForm">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Yeni Duyuru Başvurusu</h2>
          <button class="close-btn" @click="closeAnnouncementForm">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="submitAnnouncementForm">
            <div class="form-group">
              <label>Duyuru Başlığı</label>
              <input type="text" v-model="newAnnouncement.title" required>
            </div>
            <div class="form-group">
              <label>İçerik</label>
              <textarea v-model="newAnnouncement.content" required rows="4"></textarea>
            </div>
            <div class="form-group">
              <label>Duyuru Görseli</label>
              <div class="image-upload-container">
                <div class="image-upload-box" @click="triggerFileInput">
                  <input 
                    type="file" 
                    ref="fileInput" 
                    @change="handleAnnouncementImageUpload" 
                    accept="image/*" 
                    style="display: none;"
                  >
                  <div v-if="!newAnnouncement.imagePreview">
                    <div class="upload-icon">
                      <i class="fas fa-cloud-upload-alt"></i>
                    </div>
                    <div class="upload-text">
                      Görsel yüklemek için tıklayın veya sürükleyin
                    </div>
                  </div>
                  <div v-else class="preview-container">
                    <img :src="newAnnouncement.imagePreview" alt="Duyuru Görseli">
                  </div>
                </div>
              </div>
            </div>
            <div class="form-actions">
              <button type="submit" class="submit-btn">Gönder</button>
              <button type="button" class="cancel-btn" @click="closeAnnouncementForm">İptal</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Sonuç Bildiri Formu Modal -->
    <div v-if="showResultReportModal" class="modal" @click="closeResultReportModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Etkinlik Sonuç Bildiri Formu</h2>
          <button class="close-btn" @click="closeResultReportModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="submitResultReport">
            <div class="form-group">
              <label>Etkinlik Seçin</label>
              <select v-model="selectedCompletedEvent" required>
                <option value="">Etkinlik seçin</option>
                <option v-for="event in completedEvents" :key="event.eventId" :value="event.eventId"
                  :disabled="event.resultReportDocumentId || event.isExpired">
                  {{ event.name }}
                  <span v-if="event.resultReportDocumentId"> (Yüklendi)</span>
                  <span v-else-if="event.isExpired"> (Süre Dolmuş)</span>
                  <span v-else> (Yüklenmemiş)</span>
                </option>
              </select>
            </div>
            
            <div class="form-group">
              <label>Sonuç Bildiri Formu <span class="required">*</span></label>
              <div class="file-upload-container">
                <div class="file-upload-box" @click="triggerResultReportInput">
                  <input 
                    type="file" 
                    ref="resultReportInput" 
                    @change="handleResultReportUpload" 
                    accept=".pdf" 
                    style="display: none;"
                  >
                  <div class="upload-icon">
                    <i class="fas fa-file-upload"></i>
                  </div>
                  <div class="upload-text">
                    Etkinlik sonuç bildirim formunu yüklemek için tıklayın
                  </div>
                  <div v-if="resultReportFile" class="file-info">
                    <i class="fas fa-file-alt"></i>
                    <span>{{ resultReportFile.name }}</span>
                    <button @click.stop="removeResultReport" class="remove-file">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="form-actions">
              <button type="submit" class="submit-btn" :disabled="!selectedCompletedEvent || !resultReportFile">
                Formu Gönder
              </button>
              <button type="button" class="cancel-btn" @click="closeResultReportModal">
                İptal
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Departman Dağılımı Modal -->
    <div v-if="showDepartmentModal" class="modal" @click="closeDepartmentModal">
      <div class="modal-content department-modal" @click.stop>
        <div class="modal-header">
          <h2>Bölüm/Program Dağılımı</h2>
          <button class="close-btn" @click="closeDepartmentModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div v-if="loadingDepartmentData" class="loading-container">
            <div class="spinner"></div>
            <p>Yükleniyor...</p>
          </div>

          <div v-else-if="errorDepartmentData" class="error-container">
            <p>{{ errorDepartmentData }}</p>
            <button @click="loadDepartmentDistribution" class="btn btn-primary">Tekrar Dene</button>
          </div>

          <div v-else-if="parsedDepartmentData && parsedDepartmentData.length > 0" class="distribution-content">
            <table class="department-table">
              <thead>
                <tr>
                  <th>Bölüm</th>
                  <th>Üye Sayısı</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in parsedDepartmentData" :key="item.departmentId">
                  <td>{{ item.departmentName }}</td>
                  <td>{{ item.memberCount }}</td>
                </tr>
              </tbody>
            </table>
            
            <div class="department-summary">
              <h3>Toplam Üye: {{ getTotalMemberCount() }}</h3>
            </div>
          </div>
          
          <div v-else class="no-data">
            <i class="fas fa-chart-pie empty-icon"></i>
            <p>Departman dağılımı verisi bulunamadı.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Etkinlik Detay Modal -->
<div v-if="showEventDetailModal" class="modal">
  <div class="modal-content event-detail-modal">
    <div class="modal-header">
      <h2>
        <i class="fas fa-calendar-alt"></i>
        {{ selectedEvent.name || 'Etkinlik Detayı' }}
      </h2>
      <button @click="closeEventDetailModal" class="close-btn">
        <i class="fas fa-times"></i>
      </button>
    </div>
    <div class="modal-body">
      <div class="event-detail-grid">
        <div>
          <strong><i class="fas fa-align-left"></i> Açıklama:</strong>
          <div class="event-desc">{{ selectedEvent.description }}</div>
        </div>
        <div>
          <strong><i class="fas fa-calendar"></i> Başlangıç:</strong>
          <span>{{ formatDate(selectedEvent.startDate) }}</span>
        </div>
        <div>
          <strong><i class="fas fa-calendar-check"></i> Bitiş:</strong>
          <span>{{ formatDate(selectedEvent.endDate) }}</span>
        </div>
        <div>
          <strong><i class="fas fa-users"></i> Katılımcı:</strong>
          <span>{{ selectedEvent.participantCount }}/{{ selectedEvent.maxParticipants }}</span>
        </div>
        <div>
          <strong><i class="fas fa-info-circle"></i> Durum:</strong>
          <span>{{ getStatusText(selectedEvent.status) }}</span>
        </div>

        <!-- Rezervasyon Listesi -->
        <div>
          <strong><i class="fas fa-clock"></i> Rezervasyonlar:</strong>
          <div v-if="reservations.length === 0">
            Rezervasyon yapılmamış
          </div>
          <ul v-else>
            <li v-for="res in reservations" :key="res.id">
              <strong>{{ res.locationName }}</strong> — {{ res.slot }}
            </li>
          </ul>
        </div>
      </div>

      <div class="event-documents">
        <h3><i class="fas fa-file-alt"></i> Belgeler</h3>
        <ul>
          <li>
            <strong>Başvuru Formu:</strong>
            <template v-if="selectedEvent.applicationFormDocumentId">
              <button @click="downloadDocument(selectedEvent.applicationFormDocumentId, 'application')" class="doc-btn">İndir</button>
              <button @click="viewDocumentWithAuth(selectedEvent.applicationFormDocumentId)" class="doc-btn">Görüntüle</button>
            </template>
            <span v-else class="doc-missing">Yüklenmemiş</span>
          </li>
          <li>
            <strong>Yönetim Kurulu Formu:</strong>
            <template v-if="selectedEvent.managementBoardFormDocumentId">
              <button @click="downloadDocument(selectedEvent.managementBoardFormDocumentId, 'management')" class="doc-btn">İndir</button>
              <button @click="viewDocumentWithAuth(selectedEvent.managementBoardFormDocumentId)" class="doc-btn">Görüntüle</button>
            </template>
            <span v-else class="doc-missing">Yüklenmemiş</span>
          </li>
          <li>
            <strong>Sonuç Bildiri Formu:</strong>
            <template v-if="selectedEvent.resultReportDocumentId">
              <button @click="downloadDocument(selectedEvent.resultReportDocumentId, 'result')" class="doc-btn">İndir</button>
              <button @click="viewDocumentWithAuth(selectedEvent.resultReportDocumentId)" class="doc-btn">Görüntüle</button>
            </template>
            <span v-else class="doc-missing">Yüklenmemiş</span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>


    
  </div>
</template>

<script>
import { ref, reactive, onMounted, computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/services/api';
import studentService from '@/services/studentService';
import userService from '@/services/userService';
import eventService from '@/services/eventService';
import ReservationModal from '@/components/reservation/ReservationModal.vue'


export default {
  name: 'CommunityManagement',
  components: {
  ReservationModal
},
  setup() {
    const route = useRoute();
    const clubId = route.params.id;
    const clubDetails = ref(null);
    const applications = ref([]);
    const members = ref([]);
    const loading = ref(true);
    const error = ref(null);
    const processing = reactive({});
    const selectedMember = ref(null);
    const memberDetails = ref(null);
    const showRemoveConfirm = ref(false);
    const memberToRemove = ref(null);
    const notification = reactive({
      show: false,
      type: 'info',
      message: '',
      timeout: null
    });
    const departments = ref([
      { id: 1, name: 'Bilgisayar Mühendisliği' },
      { id: 2, name: 'Elektrik-Elektronik Mühendisliği' },
      { id: 3, name: 'Makine Mühendisliği' },
      { id: 4, name: 'Endüstri Mühendisliği' },
      { id: 5, name: 'İnşaat Mühendisliği' }
    ]);
    const memberSearchQuery = ref('');
    const departmentFilter = ref('');
    const dateFilter = ref('');
    const isEditing = ref(false);
    const editedClub = reactive({
      name: '',
      description: '',
      imageUrl: ''
    });
    const imageInput = ref(null);
    const newImage = ref(null);
    const activeTab = ref('applications');
    const events = ref([]);
    const announcements = ref([]);
    const showEventForm = ref(false);
    const newEvent = ref({
      title: '',
      description: '',
      startDate: '',
      endDate: '',
      startTime: '',
      endTime: '',
      maxParticipants: 1,
      image: null,
      imagePreview: null,
      category: '', // Tekli seçim için
      applicationFormFile: null,
      managementBoardFormFile: null
    });
    const expenses = ref([]);
    const showExpenseForm = ref(false);
    const showReservationModal = ref(false);
    
    // Departman dağılımı için gerekli değişkenler
    const showDepartmentModal = ref(false);
    const departmentDistribution = ref([]);
    const loadingDepartmentData = ref(false);
    const errorDepartmentData = ref(null);

    const showAnnouncementForm = ref(false);
    const newAnnouncement = ref({
      title: '',
      content: '',
      imagePreview: null,
      imageFile: null
    });
    const isAdvisor = ref(false);

    // Add these refs after other refs at the beginning of the script section
    const applicationFormInput = ref(null);
    const managementFormInput = ref(null);

    // Add these refs and state variables
    const resultReportInput = ref(null);
    const resultReportFile = ref(null);
    const selectedCompletedEvent = ref('');
    const showResultReportModal = ref(false);
    const completedEvents = ref([]);

    // 1. State ekle
    const selectedEvent = ref(null);
    const showEventDetailModal = ref(false);
    const reservations = ref([]);


    const showNotification = (type, message, duration = 3000) => {
      if (notification.timeout) {
        clearTimeout(notification.timeout);
      }
      notification.type = type;
      notification.message = message;
      notification.show = true;
      notification.timeout = setTimeout(() => {
        notification.show = false;
      }, duration);
    };

    const formatDate = (dateString) => {
      if (!dateString) return '';
      return new Date(dateString).toLocaleDateString('tr-TR');
    };

    const showMemberDetails = async (member) => {
      selectedMember.value = member;
      try {
        const response = await studentService.getMemberDetails(member.userId);
        memberDetails.value = response;
      } catch (error) {
        showNotification('error', 'Üye detayları alınamadı: ' + error.message);
      }
    };

    const closeModal = () => {
      selectedMember.value = null;
      memberDetails.value = null;
      showRemoveConfirm.value = false;
      memberToRemove.value = null;
    };

    const confirmRemoveMember = (member) => {
      memberToRemove.value = member;
      showRemoveConfirm.value = true;
    };

    const removeMember = async () => {
      if (!memberToRemove.value) return;
      
      try {
        await studentService.removeClubMember(memberToRemove.value.membershipId);
        showNotification('success', 'Üye başarıyla çıkarıldı');
        members.value = members.value.filter(m => m.membershipId !== memberToRemove.value.membershipId);
      } catch (error) {
        showNotification('error', 'Üye çıkarma işlemi başarısız: ' + error.message);
      } finally {
        showRemoveConfirm.value = false;
        memberToRemove.value = null;
      }
    };
      const loadPendingEvents = async () => {
  try {
    const response = await api.get(`/events/pending?clubId=${clubId}`);
    if (response.data) {
      events.value = response.data.map(event => ({
        id: event.id,
        title: event.name,
        description: event.description,
        startDate: event.startDate.split('T')[0],
        endDate: event.endDate.split('T')[0],
        startTime: event.startDate.split('T')[1].substring(0, 5),
        endTime: event.endDate.split('T')[1].substring(0, 5),
        maxParticipants: event.maxParticipants,
        currentParticipants: event.participantCount,
        status: event.status,
        image: event.image ? `data:image/jpeg;base64,${event.image}` : null
      }));
    }
  } catch (error) {
    console.error('Bekleyen etkinlikler yüklenirken hata:', error);
    showNotification('error', 'Bekleyen etkinlikler yüklenirken bir hata oluştu.');
  }
};

    const loadData = async () => {
      loading.value = true;
      error.value = null;
      try {
        console.log('Topluluk ID:', clubId);
        
        // Önce kullanıcının üyeliklerini kontrol et
        const membershipsResponse = await studentService.getMyMemberships();
        console.log('Üyelik bilgileri:', membershipsResponse);
        
        // Kullanıcının bu topluluğun lideri olduğunu doğrula
        const isLeader = membershipsResponse.data.some(m => 
          parseInt(m.clubId) === parseInt(clubId) && 
          (m.role || '').toLowerCase() === 'başkan'
        );

        // Kullanıcı rolünü localStorage'dan al
        const userStr = localStorage.getItem('user');
        const user = userStr ? JSON.parse(userStr) : null;
        const isAdmin = user && user.userType === 'admin';

        if (!isLeader && !isAdmin) {
          throw new Error('Bu topluluğu yönetme yetkiniz bulunmamaktadır.');
        }

         // Topluluk detaylarını API'den getir
        const response = await api.get(`/clubs/${clubId}`);
        
        if (response.data && response.data.club) {
          clubDetails.value = {
            name: response.data.club.name,
            description: response.data.club.description,
            imageUrl: response.data.club.image ? `data:image/jpeg;base64,${response.data.club.image}` : '/default-club.png'
         };        
          } else {
          throw new Error('Topluluk detayları alınamadı');
        }

        // Bekleyen başvuruları getir
        const applicationsResponse = await studentService.getPendingApplicationsForMyLedClubs();
        if (Array.isArray(applicationsResponse)) {
          applications.value = await Promise.all(applicationsResponse
            .filter(app => parseInt(app.clubId) === parseInt(clubId))
            .map(async app => {
              try {
                const details = await studentService.getMemberDetails(app.userId);
                return {
                  ...app,
                  membershipId: app.membershipId || app.MembershipId || app.membershipID || app.id,
                  departmentId: details.departmentId,
                  name: details.name,
                  surname: details.surname,
                  schoolNumber: details.schoolNumber
                };
              } catch (error) {
                console.error('Üye detayları alınamadı:', error);
                return app;
              }
            }));
        } else {
          applications.value = [];
        }

        // Topluluk üyelerini getir
        const membersResponse = await studentService.getMembersForMyLedClubs(clubId);
        if (Array.isArray(membersResponse)) {
          members.value = await Promise.all(membersResponse.map(async member => {
            try {
              const details = await studentService.getMemberDetails(member.userId);
              return {
                ...member,
                departmentId: details.departmentId,
                name: details.name,
                surname: details.surname,
                schoolNumber: details.schoolNumber,
                email: details.email,
                phoneNumber: details.phoneNumber
              };
            } catch (error) {
              console.error('Üye detayları alınamadı:', error);
              return member;
            }
          }));
        } else {
          members.value = [];
        }

        // Yeni veri yükleme işlemleri
        await Promise.all([
          loadEvents(),
          loadAnnouncements()
        ]);
        const loadExpenses = async () => {
        try {
    const response = await api.get(`/club-expenses/club/${clubId}`);
    expenses.value = (response.data || []).map(exp => ({
    ...exp,
   dokumanUrl: exp.dokumanUrl || exp.dokuman || exp.Dokuman || null
    }));
    console.log('Yüklenen giderler:', expenses.value);

  } catch (error) {
    console.error('Giderler yüklenirken hata:', error);
    showNotification('error', 'Giderler yüklenirken bir hata oluştu');
  }
};

      } catch (err) {
        console.error('Veri yüklenirken hata:', err);
        error.value = err.response?.data?.message || err.message || 'Veriler yüklenirken bir hata oluştu.';
        showNotification('error', error.value);
      } finally {
        loading.value = false;
      }
    };

    const approveApplication = async (membershipId) => {
      if (!membershipId) {
        console.error('membershipId is undefined:', membershipId);
        showNotification('error', 'Üyelik ID bilgisi eksik');
        return;
      }

      processing[membershipId] = true;
      try {
        console.log('Onaylanacak başvuru ID:', membershipId);
        const response = await studentService.approveMembership(membershipId);
        console.log('Onaylama yanıtı:', response);
        await loadData(); // Verileri yenile
        showNotification('success', 'Başvuru başarıyla onaylandı.');
      } catch (err) {
        console.error('Onaylama hatası:', err);
        showNotification('error', 'Başvuru onaylanırken bir hata oluştu.');
      } finally {
        processing[membershipId] = false;
      }
    };

    const rejectApplication = async (membershipId) => {
      if (!membershipId) {
        console.error('membershipId is undefined:', membershipId);
        showNotification('error', 'Üyelik ID bilgisi eksik');
        return;
      }

      processing[membershipId] = true;
      try {
        console.log('Reddedilecek başvuru ID:', membershipId);
        const response = await studentService.rejectMembership(membershipId);
        console.log('Reddetme yanıtı:', response);
        await loadData(); // Verileri yenile
        showNotification('success', 'Başvuru başarıyla reddedildi.');
      } catch (err) {
        console.error('Reddetme hatası:', err);
        showNotification('error', 'Başvuru reddedilirken bir hata oluştu.');
      } finally {
        processing[membershipId] = false;
      }
    };

    const getDepartmentName = (departmentId) => {
      const department = departments.value.find(d => d.id === departmentId);
      return department ? department.name : 'Belirsiz';
    };

    const getCurrentDate = () => {
      const today = new Date();
      return today.toISOString().split('T')[0];
    };
    const newExpense = ref({
  date: getCurrentDate(),
  cashSupport: 0,
  inKindSupport: 0,
  description: '',
  documentFile: null,
  documentName: ''
});
    const filteredMembers = computed(() => {
      return members.value.filter(member => {
        const matchesSearch = !memberSearchQuery.value || 
          member.name?.toLowerCase().includes(memberSearchQuery.value.toLowerCase()) ||
          member.surname?.toLowerCase().includes(memberSearchQuery.value.toLowerCase()) ||
          member.schoolNumber?.toLowerCase().includes(memberSearchQuery.value.toLowerCase());

        const matchesDepartment = !departmentFilter.value || 
          member.departmentId === departmentFilter.value;

        const matchesDate = !dateFilter.value || 
          new Date(member.joinedAt).toISOString().split('T')[0] === dateFilter.value;

        return matchesSearch && matchesDepartment && matchesDate;
      });
    });

    const startEditing = () => {
      editedClub.name = clubDetails.value.name;
      editedClub.description = clubDetails.value.description;
      isEditing.value = true;
    };

    const cancelEditing = () => {
      isEditing.value = false;
    };

    const startImageEdit = () => {
      isEditing.value = true;
    };

    const cancelImageEdit = () => {
      isEditing.value = false;
      newImage.value = null;
    };

    const handleImageChange = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5MB limit
          showNotification('error', 'Resim boyutu 5MB\'dan küçük olmalıdır');
          return;
        }
        
        if (!['image/jpeg', 'image/png'].includes(file.type)) {
          showNotification('error', 'Sadece JPEG ve PNG formatları desteklenmektedir');
          return;
        }
        
        newImage.value = file;
        const reader = new FileReader();
        reader.onload = (e) => {
          editedClub.imageUrl = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    };

    const saveChanges = async () => {
      try {
        const formData = new FormData();
        formData.append('name', editedClub.name);
        formData.append('description', editedClub.description);
        
        if (newImage.value) {
          formData.append('image', newImage.value);
        }

        const response = await api.put(`/clubs/${clubId}/guncelle`, formData);
        
        if (response.data) {
          clubDetails.value = {
            ...clubDetails.value,
            name: editedClub.name,
            description: editedClub.description
          };
          
          if (response.data.imageUrl) {
            clubDetails.value.imageUrl = response.data.imageUrl;
          }
          
          showNotification('success', 'Topluluk bilgileri başarıyla güncellendi');
          isEditing.value = false;
          newImage.value = null;
        }
      } catch (error) {
        console.error('Topluluk güncellenirken hata:', error);
        showNotification('error', 'Topluluk güncellenirken bir hata oluştu');
      }
    };

    const getEventStatusText = (status) => {
      const statusMap = {
        pending: 'Onay Bekliyor',
        approved: 'Onaylandı',
        rejected: 'Reddedildi'
      };
      return statusMap[status] || status;
    };

    const getEventStatusClass = (status) => {
      return {
        'status-badge': true,
        'pending': status === 'pending',
        'approved': status === 'approved',
        'rejected': status === 'rejected'
      };
    };

    const getAnnouncementStatusText = (status) => {
      const statusMap = {
        'pending': 'Onay Bekliyor',
        'approved': 'Onaylandı',
        'rejected': 'Reddedildi'
      };
      return statusMap[status?.toLowerCase()] || status;
    };

    const loadEvents = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }

        const response = await api.get(`/events/listele?clubId=${clubId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (response.data) {
          events.value = response.data.map(event => {
            const startDate = new Date(event.startDate || event.StartDate);
            const endDate = new Date(event.endDate || event.EndDate);
            return {
              id: event.id || event.eventId || event.EventId,
              title: event.name || event.Name,
              description: event.description || event.Description,
              startDate: startDate.toISOString().split('T')[0],
              endDate: endDate.toISOString().split('T')[0],
              startTime: startDate.toTimeString().slice(0, 5),
              endTime: endDate.toTimeString().slice(0, 5),
              maxParticipants: event.maxParticipants || event.MaxParticipants || 0,
              currentParticipants: event.participantCount || event.ParticipantCount || 0,
              status: event.status || event.Status || 'pending',
              image: event.image ? `data:image/jpeg;base64,${event.image}` : null,
              applicationFormDocumentId: event.applicationFormDocumentId || event.ApplicationFormDocumentId || null,
              managementBoardFormDocumentId: event.managementBoardFormDocumentId || event.ManagementBoardFormDocumentId || null,
              resultReportDocumentId: event.resultReportDocumentId || event.ResultReportDocumentId || null
            };
          });
        }
      } catch (error) {
        console.error('Etkinlikler yüklenirken hata:', error);
        if (error.response?.status === 403) {
          showNotification('error', 'Etkinlikleri görüntüleme yetkiniz bulunmamaktadır.');
        } else {
          const errorMessage = error.response?.data?.message || 
                             error.response?.data?.error || 
                             'Etkinlikler yüklenirken bir hata oluştu';
          showNotification('error', errorMessage);
        }
      }
    };

    const loadAnnouncements = async () => {
      try {
        console.log('Duyurular yüklenmeye başlıyor...');
        console.log('Club ID:', clubId);

        const response = await api.get(`/announcements/club/${clubId}?isActive=true`);
        
        console.log('API Yanıtı:', response);

        if (response.data) {
          announcements.value = response.data;
          console.log('Yüklenen duyurular:', announcements.value);
        }
      } catch (error) {
        console.error('Duyurular yüklenirken hata:', error);
        const errorMessage = error.response?.data?.message || 
                           error.response?.data?.error || 
                           'Duyurular yüklenirken bir hata oluştu';
        showNotification('error', errorMessage);
      }
    };


    const loadExpenses = async () => {
  try {
    const response = await api.get(`/club-expenses/club/${clubId}`);
    expenses.value = response.data || [];
  } catch (error) {
    console.error('Giderler yüklenirken hata:', error);
    showNotification('error', 'Giderler yüklenirken bir hata oluştu');
  }
};


    const handleEventImageUpload = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5MB limit
          showNotification('error', 'Resim boyutu 5MB\'dan küçük olmalıdır');
          return;
        }
        
        if (!['image/jpeg', 'image/png'].includes(file.type)) {
          showNotification('error', 'Sadece JPEG ve PNG formatları desteklenmektedir');
          return;
        }
        
        // Dosya bilgisini sakla
        newEvent.value.image = file;
        
        // Önizleme için URL oluştur
        const reader = new FileReader();
        reader.onload = e => {
          newEvent.value.imagePreview = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    };

    const submitEventForm = async () => {
      try {
        const formData = new FormData();
        formData.append('Name', newEvent.value.title);
        formData.append('Description', newEvent.value.description);
        formData.append('ClubId', clubId);
        
        // Tarih ve saati birleştir
        const startDateTimeStr = `${newEvent.value.startDate}T${newEvent.value.startTime}:00`;
        const endDateTimeStr = `${newEvent.value.endDate}T${newEvent.value.endTime}:00`;
        
        formData.append('StartDate', startDateTimeStr);
        formData.append('EndDate', endDateTimeStr);
        formData.append('MaxParticipants', String(newEvent.value.maxParticipants));
        formData.append('EventType', 'physical');
        // Kategori bilgisini ekle
        formData.append('Category', newEvent.value.category || '');

        // Resim dosyasını ekle
        if (newEvent.value.image instanceof File) {
          console.log('Resim dosyası yükleniyor:', newEvent.value.image.name);
          console.log('Resim dosyası boyutu:', newEvent.value.image.size);
          console.log('Resim dosyası tipi:', newEvent.value.image.type);
          formData.append('ImageFile', newEvent.value.image);
        }
        
        // Başvuru formunu ekle
        if (newEvent.value.applicationFormFile instanceof File) {
          console.log('Başvuru formu yükleniyor:', newEvent.value.applicationFormFile.name);
          formData.append('ApplicationFormFile', newEvent.value.applicationFormFile);
        } else {
          showNotification('error', 'Etkinlik başvuru formu gereklidir');
          return;
        }
        
        // Yönetim kurulu formunu ekle
        if (newEvent.value.managementBoardFormFile instanceof File) {
          console.log('Yönetim kurulu formu yükleniyor:', newEvent.value.managementBoardFormFile.name);
          formData.append('ManagementBoardFormFile', newEvent.value.managementBoardFormFile);
        } else {
          showNotification('error', 'Yönetim kurulu formu gereklidir');
          return;
        }

        // FormData içeriğini kontrol et
        console.log('FormData içeriği:');
        for (let pair of formData.entries()) {
          console.log(pair[0] + ': ' + (pair[1] instanceof File ? pair[1].name : pair[1]));
        }

        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }

        const result = await api.post('/events/ekle', formData, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Accept': 'application/json',
            'Content-Type': 'multipart/form-data'
          }
        });
        
        if (result.data) {
          showNotification('success', 'Etkinlik başvurusu başarıyla gönderildi. Danışman onayı bekleniyor.');
          closeEventForm();
          await loadEvents();
        }
      } catch (error) {
        console.error('Etkinlik oluşturma hatası:', error);
        console.error('Hata detayları:', error.response?.data);
        if (error.response?.status === 403) {
          // Kullanıcı rolünü kontrol et
          const userStr = localStorage.getItem('user');
          const user = userStr ? JSON.parse(userStr) : null;
          const isAdmin = user && user.userType === 'admin';
          
          if (!isAdmin) {
            showNotification('error', 'Bu işlem için yetkiniz bulunmamaktadır. Topluluk başkanı olduğunuzdan emin olun.');
          } else {
            // Admin için etkinlik oluşturmaya izin ver
            const errorMessage = error.response?.data?.message || 
                              error.response?.data?.error || 
                              error.message || 
                              'Etkinlik oluşturulurken bir hata oluştu';
            showNotification('error', errorMessage);
          }
        } else {
          const errorMessage = error.response?.data?.message || 
                             error.response?.data?.error || 
                             error.message || 
                             'Etkinlik oluşturulurken bir hata oluştu';
          showNotification('error', errorMessage);
        }
      }
    };
    const submitExpenseForm = async () => {
  try {
    const formData = new FormData();
    formData.append('clubId', clubId); // Ensure `clubId` is correct
    formData.append('date', newExpense.value.date);
    formData.append('cashSupport', newExpense.value.cashSupport.toString());
    formData.append('inKindSupport', newExpense.value.inKindSupport.toString());
    formData.append('description', newExpense.value.description);

    if (newExpense.value.documentFile) {
      formData.append('Dokuman', newExpense.value.documentFile);
    }

    const response = await api.post('/club-expenses', formData);

    if (response.data) {
      showNotification('success', 'Maliyet formu başarıyla kaydedildi');
      await loadExpenses();
      closeExpenseForm();
    }
  } catch (error) {
    console.error('Maliyet formu gönderilirken hata:', error);
    const msg = error.response?.data?.message || error.message || 'Form gönderilirken hata oluştu';
    showNotification('error', msg);
  }
};


    const approveEvent = async (eventId) => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }

        const result = await api.put(`/events/${eventId}/approve`, null, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (result.data) {
          showNotification('success', 'Etkinlik başarıyla onaylandı');
          await loadEvents();
        } else {
          throw new Error('Etkinlik onaylanırken bir hata oluştu');
        }
      } catch (error) {
        console.error('Etkinlik onaylanırken hata:', error);
        if (error.response?.status === 403) {
          // Kullanıcı rolünü kontrol et
          const userStr = localStorage.getItem('user');
          const user = userStr ? JSON.parse(userStr) : null;
          const isAdmin = user && user.userType === 'admin';
          
          if (!isAdmin) {
            showNotification('error', 'Etkinlik onaylama yetkiniz bulunmamaktadır.');
          } else {
            // Admin için etkinlik onaylamaya izin ver
            const errorMessage = error.response?.data?.message || 
                              error.response?.data?.error || 
                              'Etkinlik onaylanırken bir hata oluştu';
            showNotification('error', errorMessage);
          }
        } else {
          const errorMessage = error.response?.data?.message || 
                             error.response?.data?.error || 
                             'Etkinlik onaylanırken bir hata oluştu';
          showNotification('error', errorMessage);
        }
      }
    };

    const rejectEvent = async (eventId) => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }

        const result = await api.put(`/events/${eventId}/reject`, null, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (result.data) {
          showNotification('success', 'Etkinlik başarıyla reddedildi');
          await loadEvents();
        } else {
          throw new Error('Etkinlik reddedilirken bir hata oluştu');
        }
      } catch (error) {
        console.error('Etkinlik reddedilirken hata:', error);
        if (error.response?.status === 403) {
          // Kullanıcı rolünü kontrol et
          const userStr = localStorage.getItem('user');
          const user = userStr ? JSON.parse(userStr) : null;
          const isAdmin = user && user.userType === 'admin';
          
          if (!isAdmin) {
            showNotification('error', 'Etkinlik reddetme yetkiniz bulunmamaktadır.');
          } else {
            // Admin için etkinlik reddetmeye izin ver
            const errorMessage = error.response?.data?.message || 
                              error.response?.data?.error || 
                              'Etkinlik reddedilirken bir hata oluştu';
            showNotification('error', errorMessage);
          }
        } else {
          const errorMessage = error.response?.data?.message || 
                             error.response?.data?.error || 
                             'Etkinlik reddedilirken bir hata oluştu';
          showNotification('error', errorMessage);
        }
      }
    };

    const submitAnnouncementForm = async () => {
      try {
        const formData = {
          title: newAnnouncement.value.title,
          content: newAnnouncement.value.content,
          clubId: clubId,
          imageUrl: newAnnouncement.value.imagePreview
        };

        const result = await api.post('/announcements', formData);

        if (result.data) {
          showNotification('success', 'Duyuru başarıyla gönderildi');
          closeAnnouncementForm();
          await loadAnnouncements();
        }
      } catch (error) {
        console.error('Duyuru gönderilirken hata:', error);
        const errorMessage = error.response?.data?.message || 
                           error.response?.data?.error || 
                           'Duyuru gönderilirken bir hata oluştu';
        showNotification('error', errorMessage);
      }
    };

    const approveAnnouncement = async (announcementId) => {
      try {
        await api.put(`/announcements/${announcementId}/approve`);
        showNotification('success', 'Duyuru başarıyla onaylandı');
        await loadAnnouncements();
      } catch (error) {
        console.error('Duyuru onaylanırken hata:', error);
        showNotification('error', 'Duyuru onaylanırken bir hata oluştu');
      }
    };

    const rejectAnnouncement = async (announcementId) => {
      try {
        await api.put(`/announcements/${announcementId}/reject`);
        showNotification('success', 'Duyuru başarıyla reddedildi');
        await loadAnnouncements();
      } catch (error) {
        console.error('Duyuru reddedilirken hata:', error);
        showNotification('error', 'Duyuru reddedilirken bir hata oluştu');
      }
    };

    const closeEventForm = () => {
      showEventForm.value = false;
      newEvent.value = {
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        startTime: '',
        endTime: '',
        maxParticipants: 1,
        image: null,
        imagePreview: null,
        category: '',
        applicationFormFile: null,
        managementBoardFormFile: null
      };
      if (applicationFormInput.value) applicationFormInput.value.value = '';
      if (managementFormInput.value) managementFormInput.value.value = '';
    };

    const closeExpenseForm = () => {
  showExpenseForm.value = false;
  newExpense.value = {
    date: getCurrentDate(),
    cashSupport: 0,
    inKindSupport: 0,
    description: '',
    documentFile: null,
    documentName: ''
  };
};

    const closeAnnouncementForm = () => {
      showAnnouncementForm.value = false;
      newAnnouncement.value = {
        title: '',
        content: '',
        imagePreview: null,
        imageFile: null
      };
    };

    const triggerFileInput = () => {
      const fileInput = document.querySelector('input[type="file"]');
      if (fileInput) {
        fileInput.click();
      }
    };

    const removeImage = () => {
      editedClub.imageUrl = null;
      newImage.value = null;
      if (imageInput.value) {
        imageInput.value.value = '';
      }
    };

    const handleAnnouncementImageUpload = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5MB limit
          showNotification('error', 'Resim boyutu 5MB\'dan küçük olmalıdır');
          return;
        }
        
        if (!['image/jpeg', 'image/png'].includes(file.type)) {
          showNotification('error', 'Sadece JPEG ve PNG formatları desteklenmektedir');
          return;
        }
        
        const reader = new FileReader();
        reader.onload = e => {
          newAnnouncement.value.imagePreview = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    };

    const getImageUrl = (imageUrl) => {
      if (!imageUrl) return '/default-announcement.png';
      if (imageUrl.startsWith('http')) return imageUrl;
      return `https://localhost:7274${imageUrl}`;
    };

    const handleImageError = (event) => {
      event.target.src = '/default-announcement.png';
    };

    onMounted(async () => {
      await loadData();
      await loadExpenses();

      // Eğer danışmansa, yetkili etkinlikleri yükle
      if (isAdvisor) {
        await loadPendingEvents();
      } else {
        await loadEvents();
      }
    });

    watch(activeTab, async (newTab) => {
  if (newTab === 'announcements') {
    await loadAnnouncements();
  } else if (newTab === 'events') {
    await loadEvents();
  } else if (newTab === 'expenses') {
    await loadExpenses(); // ✅ BURASI
  }
});

    // Departman dağılımı verilerini getirme fonksiyonu
    const loadDepartmentDistribution = async () => {
      try {
        loadingDepartmentData.value = true;
        errorDepartmentData.value = null;
        const result = await userService.getDepartmentDistribution(clubId);
        
        console.log("Departman dağılımı API yanıtı:", result);
        
        // API yanıtını analiz et ve bunu departmentDistribution'a ata
        if (result) {
          departmentDistribution.value = result;
        } else {
          departmentDistribution.value = [];
          errorDepartmentData.value = "Veri bulunamadı";
        }
        
        loadingDepartmentData.value = false;
      } catch (error) {
        console.error('Departman dağılımı verileri alınırken hata:', error);
        errorDepartmentData.value = error.response?.data?.message || error.message || 'Departman dağılımı verileri alınamadı';
        showNotification('error', 'Departman dağılımı verileri alınamadı');
        loadingDepartmentData.value = false;
      }
    };

    // Departman dağılımı modalını göster
    const showDepartmentDistributionModal = async () => {
      showDepartmentModal.value = true;
      await loadDepartmentDistribution();
    };

    // Departman dağılımı modalını kapat
    const closeDepartmentModal = () => {
      showDepartmentModal.value = false;
    };

    // Departman bazlı üye yüzdesini hesapla
    const getPercentage = (count) => {
      const total = getTotalMemberCount();
      if (total === 0) return 0;
      return Math.round((count / total) * 100);
    };

    // Toplam üye sayısını hesapla
    const getTotalMemberCount = () => {
      if (!departmentDistribution.value) return 0;
      
      // API yanıtının formatına göre toplam sayıyı hesapla
      if (Array.isArray(departmentDistribution.value)) {
        return departmentDistribution.value.reduce((sum, item) => sum + (item.memberCount || 0), 0);
      } else {
        return Object.values(departmentDistribution.value).reduce((sum, count) => sum + count, 0);
      }
    };

    // Departman ID'sine göre rastgele renk üret
    const getRandomColor = (id, opacity = 1) => {
      const colors = [
        `rgba(59, 130, 246, ${opacity})`,  // Mavi
        `rgba(16, 185, 129, ${opacity})`,  // Yeşil
        `rgba(239, 68, 68, ${opacity})`,   // Kırmızı
        `rgba(245, 158, 11, ${opacity})`,  // Turuncu
        `rgba(139, 92, 246, ${opacity})`,  // Mor
        `rgba(236, 72, 153, ${opacity})`,  // Pembe
        `rgba(75, 85, 99, ${opacity})`,    // Gri
        `rgba(14, 165, 233, ${opacity})`,  // Açık Mavi
        `rgba(168, 85, 247, ${opacity})`,  // Eflatun
        `rgba(249, 115, 22, ${opacity})`   // Koyu Turuncu
      ];
      
      // ID'nin 1'den başladığını varsayıyoruz
      const colorIndex = (id - 1) % colors.length;
      return colors[Math.max(0, colorIndex)];
    };

    // API yanıtını işle ve UI için uygun formata çevir
    const parsedDepartmentData = computed(() => {
      if (!departmentDistribution.value) return [];
      
      // API yanıtı bir dizi ise doğrudan kullan
      if (Array.isArray(departmentDistribution.value)) {
        return departmentDistribution.value.map(item => ({
          departmentId: item.departmentId || item.clubId || 0,
          departmentName: item.departmentName || 'Belirsiz',
          memberCount: item.memberCount || 0
        }));
      }
      
      // API yanıtı bir obje ise (bölüm ID'si: üye sayısı şeklinde)
      return Object.entries(departmentDistribution.value).map(([key, value]) => {
        const departmentId = parseInt(key);
        return {
          departmentId,
          departmentName: getDepartmentName(departmentId) || 'Belirsiz',
          memberCount: value
        };
      });
    });

    const triggerDocumentInput = () => {
      const documentInput = document.querySelector('input[type="file"]');
      if (documentInput) {
        documentInput.click();
      }
    };

  const handleDocumentUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    newExpense.value.documentFile = file;
    newExpense.value.documentName = file.name;
  }
};

    const removeDocument = () => {
      newExpense.value.documentFile = null;
      newExpense.value.documentName = '';
      if (document.querySelector('input[type="file"]')) {
        document.querySelector('input[type="file"]').value = '';
      }
    };

    const viewDocument = (url) => {
      if (url) {
        window.open(url, '_blank'); // Belgeyi yeni sekmede açar
      } else {
        showNotification('error', 'Bu gider için belge bulunamadı.');
      }
    };

    const deleteExpense = async (id) => {
      try {
        const confirmed = confirm('Bu gideri silmek istediğinize emin misiniz?');
        if (!confirmed) return;

        await api.delete(`/club-expenses/${id}`);
        showNotification('success', 'Gider başarıyla silindi.');
        await loadExpenses(); // Gider listesini yenile
      } catch (error) {
        console.error('Gider silinirken hata:', error);
        showNotification('error', 'Gider silinirken bir hata oluştu.');
      }
    };

    const editExpense = (expense) => {
      // Düzenleme modalını açmak için gerekli işlemler
      showExpenseForm.value = true;
      newExpense.value = {
        date: expense.date,
        cashSupport: expense.cashSupport,
        inKindSupport: expense.inKindSupport,
        description: expense.description,
        documentFile: null,
        documentName: '',
      };
    };

    const eventCategories = [
      'Bilim',
      'Sanat',
      'Spor',
      'Teknoloji',
      'Toplantı',
      'Kongre',
      'Seminer',
      'Sempozyum',
      'Eğitim',
      'Kültür',
      'Sosyal Etkinlik',
      'Diğer'
    ];

    // Add these functions after the existing triggerFileInput function
    const triggerApplicationFormInput = () => {
      if (applicationFormInput.value) {
        applicationFormInput.value.click();
      }
    };

    const triggerManagementFormInput = () => {
      if (managementFormInput.value) {
        managementFormInput.value.click();
      }
    };

    const handleApplicationFormUpload = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 10 * 1024 * 1024) { // 10MB limit
          showNotification('error', 'Dosya boyutu 10MB\'yi aşamaz');
          return;
        }
        if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
          showNotification('error', 'Sadece PDF dosyası yükleyebilirsiniz!');
          return;
        }
        newEvent.value.applicationFormFile = file;
      }
    };

    const handleManagementFormUpload = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 10 * 1024 * 1024) { // 10MB limit
          showNotification('error', 'Dosya boyutu 10MB\'yi aşamaz');
          return;
        }
        if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
          showNotification('error', 'Sadece PDF dosyası yükleyebilirsiniz!');
          return;
        }
        newEvent.value.managementBoardFormFile = file;
      }
    };

    const removeApplicationForm = () => {
      newEvent.value.applicationFormFile = null;
      if (applicationFormInput.value) {
        applicationFormInput.value.value = '';
      }
    };

    const removeManagementForm = () => {
      newEvent.value.managementBoardFormFile = null;
      if (managementFormInput.value) {
        managementFormInput.value.value = '';
      }
    };

    // Add this function to load completed events
    const loadCompletedEvents = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }
        const response = await api.get(`/events/completed/${clubId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        if (response.data) {
          completedEvents.value = response.data.map(event => {
            const endDate = new Date(event.endDate || event.EndDate);
            const now = new Date();
            const isExpired = now > new Date(endDate.getTime() + 15 * 24 * 60 * 60 * 1000);
            return {
              ...event,
              isExpired
            };
          });
        }
      } catch (error) {
        completedEvents.value = [];
      }
    };

    // Add these functions for result report handling
    const showResultReportForm = async () => {
      showResultReportModal.value = true;
      await loadCompletedEvents();
    };

    const closeResultReportModal = () => {
      showResultReportModal.value = false;
      resultReportFile.value = null;
      selectedCompletedEvent.value = '';
    };

    const triggerResultReportInput = () => {
      if (resultReportInput.value) {
        resultReportInput.value.click();
      }
    };

    const handleResultReportUpload = (event) => {
      const file = event.target.files[0];
      if (file) {
        if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
          showNotification('error', 'Sadece PDF dosyası yükleyebilirsiniz!');
          return;
        }
        resultReportFile.value = file;
      }
    };

    const removeResultReport = () => {
      resultReportFile.value = null;
      if (resultReportInput.value) {
        resultReportInput.value.value = '';
      }
    };

    const submitResultReport = async () => {
      if (!selectedCompletedEvent.value || !resultReportFile.value) {
        showNotification('error', 'Lütfen tüm alanları doldurun.');
        return;
      }
      const formData = new FormData();
      formData.append('ReportFile', resultReportFile.value);
      try {
        await eventService.submitEventResultReport(selectedCompletedEvent.value, formData);
        showNotification('success', 'Sonuç bildiri formu başarıyla yüklendi.');
        closeResultReportModal();
        await loadCompletedEvents(); // Listeyi güncelle
      } catch (error) {
        showNotification('error', 'Sonuç bildiri formu yüklenirken hata oluştu.');
      }
    };

    // Add button for showing result report form
    const addResultReportButton = `
    <div>
      <button @click="showResultReportForm" class="btn btn-secondary">
        <i class="fas fa-file-upload"></i> Sonuç Bildiri Formu Ekle
      </button>
    </div>
    `;

    const openEventDetailModal = async (event) => {
  selectedEvent.value = {
    ...event,
    applicationFormDocumentId: event.applicationFormDocumentId || event.ApplicationFormDocumentId || null,
    managementBoardFormDocumentId: event.managementBoardFormDocumentId || event.ManagementBoardFormDocumentId || null,
    resultReportDocumentId: event.resultReportDocumentId || event.ResultReportDocumentId || null
  };
  showEventDetailModal.value = true;

  // Rezervasyonları yükle
  try {
    const token = localStorage.getItem('token');
    const res = await api.get(`/events/${event.id}/reservations`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    reservations.value = res.data || [];
  } catch (err) {
    console.error("Rezervasyonlar alınamadı", err);
    reservations.value = [];
  }
};

    const closeEventDetailModal = () => {
      showEventDetailModal.value = false;
      selectedEvent.value = null;
    };
    const getDocumentUrl = (documentId) => {
      if (!documentId) return null;
      return `https://localhost:7274/api/events/documents/${documentId}`;
    };

    const getStatusText = (status) => {
      const statusMap = {
        advisor_pending: 'Danışman Onayında',
        admin_pending: 'Admin Onayında',
        approved: 'Onaylandı',
        rejected: 'Reddedildi',
        upcoming: 'Yaklaşan',
        ongoing: 'Devam Eden',
        completed: 'Tamamlanan',
        cancelled: 'İptal Edilen'
      };
      return statusMap[status] || status;
    };

    // Belge indirme (Authorization header ile)
    const downloadDocument = async (documentId, type) => {
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7274/api/events/documents/${documentId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Dosya indirilemedi');
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        let filename = 'dokuman.pdf';
        if (type === 'application') filename = 'BasvuruFormu.pdf';
        if (type === 'management') filename = 'YonetimKuruluKarari.pdf';
        if (type === 'result') filename = 'SonucRaporu.pdf';
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
          window.URL.revokeObjectURL(url);
          document.body.removeChild(a);
        }, 100);
      } catch (err) {
        showNotification('error', 'Döküman indirilemedi.');
      }
    };
    // Belgeyi yeni sekmede aç (Authorization header ile)
    const viewDocumentWithAuth = async (documentId) => {
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7274/api/events/documents/${documentId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Dosya görüntülenemedi');
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 1000 * 60);
      } catch (err) {
        showNotification('error', 'Döküman görüntülenemedi.');
      }
    };

    return {
      clubDetails,
      applications,
      members,
      loading,
      error,
      processing,
      notification,
      selectedMember,
      memberDetails,
      showRemoveConfirm,
      memberToRemove,
      formatDate,
      showNotification,
      showMemberDetails,
      closeModal,
      confirmRemoveMember,
      removeMember,
      loadData,
      approveApplication,
      rejectApplication,
      departments,
      getDepartmentName,
      memberSearchQuery,
      departmentFilter,
      dateFilter,
      getCurrentDate,
      filteredMembers,
      isEditing,
      editedClub,
      startEditing,
      cancelEditing,
      handleImageChange,
      saveChanges,
      imageInput,
      activeTab,
      events,
      announcements,
      showEventForm,
      newEvent,
      showAnnouncementForm,
      newAnnouncement,
      isAdvisor,
      getEventStatusText,
      getEventStatusClass,
      getAnnouncementStatusText,
      submitEventForm,
      submitAnnouncementForm,
      approveEvent,
      rejectEvent,
      approveAnnouncement,
      rejectAnnouncement,
      closeEventForm,
      closeAnnouncementForm,
      expenses,
      loadExpenses,
      showExpenseForm,
      showReservationModal,
      closeExpenseForm,
      newExpense,
      submitExpenseForm,
      handleEventImageUpload,
      handleAnnouncementImageUpload,
      triggerFileInput,
      removeImage,
      getImageUrl,
      handleImageError,
      loadPendingEvents,
      showDepartmentModal,
      departmentDistribution,
      loadingDepartmentData,
      errorDepartmentData,
      loadDepartmentDistribution,
      showDepartmentDistributionModal,
      closeDepartmentModal,
      getTotalMemberCount,
      parsedDepartmentData,
      triggerDocumentInput,
      handleDocumentUpload,
      removeDocument,
      viewDocument,
      deleteExpense,
      editExpense,
      eventCategories, // BUNU EKLE!
      applicationFormInput,
      managementFormInput,
      triggerApplicationFormInput,
      triggerManagementFormInput,
      handleApplicationFormUpload,
      handleManagementFormUpload,
      removeApplicationForm,
      removeManagementForm,
      resultReportInput,
      resultReportFile,
      selectedCompletedEvent,
      showResultReportModal,
      completedEvents,
      showResultReportForm,
      closeResultReportModal,
      triggerResultReportInput,
      handleResultReportUpload,
      removeResultReport,
      submitResultReport,
      addResultReportButton,
      selectedEvent,
      showEventDetailModal,
      openEventDetailModal,
      closeEventDetailModal,
      getDocumentUrl,
      getStatusText,
      downloadDocument,
      viewDocumentWithAuth,
      reservations,
    };
  }
}
</script>

<style scoped>
.community-management {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 30px;
  text-align: center;
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.page-header h1 {
  margin-bottom: 10px;
  color: #2c3e50;
}

.club-description {
  color: #666;
  font-size: 1.1rem;
  margin-bottom: 20px;
}

.edit-btn {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.3s;
}

.edit-btn:hover {
  background: #45a049;
}

.edit-form {
  max-width: 600px;
  margin: 20px auto;
  text-align: left;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #374151;
  font-weight: 500;
}

.form-group input[type="file"] {
  display: none;
}

.image-upload-container {
  width: 100%;
  margin-top: 10px;
  max-width: 500px;
  margin-left: auto;
  margin-right: auto;
}

.image-upload-box {
  width: 100%;
  height: 300px;
  border: 2px dashed #ccc;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  background-color: #f8f9fa;
  position: relative;
  overflow: hidden;
}

.image-upload-box:hover {
  border-color: #3b82f6;
  background-color: #f0f4f1;
}

.upload-icon {
  font-size: 48px;
  color: #666;
  margin-bottom: 10px;
}

.upload-text {
  color: #666;
  font-size: 16px;
  text-align: center;
}

.preview-container {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  background-color: #f8f9fa;
}

.preview-container img {
  max-width: 100%;
  max-height: 100%;
  width: auto;
  height: auto;
  object-fit: contain;
  padding: 10px;
}

.remove-image {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(0, 0, 0, 0.6);
  color: white;
  border: none;
  border-radius: 50%;
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 10;
}

.remove-image:hover {
  background: rgba(0, 0, 0, 0.8);
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
}

.form-control:focus {
  outline: none;
  border-color: #4CAF50;
}

.preview-image {
  max-width: 200px;
  margin-top: 10px;
  border-radius: 4px;
}

.edit-actions {
  display: flex;
  gap: 10px;
  justify-content: center;
  margin-top: 20px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.3s;
  display: flex;
  align-items: center;
  gap: 5px;
}

.btn i {
  font-size: 14px;
}

.btn-primary {
  background: #4CAF50;
  color: white;
}

.btn-primary:hover {
  background: #45a049;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #5a6268;
}

.stats-wrapper {
  display: flex;
  flex-direction: column;
  width: 100%;
  position: relative;
  margin-bottom: 30px;
}

.stats-header {
  display: flex;
  justify-content: flex-end;
  width: 100%;
  margin-bottom: 15px;
  position: relative;
}

.modern-button {
  background-color: #3478F6;
  color: white;
  border: none;
  padding: 11px 25px;
  border-radius: 50px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  box-shadow: 0 2px 5px rgba(59, 130, 246, 0.3);
  margin-right: 5px;
}

.modern-button:hover {
  background-color: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
}

.modern-button i {
  font-size: 1rem;
}

.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  width: 100%;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 15px;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.stat-card-content {
  display: flex;
  align-items: center;
  gap: 15px;
  width: 100%;
}

.stat-card-content i {
  font-size: 2.2rem;
  color: #3498db;
}

.stat-info {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.stat-info h3 {
  margin: 0;
  font-size: 1.8rem;
  color: #2c3e50;
}

.stat-info p {
  margin: 0;
  color: #666;
  font-size: 1rem;
}

.loading-container,
.error-container {
  text-align: center;
  padding: 40px;
}

.applications-section {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.applications-section h2 {
  margin-bottom: 20px;
  color: #2c3e50;
}

.no-applications {
  text-align: center;
  padding: 20px;
  color: #666;
}

.table-responsive {
  overflow-x: auto;
  margin: 20px 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.custom-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.custom-table th {
  background-color: #f8f9fa;
  padding: 12px 15px;
  font-weight: 600;
  color: #2c3e50;
  border-bottom: 2px solid #eee;
}

.custom-table td {
  padding: 12px 15px;
  border-bottom: 1px solid #eee;
  vertical-align: middle;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}

.user-name {
  font-weight: 500;
  color: #2c3e50;
}

.user-id {
  font-size: 0.85rem;
  color: #666;
}

.role-badge {
  display: inline-block;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 500;
}

.role-badge.başkan {
  background-color: #ffeaa7;
  color: #d35400;
}

.role-badge.üye {
  background-color: #e8f4f8;
  color: #2980b9;
}

.status-badge {
  display: inline-block;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 500;
}

.status-badge.active {
  background-color: #ebfbf0;
  color: #1f9d55;
}

.status-badge.pending {
  background-color: #fff5f5;
  color: #e53e3e;
}

.actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn:hover {
  transform: translateY(-1px);
}

.action-btn.approve {
  background-color: #ebfbf0;
  color: #1f9d55;
}

.action-btn.reject {
  background-color: #fff5f5;
  color: #e53e3e;
}

.action-btn.info {
  background-color: #ebf8ff;
  color: #2b6cb0;
}

.action-btn.remove {
  background-color: #fff5f5;
  color: #e53e3e;
}

.notification {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 15px 20px;
  border-radius: 8px;
  z-index: 1000;
  min-width: 300px;
  box-shadow: 0 3px 6px rgba(0,0,0,0.16);
}

.notification-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.notification-success {
  background-color: #2ecc71;
  color: white;
}

.notification-error {
  background-color: #e74c3c;
  color: white;
}

.notification-info {
  background-color: #3498db;
  color: white;
}

.notification-close {
  background: transparent;
  border: none;
  color: white;
  cursor: pointer;
  padding: 5px;
  margin-left: 10px;
  opacity: 0.8;
  transition: opacity 0.2s;
}

.notification-close:hover {
  opacity: 1;
}

.members-section {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-top: 30px;
}

.members-section h2 {
  margin-bottom: 20px;
  color: #2c3e50;
  font-size: 1.5rem;
  border-bottom: 2px solid #f0f0f0;
  padding-bottom: 10px;
}

.no-members {
  text-align: center;
  padding: 40px;
  color: #7f8c8d;
  background: #f8f9fa;
  border-radius: 8px;
  margin: 20px 0;
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
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 1rem;
}

.modal-footer {
  padding: 1rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #666;
}

.close-btn:hover {
  color: #333;
}

.member-details {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.detail-row {
  display: flex;
  gap: 1rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid #eee;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  font-weight: bold;
  min-width: 120px;
}

.confirmation-modal {
  max-width: 400px;
}

.confirmation-modal .modal-body {
  text-align: center;
  padding: 1.5rem;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  border: none;
  font-weight: 500;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn:hover {
  opacity: 0.9;
}

.filters-section {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
  align-items: center;
}

.search-box {
  position: relative;
  width: 300px;
}

.search-box input {
  width: 100%;
  padding: 0.5rem 2.5rem 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
}

.search-box i {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #666;
}

.filter-group {
  display: flex;
  gap: 1rem;
}

.filter-group select,
.filter-group .date-filter {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
  min-width: 200px;
  background-color: white;
}

.filter-group .date-filter {
  cursor: pointer;
}

.filter-group .date-filter::-webkit-calendar-picker-indicator {
  cursor: pointer;
}

.club-profile {
  display: flex;
  gap: 30px;
  margin-bottom: 30px;
  background: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.club-image-container {
  position: relative;
  width: 300px;
  height: 300px;
  border-radius: 10px;
  overflow: hidden;
  flex-shrink: 0;
}

.club-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.club-info {
  flex: 1;
}

.club-details h1 {
  margin: 0 0 15px 0;
  color: #333;
  font-size: 2rem;
}

.club-description {
  color: #666;
  line-height: 1.6;
  margin-bottom: 20px;
  font-size: 1.1rem;
}

.club-edit-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-control {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
}

.form-control:focus {
  outline: none;
  border-color: #4CAF50;
}

.edit-actions {
  display: flex;
  gap: 10px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-primary {
  background: #4CAF50;
  color: white;
}

.btn-primary:hover {
  background: #45a049;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #5a6268;
}

.btn-danger {
  background: #dc3545;
  color: white;
}

.btn-danger:hover {
  background: #c82333;
}

.edit-image-btn {
  position: absolute;
  bottom: 10px;
  right: 10px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  cursor: pointer;
  transition: background 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.edit-image-btn:hover {
  background: rgba(0, 0, 0, 0.9);
}

.preview-image {
  max-width: 200px;
  max-height: 200px;
  margin-top: 10px;
  border-radius: 5px;
  object-fit: cover;
}

@media (max-width: 768px) {
  .community-management {
    padding: 10px;
  }

  .table-responsive {
    overflow-x: auto;
    margin: 20px 0;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }

  .custom-table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
  }

  .custom-table th {
    background-color: #f8f9fa;
    padding: 12px 15px;
    font-weight: 600;
    color: #2c3e50;
    border-bottom: 2px solid #eee;
  }

  .custom-table td {
    padding: 12px 15px;
    border-bottom: 1px solid #eee;
    vertical-align: middle;
  }

  .user-info {
  display: flex;
  align-items: center;
    gap: 10px;
  }

  .user-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    object-fit: cover;
  }

  .user-name {
    font-weight: 500;
    color: #2c3e50;
  }

  .user-id {
    font-size: 0.85rem;
    color: #666;
  }

  .role-badge {
    display: inline-block;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.85rem;
    font-weight: 500;
  }

  .role-badge.başkan {
    background-color: #ffeaa7;
    color: #d35400;
  }

  .role-badge.üye {
    background-color: #e8f4f8;
    color: #2980b9;
  }

  .status-badge {
    display: inline-block;
    padding: 4px 8px;
  border-radius: 4px;
    font-size: 0.85rem;
    font-weight: 500;
  }

  .status-badge.active {
    background-color: #ebfbf0;
    color: #1f9d55;
  }

  .status-badge.pending {
    background-color: #fff5f5;
    color: #e53e3e;
  }

  .actions {
  display: flex;
    gap: 8px;
  }

  .action-btn {
    width: 32px;
    height: 32px;
  border: none;
  border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
  cursor: pointer;
    transition: all 0.2s;
  }

  .action-btn:hover {
    transform: translateY(-1px);
  }

  .action-btn.approve {
    background-color: #ebfbf0;
    color: #1f9d55;
  }

  .action-btn.reject {
    background-color: #fff5f5;
    color: #e53e3e;
  }

  .action-btn.info {
    background-color: #ebf8ff;
    color: #2b6cb0;
  }

  .action-btn.remove {
    background-color: #fff5f5;
    color: #e53e3e;
  }

  .notification {
    top: 10px;
    right: 10px;
    left: 10px;
    min-width: auto;
  }

  .filters-section {
    flex-direction: column;
    gap: 0.8rem;
  }

  .search-box,
  .filter-group {
    width: 100%;
  }

  .filter-group select,
  .filter-group .date-filter {
    width: 100%;
  }
}

/* Yeni stiller */
.tabs-container {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  padding: 0 1rem;
}

.tab-button {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  background: #f5f5f5;
  color: #666;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
}

.tab-button:hover {
  background: #e0e0e0;
}

.tab-button.active {
  background: #4CAF50;
  color: white;
}

.tab-content {
  background: white;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.create-btn {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 2rem;
  transition: background 0.3s;
}

.create-btn:hover {
  background: #45a049;
}

.no-content {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.events-container {
  padding: 2rem;
  background: #f8fafc;
}

.events-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding: 0 1rem;
}

.events-header h2 {
  font-size: 1.8rem;
  color: #1e293b;
  font-weight: 600;
}

.primary-button {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.1);
}

.primary-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
}

.primary-button:before {
  content: '+';
  font-size: 1.2rem;
  font-weight: 600;
}

.events-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1rem;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

.event-card {
  position: relative;
  background: #ffffff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid #e2e8f0;
  will-change: transform, box-shadow, border-color;
}

.event-card:hover {
  transform: translateY(-4px);
  border-color: #3b82f6;
  box-shadow: 
    0 0 0 2px rgba(59, 130, 246, 0.1),
    0 12px 24px -4px rgba(59, 130, 246, 0.1),
    0 8px 16px -4px rgba(17, 24, 39, 0.1);
}

.event-card::after {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: 12px;
  padding: 2px;
  background: linear-gradient(
    135deg,
    rgba(59, 130, 246, 0),
    rgba(59, 130, 246, 0.3),
    rgba(147, 197, 253, 0.3),
    rgba(59, 130, 246, 0)
  );
  -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  -webkit-mask-composite: xor;
  mask-composite: exclude;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.event-card:hover::after {
  opacity: 1;
}

.event-image-wrapper {
  position: relative;
  width: 100%;
  padding-top: 56.25%; /* 16:9 aspect ratio */
  overflow: hidden;
}

.event-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
  transition: transform 0.3s ease;
}

.event-card:hover .event-image {
  transform: scale(1.05);
}

.event-content {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.event-main-info {
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 1rem;
}

.event-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
  line-height: 1.3;
}

.event-description {
  color: #64748b;
  font-size: 0.875rem;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.meta-group {
  display: grid;
  gap: 0.5rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.meta-item i {
  font-size: 0.9rem;
  color: #3b82f6;
  width: 1.75rem;
  height: 1.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eff6ff;
  border-radius: 6px;
}

.meta-text {
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
}

.meta-label {
  font-size: 0.65rem;
  text-transform: uppercase;
  color: #64748b;
  font-weight: 600;
  letter-spacing: 0.05em;
}

.meta-value {
  font-size: 0.875rem;
  color: #1e293b;
  font-weight: 600;
}

.meta-time {
  font-size: 0.75rem;
  color: #64748b;
}

.participants-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.participants-info i {
  font-size: 0.9rem;
  color: #8b5cf6;
  width: 1.75rem;
  height: 1.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f3e8ff;
  border-radius: 6px;
}

.participants-count {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
}

.participants-label {
  font-size: 0.65rem;
  color: #64748b;
  display: block;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.event-status-badge {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  z-index: 10;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-weight: 500;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  backdrop-filter: blur(8px);
  background: rgba(255, 255, 255, 0.9);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.event-status-badge.pending {
  color: #854d0e;
  background: rgba(254, 243, 199, 0.9);
}

.event-status-badge.approved {
  color: #166534;
  background: rgba(220, 252, 231, 0.9);
}

.event-status-badge.rejected {
  color: #991b1b;
  background: rgba(254, 226, 226, 0.9);
}

.event-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.75rem;
  border-top: 1px solid #e2e8f0;
}

.action-button {
  flex: 1;
  padding: 0.5rem;
  border: none;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

@media (max-width: 768px) {
  .events-list {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 0.75rem;
    padding: 0.5rem;
  }

  .event-image-wrapper {
    height: 120px;
  }

  .event-content {
    padding: 0.75rem;
    gap: 0.75rem;
  }

  .meta-item, .participants-info {
    padding: 0.4rem;
  }

  .meta-item i, .participants-info i {
    width: 1.5rem;
    height: 1.5rem;
    font-size: 0.8rem;
  }

  .event-image-wrapper {
    padding-top: 66.67%; /* 3:2 aspect ratio for mobile */
  }
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}

.form-actions button {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s ease;
}

.form-actions .submit-btn {
  background: #4CAF50;
  color: white;
}

.form-actions .cancel-btn {
  background: #f5f5f5;
  color: #666;
}

.form-actions button:hover {
  transform: translateY(-2px);
}

.department-distribution-btn {
  /* Ensuring it looks like other tab buttons */
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  background: #f5f5f5; /* Default tab button background */
  color: #666; /* Default tab button text color */
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
}

.department-distribution-btn:hover {
  background: #e0e0e0; /* Hover effect like other tab buttons */
}

/* If you want it to look active when modal is open, 
   you might need a class binding like :class="{ active: showDepartmentModal }" 
   and corresponding .active styles, but this might conflict with tab active states.
   For now, it will look like a non-active tab button.
*/

.department-distribution-btn i {
  margin-right: 8px; /* Space between icon and text, adjust as needed */
}

.department-distribution-container {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin-bottom: 20px;
}

.department-item {
  background: white;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
  transition: all 0.3s ease;
  border: 1px solid #e2e8f0;
}

.department-item:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
}

.department-header {
  padding: 10px 15px;
  color: white;
}

.department-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
}

.department-content {
  padding: 15px;
}

.member-count {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.member-count i {
  color: #3b82f6;
  font-size: 1.2rem;
}

.member-count span {
  font-size: 1.1rem;
  font-weight: 600;
  color: #374151;
}

.percentage-bar-container {
  height: 20px;
  background: #f0f0f0;
  border-radius: 10px;
  position: relative;
  overflow: hidden;
}

.percentage-bar {
  height: 100%;
  border-radius: 10px;
  transition: width 1s ease-out;
}

.percentage-text {
  position: absolute;
  right: 10px;
  top: 0;
  height: 100%;
  display: flex;
  align-items: center;
  font-size: 0.8rem;
  font-weight: 600;
  color: #1f2937;
}

.department-summary {
  text-align: center;
  padding: 15px;
  background: #f9fafb;
  border-radius: 8px;
  margin-top: 10px;
}

.department-summary h3 {
  margin: 0;
  color: #1f2937;
  font-size: 1.2rem;
}

.department-modal {
  max-width: 700px;
  width: 90%;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  animation: fadeIn 0.3s ease;
  border: 1px solid #e2e8f0;
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}

.distribution-content {
  padding: 10px;
}

.spinner {
  border: 4px solid rgba(0, 0, 0, 0.1);
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border-left-color: #3b82f6;
  animation: spin 1s linear infinite;
  margin: 0 auto 15px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px;
  color: #666;
}

.error-container {
  text-align: center;
  padding: 30px;
  color: #e53e3e;
}

.error-container p {
  margin-bottom: 15px;
}

.no-data {
  flex: 1;
  text-align: center;
  color: #666;
  padding: 30px 0;
}

.empty-icon {
  font-size: 4rem;
  color: #ccc;
  margin-bottom: 1rem;
}

.btn-primary {
  background: #3b82f6;
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-primary:hover {
  background: #2563eb;
}

.department-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  border-radius: 8px;
  overflow: hidden;
  font-size: 16px;
}

.department-table th {
  background-color: #f3f4f6;
  color: #2c3e50;
  font-weight: 600;
  padding: 15px 20px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
  font-size: 18px;
}

.department-table td {
  padding: 15px 20px;
  border-bottom: 1px solid #e5e7eb;
  color: #374151;
}

.department-table td:first-child {
  font-weight: 500;
}

.department-table td:last-child {
  font-weight: 600;
  text-align: center;
}

.department-table tr:last-child td {
  border-bottom: none;
}

.department-table tbody tr:hover {
  background-color: #f9fafb;
}

.department-summary {
  margin-top: 20px;
  text-align: right;
  font-weight: 600;
  color: #1f2937;
  padding: 0 10px;
  font-size: 18px;
}

.department-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 6px 12px;
  margin-top: 10px;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.department-button:hover {
  background-color: #2563eb;
  transform: translateY(-1px);
}

.department-button i {
  margin-right: 5px;
  font-size: 0.9rem;
}

.modern-button {
  background-color: #3478F6;
  color: white;
  border: none;
  padding: 11px 25px;
  border-radius: 50px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  box-shadow: 0 2px 5px rgba(59, 130, 246, 0.3);
  margin-bottom: 15px;
  align-self: flex-end;
}

.modern-button:hover {
  background-color: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
}

.modern-button i {
  font-size: 1rem;
}

.stats-container {
  display: flex;
  flex-direction: column;
  margin-bottom: 20px;
}

.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  width: 100%;
}

.department-btn {
  background-color: #4CAF50 !important;
  color: white;
  border-radius: 6px !important;
  padding: 8px 16px !important;
  box-shadow: none !important;
  outline: none !important;
  border: none !important;
}

.department-btn i {
  color: white;
}

.document-upload-container {
  width: 100%;
  margin-top: 10px;
}

.document-upload-box {
  width: 100%;
  min-height: 100px;
  border: 2px dashed #ccc;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  background-color: #f8f9fa;
}

.document-upload-box:hover {
  border-color: #3b82f6;
  background-color: #f0f4f1;
}

.document-preview {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  background: #fff;
  border-radius: 6px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.document-preview i {
  font-size: 24px;
  color: #3b82f6;
}

.remove-document {
  background: none;
  border: none;
  color: #dc3545;
  cursor: pointer;
  padding: 5px;
  margin-left: 10px;
}

.action-btn.view {
  background-color: #e3f2fd;
  color: #1976d2;
}

.action-btn.edit {
  background-color: #fff3e0;
  color: #f57c00;
}

.actions {
  display: flex;
  gap: 8px;
}
.action-btn.view {
  background-color: #e3f2fd;
  color: #1976d2;
  border: none;
  padding: 8px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.action-btn.view:hover {
  background-color: #bbdefb;
}

.action-btn.delete {
  background-color: #ffebee;
  color: #d32f2f;
  border: none;
  padding: 8px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.action-btn.delete:hover {
  background-color: #ffcdd2;
}

.action-btn.edit {
  background-color: #e3f2fd;
  color: #1976d2;
  border: none;
  padding: 8px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.action-btn.edit:hover {
  background-color: #bbdefb;
}

.category-checkbox-group {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-top: 8px;
}
.category-checkbox {
  position: relative;
  padding-left: 32px;
  cursor: pointer;
  font-size: 1rem;
  user-select: none;
  color: #333;
  background: #f7f7fa;
  border-radius: 8px;
  transition: background 0.2s;
  min-width: 160px;
  min-height: 40px;
  display: flex;
  align-items: center;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04);
}
.category-checkbox input[type="checkbox"] {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}
.category-checkbox .checkmark {
  position: absolute;
  left: 8px;
  top: 50%;
  transform: translateY(-50%);
  height: 20px;
  width: 20px;
  background-color: #fff;
  border: 2px solid #007bff;
  border-radius: 4px;
  transition: background 0.2s, border 0.2s;
}
.category-checkbox input:checked ~ .checkmark {
  background-color: #007bff;
  border-color: #007bff;
}
.category-checkbox .checkmark:after {
  content: '';
  position: absolute;
  display: none;
}
.category-checkbox input:checked ~ .checkmark:after {
  display: block;
}
.category-checkbox .checkmark:after {
  left: 6px;
  top: 2px;
  width: 6px;
  height: 12px;
  border: solid white;
  border-width: 0 3px 3px 0;
  transform: rotate(45deg);
}

.category-radio-group {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-top: 8px;
}
.category-radio {
  position: relative;
  padding-left: 32px;
  cursor: pointer;
  font-size: 1rem;
  user-select: none;
  color: #333;
  background: #f7f7fa;
  border-radius: 8px;
  transition: background 0.2s;
  min-width: 180px;
  min-height: 40px;
  display: flex;
  align-items: center;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04);
}
.category-radio input[type="radio"] {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}
.category-radio .custom-radio {
  position: absolute;
  left: 8px;
  top: 50%;
  transform: translateY(-50%);
  height: 20px;
  width: 20px;
  background-color: #fff;
  border: 2px solid #007bff;
  border-radius: 50%;
  transition: background 0.2s, border 0.2s;
}
.category-radio input:checked ~ .custom-radio {
  background-color: #007bff;
  border-color: #007bff;
}
.category-radio .custom-radio:after {
  content: '';
  position: absolute;
  display: none;
}
.category-radio input:checked ~ .custom-radio:after {
  display: block;
}
.category-radio .custom-radio:after {
  left: 6px;
  top: 6px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: white;
  position: absolute;
}

/* File upload styles */
.file-upload-container {
  width: 100%;
  margin: 10px 0;
}

.file-upload-box {
  border: 2px dashed #ccc;
  border-radius: 8px;
  padding: 20px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s;
}

.file-upload-box:hover {
  border-color: #4f46e5;
}

.file-info {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 10px;
  background-color: #f3f4f6;
  padding: 10px;
  border-radius: 6px;
  gap: 10px;
}

.file-info i {
  color: #4f46e5;
  font-size: 1.2rem;
}

.remove-file {
  background: none;
  border: none;
  color: #ef4444;
  cursor: pointer;
  padding: 5px;
}

.remove-file i {
  color: #ef4444;
}

span.required {
  color: #ef4444;
  margin-left: 2px;
}

.button-group {
  display: flex;
  gap: 10px;
}

.secondary-button {
  background-color: #6b7280;
  color: white;
  border: none;
  border-radius: 10px;
  padding: 10px 16px;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.secondary-button:hover {
  background-color: #4b5563;
}

/* Etkinlik detay modalı ve belgeler için özel stiller */
.event-detail-modal {
  max-width: 500px;
  min-width: 350px;
  padding: 0;
}
.event-detail-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
  margin-bottom: 24px;
}
.event-detail-grid strong {
  display: inline-block;
  min-width: 120px;
}
.event-desc {
  margin-top: 4px;
  color: #444;
  font-size: 1.05em;
}
.event-documents {
  margin-top: 18px;
  background: #f8fafc;
  border-radius: 8px;
  padding: 12px 16px;
}
.event-documents h3 {
  margin-bottom: 10px;
  font-size: 1.1em;
  color: #374151;
}
.event-documents ul {
  list-style: none;
  padding: 0;
  margin: 0;
}
.event-documents li {
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.doc-btn {
  margin-left: 8px;
  color: #fff;
  background: #3b82f6;
  border: none;
  border-radius: 8px;
  padding: 8px 18px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s, box-shadow 0.2s, color 0.2s;
  box-shadow: 0 1px 4px rgba(59,130,246,0.08);
  outline: none;
  display: inline-block;
}
.doc-btn:hover {
  background: #2563eb;
  color: #e0f2fe;
}
.doc-btn:active {
  background: #1d4ed8;
  color: #fff;
}
.doc-missing {
  color: #ef4444;
  font-weight: 500;
  margin-left: 8px;
  font-size: 1em;
  background: #fef2f2;
  padding: 2px 10px;
  border-radius: 6px;
  display: inline-block;
}
</style>