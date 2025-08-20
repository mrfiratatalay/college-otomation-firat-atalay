<template>
  <div v-if="visible" class="modal">
    <div class="modal-content">
      <div class="modal-header">
        <h2>{{ locationName }} - {{ weekDays[dayOfWeek] }} Saat Ayarları</h2>
        <button class="close-btn" @click="$emit('close')">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <div class="slot-form">
          <input type="time" v-model="newSlotStart" />
          <span>-</span>
          <input type="time" v-model="newSlotEnd" />
          <button class="add-slot-btn" @click="addSlot">Ekle</button>
        </div>

        <ul class="slot-list">
          <li v-for="(slot, index) in slots" :key="index" class="slot-item">
            ⏰ {{ slot.startHour }} - {{ slot.endHour }}
            <i class="fas fa-trash-alt text-red-600 ml-2 cursor-pointer" @click="removeSlot(index)"></i>
          </li>
        </ul>
      </div>

      <div class="form-actions">
        <button class="cancel-btn" @click="$emit('close')">İptal</button>
        <button class="save-btn" @click="saveSlots">Kaydet</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, watchEffect } from 'vue'
import axios from 'axios'

const props = defineProps({
  visible: Boolean,
  locationId: Number,
  dayOfWeek: Number,
  locationName: String,
})
const emit = defineEmits(['close', 'saved'])

const newSlotStart = ref('')
const newSlotEnd = ref('')
const slots = ref([])
const weekDays = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz']

// Modal her açıldığında ve ilgili gün değiştiğinde slotları çek
watch(
  () => [props.visible, props.locationId, props.dayOfWeek],
  async ([visible]) => {
    if (visible) await fetchSlots()
  },
  { immediate: true }
)

async function fetchSlots() {
  try {
    const { data } = await axios.get(`/api/eventreservation/daily-slots/${props.locationId}/${props.dayOfWeek}`)
    slots.value = data
  } catch (err) {
    console.error('❌ Slotlar alınamadı:', err)
  }
}

function addSlot() {
  if (!newSlotStart.value || !newSlotEnd.value) return
  slots.value.push({
    startHour: newSlotStart.value,
    endHour: newSlotEnd.value
  })
  newSlotStart.value = ''
  newSlotEnd.value = ''
}

function removeSlot(index) {
  slots.value.splice(index, 1)
}

async function saveSlots() {
  try {
    console.log('✅ Kaydedilen slotlar:', slots.value)
    await axios.post('/api/eventreservation/update-daily-slots', {
      locationId: props.locationId,
      dayOfWeek: props.dayOfWeek,
      slots: slots.value
    })
    emit('saved')
    emit('close')
  } catch (err) {
    alert('⚠️ Kaydetme başarısız!')
    console.error(err)
  }
}
</script>


<style scoped>
.modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}
.modal-content {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  width: 100%;
  max-width: 500px;
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}
.modal-body {
  padding: 0.5rem 0;
}
.slot-form {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
.slot-list {
  margin-top: 1rem;
}
.slot-item {
  display: flex;
  justify-content: space-between;
  padding: 0.3rem 0;
}
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}
.add-slot-btn,
.cancel-btn,
.save-btn {
  padding: 0.4rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.add-slot-btn {
  background: #2ecc71;
  color: white;
}
.cancel-btn {
  background: #f0f0f0;
}
.save-btn {
  background: #3498db;
  color: white;
}
</style>
