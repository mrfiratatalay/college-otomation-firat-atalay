<template>
  <div class="application-card">
    <div class="application-header">
      <div class="application-icon" :class="cardTypeClass">
        <i :class="iconClass"></i>
      </div>
      <div class="application-title">
        <h4>{{ titleText }}</h4>
        <span class="application-subtitle">{{ subtitleText }}</span>
      </div>
    </div>
    <div class="application-footer">
      <span :class="statusClass">{{ statusText }}</span>
      <!-- Show actions only for pending club applications -->
      <div v-if="type === 'club'" class="application-actions">
        <button @click="$emit('approve', application.clubId)" class="action-btn approve" title="Onayla">
          <i class="fas fa-check"></i>
        </button>
        <button @click="$emit('reject', application.clubId)" class="action-btn reject" title="Reddet">
          <i class="fas fa-times"></i>
        </button>
      </div>
       <!-- Optionally show details for member applications -->
       <span v-else-if="type === 'member'" class="application-date">{{ formatDate(application.joinedAt) }}</span>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ApplicationCard',
  props: {
    application: {
      type: Object,
      required: true
    },
    type: { // 'club' or 'member'
      type: String,
      required: true,
      validator: (value) => ['club', 'member'].includes(value)
    }
  },
  emits: ['approve', 'reject'], // Declare emitted events
  computed: {
    isClubApplication() {
      return this.type === 'club';
    },
    iconClass() {
      return this.isClubApplication ? 'fas fa-building' : 'fas fa-user-plus';
    },
    cardTypeClass() {
       return this.isClubApplication ? 'club-icon' : 'member-icon';
    },
    titleText() {
      // For club use club name, for member use user name
      return this.isClubApplication ? this.application.name : this.application.name; 
    },
    subtitleText() {
       // For club use category, for member use club name
       return this.isClubApplication ? (this.application.categoryName || 'Topluluk Başvurusu') : this.application.clubName;
    },
    statusText() {
        // Assuming club applications always have 'pending' status here
        // And member applications passed in dashboardData.recentApplications also have 'Bekliyor' status
        return this.isClubApplication ? 'Onay Bekliyor' : 'Üyelik Başvurusu';
    },
     statusClass() {
      return this.isClubApplication ? 'status-pending' : 'status-member';
    }
  },
  methods: {
     formatDate(dateString) {
      if (!dateString) return '';
      try {
          const date = new Date(dateString);
          if (isNaN(date.getTime())) {
              return 'Geçersiz T.'; // Shorter error for card
          }
          return new Intl.DateTimeFormat('tr-TR', {
            day: '2-digit', month: '2-digit', year: 'numeric'
          }).format(date);
      } catch (e) {
          return 'Tarih H.'; // Shorter error for card
      }
    }
  }
};
</script>

<style scoped>
.application-card {
  background: #ffffff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  border: 1px solid #e9ecef;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  gap: 12px; /* Space between header and footer */
}

.application-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.08);
}

.application-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.application-icon {
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.application-icon.club-icon {
  background-color: #ffc107; /* Warning yellow for pending clubs */
}
.application-icon.member-icon {
  background-color: #28a745; /* Success green for member requests */
}


.application-title {
    flex-grow: 1;
    overflow: hidden;
}

.application-title h4 {
  margin: 0;
  font-size: 0.95rem; /* Slightly smaller */
  color: #343a40;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.application-subtitle {
  font-size: 0.8rem;
  color: #6c757d;
  display: block; /* Ensure it takes its own line if needed */
   white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.application-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.8rem;
  padding-top: 10px;
  border-top: 1px solid #f1f3f5; /* Subtle separator */
}

.status-pending {
  font-weight: 500;
  color: #ffc107; /* Warning yellow */
  background-color: rgba(255, 193, 7, 0.1);
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
}
.status-member {
  font-weight: 500;
  color: #28a745; /* Success green */
   background-color: rgba(40, 167, 69, 0.1);
  padding: 3px 8px;
  border-radius: 4px;
   font-size: 0.75rem;
}

.application-date {
    color: #6c757d;
}

.application-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 28px; /* Smaller buttons */
  height: 28px;
  border-radius: 50%;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  color: white;
  font-size: 0.8rem; /* Smaller icon */
}

.action-btn.approve {
  background-color: #28a745; /* Green */
}
.action-btn.approve:hover {
  background-color: #218838; /* Darker green */
}

.action-btn.reject {
  background-color: #dc3545; /* Red */
}
.action-btn.reject:hover {
  background-color: #c82333; /* Darker red */
}

</style> 