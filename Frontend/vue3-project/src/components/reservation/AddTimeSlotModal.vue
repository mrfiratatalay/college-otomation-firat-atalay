<template>
  <div v-if="visible" class="modal">
    <div class="modal-content">
      <div class="modal-header">
        <h2>Yeni Saat Aralığı Ekle</h2>
        <button @click="$emit('close')" class="close-btn">
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="modal-body">
        <!-- Tesis Seçimi -->
        <div class="form-group">
          <label>Tesis Seç</label>
          <select v-model="form.locationId">
            <option disabled value="">Tesis seçiniz</option>
            <option v-for="location in locations" :key="location.id" :value="location.id">
              {{ location.name }}
            </option>
          </select>
        </div>

        <!-- Saat Aralığı -->
        <div class="form-row">
          <div class="form-group">
            <label>Başlangıç Saati</label>
            <input type="time" v-model="form.startHour" />
          </div>
          <div class="form-group">
            <label>Bitiş Saati</label>
            <input type="time" v-model="form.endHour" />
          </div>
        </div>

        <!-- Haftanın Günleri -->
        <div class="form-group">
          <label>Geçerli Günler</label>
          <div class="weekday-checkboxes">
            <label v-for="day in daysOfWeek" :key="day">
              <input type="checkbox" v-model="form.days" :value="day" />
              {{ day }}
            </label>
          </div>
        </div>

        <!-- Butonlar -->
        <div class="form-actions">
          <button class="cancel-btn" @click="$emit('close')">İptal</button>
          <button class="save-btn" @click="submit">Kaydet</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const emit = defineEmits(['close', 'success'])
const props = defineProps({ visible: Boolean })

const form = ref({
  locationId: '',
  startHour: '',
  endHour: '',
  days: []
})

const locations = ref([])
const daysOfWeek = ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar']

onMounted(async () => {
  const res = await axios.get('/api/eventreservation/locations-with-timeslots')
  locations.value = res.data.map(l => ({ id: l.id, name: l.name }))
})

async function submit() {
  try {
    const payload = {
      locationId: parseInt(form.value.locationId),
      startHour: form.value.startHour,
      endHour: form.value.endHour
      // Gün bilgisi ileride eklenecekse ayrıca eklenebilir
    }
    await axios.post('/api/eventreservation/add-timeslot', payload)
    emit('success')
    emit('close')
  } catch (err) {
    console.error('Saat aralığı eklenemedi', err)
  }
}
</script>

<style scoped>
.modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 100%;
  max-width: 500px;
  padding: 1.5rem;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.modal-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c3e50;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  color: #7f8c8d;
  cursor: pointer;
}

.modal-body {
  padding: 0.5rem 0;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 500;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.weekday-checkboxes {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.95rem;
  color: #2c3e50;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.cancel-btn,
.save-btn {
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 4px;
  font-weight: 500;
  cursor: pointer;
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
</style>
