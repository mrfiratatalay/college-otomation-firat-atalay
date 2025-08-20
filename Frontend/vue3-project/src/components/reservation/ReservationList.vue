<template>
  <div class="reservation-list">
    <div class="list-header">
      <h3>Tüm Rezervasyonlar</h3>
    </div>

    <div v-if="loading" class="loading-indicator">
      <div class="spinner"></div>
      <p>Rezervasyonlar yükleniyor...</p>
    </div>

    <div v-else-if="reservations.length === 0" class="empty-state">
      <p>Henüz herhangi bir rezervasyon bulunmamaktadır.</p>
    </div>

    <ul v-else class="reservation-items">
      <li
        v-for="reservation in reservations"
        :key="reservation.id"
        class="reservation-card"
      >
        <div class="reservation-title">
          <i class="fas fa-map-marker-alt text-blue-500 mr-2"></i>
          {{ reservation.locationName }}
        </div>
        <div class="reservation-detail">
          <i class="fas fa-clock"></i>
          Saat: {{ reservation.startHour }} - {{ reservation.endHour }}
        </div>
        <div class="reservation-detail">
          <i class="fas fa-users"></i>
          Kulüp: {{ reservation.clubName }}
        </div>
        <div class="reservation-detail">
          <i class="fas fa-calendar-alt"></i>
          Tarih: {{ formatDate(reservation.eventDate) }}
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import axios from 'axios'

const reservations = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await axios.get('/api/eventreservation/all-reservations')
    reservations.value = data
  } catch (err) {
    console.error('Rezervasyonlar alınamadı', err)
  } finally {
    loading.value = false
  }
})

function formatDate(dateStr) {
  console.log('Gelen tarih:', dateStr)
  const date = new Date(dateStr)
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year}`
}


</script>

<style scoped>
.reservation-list {
  padding: 0.5rem;
}

.list-header {
  margin-bottom: 1.5rem;
}

.list-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c3e50;
}

.loading-indicator {
  text-align: center;
  color: #7f8c8d;
  padding: 2rem 0;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 4px solid #ddd;
  border-top-color: #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.empty-state {
  background: #fefefe;
  border: 1px dashed #ccc;
  padding: 2rem;
  text-align: center;
  border-radius: 6px;
  color: #7f8c8d;
  font-weight: 500;
}

.reservation-items {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.reservation-card {
  background: #f8f9fa;
  border: 1px solid #e0e0e0;
  padding: 1rem;
  border-radius: 6px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.03);
}

.reservation-title {
  font-weight: 600;
  color: #34495e;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
}

.reservation-detail {
  color: #555;
  font-size: 0.95rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.25rem;
}
</style>
