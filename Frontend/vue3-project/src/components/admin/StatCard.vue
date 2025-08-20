<template>
  <div class="stat-card">
    <div class="stat-icon">
      <i :class="icon"></i>
    </div>
    <div class="stat-info">
      <h3>{{ title }}</h3>
      <p>{{ formattedValue }}</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'StatCard',
  props: {
    icon: {
      type: String,
      required: true
    },
    title: {
      type: String,
      required: true
    },
    value: {
      type: [Number, String],
      default: 0
    }
  },
  computed: {
    formattedValue() {
      // Optional: Add formatting like thousands separator if needed
      return this.value || 0;
    }
  }
};
</script>

<style scoped>
.stat-card {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  gap: 15px;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
}

.stat-icon {
  font-size: 1.8rem; /* Slightly smaller icon */
  color: #007bff; /* Primary blue */
  background-color: rgba(0, 123, 255, 0.1); /* Light blue background */
  width: 50px;
  height: 50px;
  min-width: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-info h3 {
  margin: 0 0 5px 0;
  font-size: 0.9rem;
  color: #6c757d; /* Muted color for title */
  font-weight: 500;
}

.stat-info p {
  margin: 0;
  font-size: 1.6rem;
  font-weight: 600;
  color: #343a40; /* Darker color for value */
}
</style> 