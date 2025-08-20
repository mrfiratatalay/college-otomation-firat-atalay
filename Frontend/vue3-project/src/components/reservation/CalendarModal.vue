<template>
  <div class="modal">
    <div class="modal-content calendar-modal">
      <div class="modal-header">
        <h2>{{ location?.name }} Takvimi</h2>
        <button class="close-btn" @click="$emit('close')">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <!-- Tarih Seçimi -->
        <label class="form-label">Tarih Seç:</label>
        <input type="date" v-model="selectedDate" class="form-input" />

        <!-- Seçilen Gün ve Hafta Günü -->
        <div v-if="selectedDate" class="date-summary">
          📅 <strong>{{ selectedDate }}</strong> —
          <em>{{ getDayName(selectedDate) }}</em>
          <button @click="toggleFullDayDisabled" class="ml-4 text-red-600 hover:underline">
            ❌ Günü İptal Et
          </button>
        </div>

        <!-- Saat Aralıkları -->
        <ul class="slot-toggle-list" v-if="dailySlots.length > 0">
          <li v-for="(slot, index) in dailySlots" :key="index" class="slot-toggle-item">
            ⏰ {{ slot.startHour }} - {{ slot.endHour }}
            <input type="checkbox" v-model="slot.enabled" class="ml-2" />
          </li>
        </ul>

        <div v-else class="text-sm text-gray-600 mt-2">
          Bu gün için saat aralığı tanımlanmamış.
        </div>
      </div>

      <div class="modal-footer">
        <button class="cancel-btn" @click="$emit('close')">Kapat</button>
        <button class="save-btn" @click="saveChanges">Kaydet</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import axios from 'axios'

const props = defineProps({
  visible: Boolean,
  location: Object
})
const emit = defineEmits(['close'])

const selectedDate = ref('')
const dailySlots = ref([])

watch(selectedDate, async (newDate) => {
  if (newDate && props.location?.id) {
    await loadSlotsForDay(newDate)
  }
})

function getDayName(dateStr) {
  const date = new Date(dateStr)
  return date.toLocaleDateString('tr-TR', { weekday: 'long' })
}

async function loadSlotsForDay(dateStr) {
  const day = new Date(dateStr).getDay()
  const locationId = props.location?.id

  console.log("📅 Seçilen tarih:", dateStr)
  console.log("🗓️ Gün:", day)
  console.log("📍 Lokasyon ID:", locationId)

  try {
    const { data } = await axios.get(
      `/api/eventreservation/daily-slots/${locationId}/${day}?date=${dateStr}`
    )

    dailySlots.value = data.map(s => ({
      id: s.id,
      startHour: s.startHour,
      endHour: s.endHour,
      enabled: s.enabled !== false // Varsayılan true, ama gelen değeri dikkate al
    }))
  } catch (err) {
    console.error("❌ Slotlar yüklenemedi:", err)
  }
}


function toggleFullDayDisabled() {
  dailySlots.value.forEach(slot => (slot.enabled = false))
}

async function saveChanges() {
  try {
    const payload = {
      locationId: props.location.id,
      date: selectedDate.value,
      disabledSlotIds: dailySlots.value
        .filter(s => !s.enabled)
        .map(s => s.id)
    }

    console.log("📤 Gönderilen payload:", payload)

    await axios.post('/api/eventreservation/disable-daily-slots', payload)
    alert('Günlük saat ayarları kaydedildi!')
    emit('close')
  } catch (err) {
    alert('Hata oluştu.')
    console.error(err)
  }
}
</script>


<style scoped>
.calendar-modal {
  width: 100%;
  max-width: 600px;
}
.date-summary {
  margin: 1rem 0;
}
.slot-toggle-list {
  list-style: none;
  padding-left: 0;
  margin-top: 0.5rem;
}
.slot-toggle-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.4rem 0;
  border-bottom: 1px solid #eee;
}
.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}
</style>
