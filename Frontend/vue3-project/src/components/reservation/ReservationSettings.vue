<template>
  <div class="reservation-settings">
    <div class="settings-header">
      <h2 class="text-xl font-semibold text-gray-700">Tesis Ayarları</h2>
      <div class="action-buttons">
        <button @click="showAddLocationModal = true" class="btn btn-primary">
          <i class="fas fa-plus mr-1"></i> Yeni Tesis
        </button>
      </div>
    </div>

    <div v-if="locations.length === 0" class="empty-state">
      Henüz kayıtlı bir tesis bulunmamaktadır.
    </div>

    <div v-else class="facility-cards">
      <div v-for="location in locations" :key="location.id" class="facility-card">
        <div class="flex justify-between items-center mb-2">
          <h4 class="card-title">{{ location.name }}</h4>
          <button class="edit-btn" @click="openEditNameModal(location)">
            <i class="fas fa-pen"></i> Düzenle
          </button>
        </div>
        <div class="calendar-icon">
          <button @click="openCalendar(location)" title="Takvimi Görüntüle">
            <i class="fas fa-calendar-alt text-2xl text-blue-500 hover:text-blue-700"></i>
          </button>
        </div>

        <!-- Haftalık Saat Çizelgesi -->
        <div class="weekly-schedule mt-6">
          <h4 class="font-semibold mb-2">Haftalık Saat Çizelgesi:</h4>
          <div v-for="(dayName, dayIndex) in weekDays" :key="dayIndex" class="day-block">
            <div class="day-header">
              <span>{{ dayName }}</span>
              <button @click="openDailyModal(location, dayIndex)" class="edit-day-btn">
                <i class="fas fa-edit"></i> Düzenle
              </button>
            </div>
            <ul>
              <li
                v-for="slot in getSortedSlots(location.dailySlots, dayIndex)"
                :key="slot.id"
                class="slot-item"
              >
                ⏰ {{ formatTime(slot.startHour) }} - {{ formatTime(slot.endHour) }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- Yeni Tesis Modalı -->
    <div v-if="showAddLocationModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Yeni Tesis Ekle</h2>
          <button @click="showAddLocationModal = false" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <label class="form-label">Tesis Adı</label>
          <input v-model="newLocationName" type="text" class="form-input" placeholder="Örn: Spor Salonu" />
          <div class="form-actions mt-4">
            <button class="cancel-btn" @click="showAddLocationModal = false">İptal</button>
            <button class="save-btn" @click="saveNewLocation">Kaydet</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Tesis Adı Güncelleme Modalı -->
    <div v-if="showEditNameModal" class="modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Tesis Adını Düzenle</h2>
          <button @click="showEditNameModal = false" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <label class="form-label">Yeni Tesis Adı</label>
          <input v-model="editName" type="text" class="form-input" />
          <div class="form-actions mt-4">
            <button class="cancel-btn" @click="showEditNameModal = false">İptal</button>
            <button class="save-btn" @click="updateLocationName">Kaydet</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Günlük Saat Düzenleme Modalı -->
    <EditDailySlotModal
      v-if="showDailyModal"
      :visible="showDailyModal"
      :location-id="selectedLocation.id"
      :day-of-week="selectedDay"
      :location-name="selectedLocation.name"
      @close="showDailyModal = false"
      @saved="fetchLocations"
    />

    <!-- Takvim Modalı -->
    <CalendarModal
      v-if="showCalendarModal"
      :visible="showCalendarModal"
      :location="selectedCalendarLocation"
      @close="showCalendarModal = false"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import EditDailySlotModal from './EditDailySlotModal.vue'
import CalendarModal from './CalendarModal.vue'

const showAddLocationModal = ref(false)
const newLocationName = ref('')
const locations = ref([])

const selectedDay = ref(null)
const selectedLocation = ref(null)
const showDailyModal = ref(false)
const showCalendarModal = ref(false)
const selectedCalendarLocation = ref(null)

const showEditNameModal = ref(false)
const editName = ref('')
const editingLocationId = ref(null)

const weekDays = ['Pazar', 'Pzt', 'Salı', 'Çarş', 'Perş', 'Cuma', 'Ctesi']

onMounted(fetchLocations)

async function fetchLocations() {
  try {
    const res = await axios.get('/api/eventreservation/locations-with-details')
    locations.value = res.data.map(l => ({
      id: l.id,
      name: l.name,
      validDays: l.validDays || [],
      disabledDates: l.disabledDates || [],
      dailySlots: l.dailySlots || [],
      newDisabledDate: ''
    }))
  } catch (error) {
    console.error('Tesisler alınamadı', error)
  }
}

function formatTime(time) {
  return time?.substring(0, 5) || '--:--'
}

function getSortedSlots(slots, day) {
  return slots
    .filter(s => s.dayOfWeek === day)
    .sort((a, b) => a.startHour.localeCompare(b.startHour))
}

function openDailyModal(location, dayIndex) {
  selectedLocation.value = location
  selectedDay.value = dayIndex
  showDailyModal.value = true
}

function openCalendar(location) {
  selectedCalendarLocation.value = location
  showCalendarModal.value = true
}

async function saveNewLocation() {
  if (!newLocationName.value.trim()) return
  try {
    await axios.post('/api/eventreservation/add-location', newLocationName.value, {
      headers: { 'Content-Type': 'application/json' }
    })
    showAddLocationModal.value = false
    newLocationName.value = ''
    fetchLocations()
  } catch (error) {
    console.error('Tesis kaydı başarısız:', error)
  }
}

function openEditNameModal(location) {
  editName.value = location.name
  editingLocationId.value = location.id
  showEditNameModal.value = true
}

async function updateLocationName() {
  try {
    await axios.post('/api/eventreservation/update-location-name', {
      id: editingLocationId.value,
      name: editName.value
    })
    showEditNameModal.value = false
    fetchLocations()
  } catch (error) {
    console.error('Tesis adı güncellenemedi:', error)
  }
}
</script>





<style scoped>

.settings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
.action-buttons {
  display: flex;
  gap: 0.75rem;
}
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  border-radius: 4px;
  padding: 0.5rem 1rem;
  cursor: pointer;
}
.btn-primary {
  background-color: #3498db;
  color: white;
}
.btn-primary:hover {
  background-color: #2980b9;
}
.btn-secondary {
  background-color: #2ecc71;
  color: white;
}
.btn-secondary:hover {
  background-color: #27ae60;
}
.facility-cards {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
.facility-card {
  background: #f9f9f9;
  border: 1px solid #ddd;
  border-radius: 6px;
  padding: 1rem;
}
.card-title {
  font-size: 1.2rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
}
.slot-form {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-top: 0.5rem;
}
.add-slot-btn {
  background: #2ecc71;
  color: white;
  padding: 0.3rem 0.75rem;
  border-radius: 4px;
  border: none;
  cursor: pointer;
}
.edit-btn {
  background: none;
  border: none;
  color: #3498db;
  cursor: pointer;
}
.slot-list ul,
.disabled-date-list {
  padding-left: 1rem;
  margin: 0.5rem 0;
}
.day-selector {
  margin: 1rem 0;
}
.checkbox-group {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.checkbox-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}
.modal-content {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  width: 100%;
  max-width: 480px;
}
.modal-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1rem;
}
.close-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
}
.modal-body {
  display: flex;
  flex-direction: column;
}
.form-label {
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: #2c3e50;
}
.form-input {
  padding: 0.6rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  margin-bottom: 1.5rem;
}
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}
.cancel-btn {
  background: #eee;
  border: none;
  padding: 0.6rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}
