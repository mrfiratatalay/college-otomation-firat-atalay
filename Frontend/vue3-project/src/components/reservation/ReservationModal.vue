<template>
  <div class="reservation-tab">
    <!-- Etkinlik Seçimi -->
    <div class="form-group">
      <label>Etkinlik Seç</label>
      <select v-model="selectedEventId">
        <option disabled value="">Etkinlik seçiniz</option>
        <option v-for="e in events" :key="e.id" :value="e.id">
          {{ e.name }} ({{ e.startDate?.slice(0, 10) || '' }})
        </option>
      </select>
    </div>

    <!-- Tesis Seçimi -->
    <div class="form-group">
      <label>Tesis Seç</label>
      <select v-model="selectedFacilityId" @change="fetchSlots">
        <option disabled value="">Tesis seçiniz</option>
        <option v-for="f in facilities" :key="f.id" :value="f.id">
          {{ f.name }}
        </option>
      </select>
    </div>

    <!-- Tarih Seçimi -->
    <div class="form-group">
      <label>Tarih Seç</label>
      <input type="date" v-model="selectedDate" @change="fetchSlots" />
    </div>

    <!-- Saat Aralıkları -->
    <div class="form-group" v-if="slots.length > 0">
      <label>Saat Seç</label>
      <div
        v-for="slot in slots"
        :key="slot.id"
        class="slot-checkbox"
        :class="{ disabled: !slot.enabled }"
      >
        <input
          type="checkbox"
          :id="'slot-' + slot.id"
          :value="slot.id"
          v-model="selectedSlotIds"
          :disabled="!slot.enabled"
        />
        <label :for="'slot-' + slot.id">
          ✔️ {{ slot.startHour }} - {{ slot.endHour }}
          <span v-if="!slot.enabled" class="full">(Dolu)</span>
        </label>
      </div>
    </div>
    <div v-else-if="selectedFacilityId && selectedDate" class="text-muted">
      Bu gün için saat aralığı tanımlanmamış.
    </div>

    <!-- Rezervasyon Butonu -->
    <div class="form-actions">
      <button class="btn btn-primary" @click="submitReservation">
        Rezervasyon Yap
      </button>
      <span v-if="error" style="color: red; margin-left: 1rem">{{ error }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const facilities = ref([])
const events = ref([])
const selectedFacilityId = ref('')
const selectedDate = ref('')
const selectedSlotIds = ref([])
const selectedEventId = ref(null)
const slots = ref([])
const error = ref('')

onMounted(async () => {
  await fetchFacilities()
  await fetchUserEvents()
})

async function fetchFacilities() {
  try {
    const res = await axios.get('/api/eventreservation/locations-with-timeslots')
    facilities.value = res.data.map(f => ({ id: f.id, name: f.name }))
  } catch (err) {
    console.error('Tesisler alınamadı', err)
  }
}

async function fetchUserEvents() {
  try {
    const token = localStorage.getItem('token')
    const res = await axios.get('/api/events/listele', {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })

    console.log('Gelen etkinlik verisi:', res.data)

    // 🔧 eventId varsa onu id olarak normalize et
    events.value = res.data
      .filter(e => (e.id ?? e.eventId) != null)
      .map(e => ({
        ...e,
        id: Number(e.id ?? e.eventId)
      }))
  } catch (err) {
    console.error('Etkinlikler alınamadı', err)
  }
}

async function fetchSlots() {
  slots.value = []
  selectedSlotIds.value = []
  error.value = ''
  if (!selectedFacilityId.value || !selectedDate.value) return

  const dayOfWeek = new Date(selectedDate.value).getDay()

  try {
    const res = await axios.get(
      `/api/eventreservation/daily-slots/${selectedFacilityId.value}/${dayOfWeek}?date=${selectedDate.value}`
    )
    slots.value = res.data.map(s => ({
      id: s.id,
      startHour: s.startHour,
      endHour: s.endHour,
      enabled: s.enabled !== false
    }))
  } catch (err) {
    error.value = 'Saat aralıkları yüklenemedi.'
    slots.value = []
  }
}

async function submitReservation() {
  console.log('Seçilen etkinlik ID:', selectedEventId.value)
  console.log('Tesis ID:', selectedFacilityId.value)
  console.log('Tarih:', selectedDate.value)
  console.log('Saatler:', selectedSlotIds.value)

  if (
    !selectedEventId.value ||
    !selectedFacilityId.value ||
    !selectedDate.value ||
    selectedSlotIds.value.length === 0
  ) {
    error.value = 'Lütfen tüm alanları doldurun.'
    return
  }

  try {
    const requests = selectedSlotIds.value.map(slotId => {
      return axios.post('/api/eventreservation/reserve', {
        eventId: selectedEventId.value,
        locationId: selectedFacilityId.value,
        timeSlotId: slotId,
        reservationDate: selectedDate.value
      })
    })

    await Promise.all(requests)
    alert('Rezervasyon(lar) başarıyla oluşturuldu!')
    selectedFacilityId.value = ''
    selectedDate.value = ''
    selectedSlotIds.value = []
    selectedEventId.value = null
    slots.value = []
    error.value = ''
  } catch (err) {
  console.error('❌ Rezervasyon hatası:', err)
  error.value =
    err.response?.data?.details ||
    err.response?.data?.message ||
    'Rezervasyon başarısız oldu.'
}

}
</script>

<style scoped>
.reservation-tab {
  padding: 1rem;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}
.form-group {
  margin-bottom: 1.5rem;
}
.form-group label {
  display: block;
  font-weight: 500;
  margin-bottom: 0.5rem;
}
.form-group select,
.form-group input[type='date'] {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.slot-checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0.3rem 0;
}
.slot-checkbox input[type='checkbox'] {
  width: 16px;
  height: 16px;
  accent-color: #3498db;
  cursor: pointer;
}
.slot-checkbox.disabled {
  opacity: 0.5;
}
.full {
  color: red;
  margin-left: 0.5rem;
}
.form-actions {
  margin-top: 1rem;
}
.btn {
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
}
.btn-primary {
  background: #3498db;
  color: white;
}
.text-muted {
  font-size: 0.9rem;
  color: #888;
}
</style>
