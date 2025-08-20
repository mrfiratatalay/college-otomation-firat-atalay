
<template>
  <div class="admin-events">
    <div class="page-header">
      <h1>Tesisler ve Rezervasyonlar</h1>
      <div class="action-btns">
        <button @click="activeTab = 'reservations'" :class="{ active: activeTab === 'reservations' }">
          <i class="fas fa-list"></i> Rezervasyonlar
        </button>
        <button @click="activeTab = 'settings'" :class="{ active: activeTab === 'settings' }">
          <i class="fas fa-cog"></i> Rezervasyon Ayarları
        </button>
      </div>
    </div>

    <div v-if="activeTab === 'reservations'">
      <ReservationList />
    </div>

    <div v-if="activeTab === 'settings'">
      <ReservationSettings />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ReservationList from '@/components/reservation/ReservationList.vue'
import ReservationSettings from '@/components/reservation/ReservationSettings.vue'

const activeTab = ref('reservations')
</script>

<style scoped>
.admin-events {
  padding: 1rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.page-subheader {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

h1,
h2 {
  font-size: 1.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.action-btns {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.action-btns button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  border: none;
  cursor: pointer;
  font-size: 0.95rem;
}

.add-btn {
  background: #2ecc71;
  color: white;
}

.add-btn:hover {
  background: #27ae60;
}

.edit-btn {
  background: #3498db;
  color: white;
}

.edit-btn:hover {
  background: #2980b9;
}

button.active {
  background: #34495e;
  color: white;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  color: #888;
  cursor: pointer;
}

.form-label {
  font-weight: 600;
  color: #2c3e50;
}

.form-input {
  padding: 0.6rem 1rem;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 1rem;
  width: 100%;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}

.cancel-btn {
  background: #bdc3c7;
  color: white;
}

.cancel-btn:hover {
  background: #95a5a6;
}

.save-btn {
  background: #2ecc71;
  color: white;
}

.save-btn:hover {
  background: #27ae60;
}
</style>