.save-btn {
  background: #2ecc71;
  color: white;
  border: none;
  padding: 0.6rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}
input[type="date"] {
  padding: 0.6rem 0.75rem;
  border: 1px solid #ccc;
  border-radius: 6px;
  background-color: white;
  font-size: 0.95rem;
  color: #2c3e50;
  transition: border 0.2s ease-in-out, box-shadow 0.2s ease-in-out;
}
input[type="date"]:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.3);
}
input[type="date"]::-webkit-calendar-picker-indicator {
  filter: invert(0.4);
  cursor: pointer;
}
.day-slot-list {
  margin-top: 0.5rem;
  padding-left: 0;
  list-style: none;
}

.edit-day-btn {
  background: #f39c12;
  color: white;
  font-size: 0.85rem;
  padding: 0.25rem 0.6rem;
  border-radius: 4px;
  border: none;
  cursor: pointer;
}

.edit-day-btn:hover {
  background: #e67e22;
}
.weekly-schedule {
  margin-top: 1rem;
}
.week-buttons {
  margin-top: 0.5rem;
  padding-left: 0;
}
.day-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.3rem 0;
  border-bottom: 1px dashed #ccc;
}
.day-edit-btn {
  background-color: #2ecc71;
  border: none;
  color: white;
  padding: 0.3rem 0.8rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
}
.facility-card {
  position: relative;
}

.calendar-icon {
  position: absolute;
  top: 1rem;
  right: 1rem;
}
.weekly-schedule {
  border-top: 1px dashed #ccc;
  padding-top: 1rem;
  margin-top: 1rem;
}

.day-block {
  margin-bottom: 1rem;
}

.day-header {
  display: flex;
  justify-content: space-between;
  font-weight: 600;
  margin-bottom: 0.25rem;
  color: #2c3e50;
}

.slot-item {
  margin-left: 1rem;
  font-size: 0.9rem;
  color: #34495e;
}
.edit-day-btn {
  background: none;
  border: none;
  color: #3498db;
  cursor: pointer;
  font-size: 0.85rem;
}



</style>
