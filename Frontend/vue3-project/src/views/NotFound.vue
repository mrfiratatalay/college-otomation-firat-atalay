<template>
  <div class="min-h-screen grid place-items-center p-8">
    <div class="max-w-md text-center">
      <h1 class="text-5xl font-bold mb-2">404</h1>
      <p class="mb-6">Aradığın sayfa bulunamadı.</p>
      <RouterLink :to="backHref" class="underline">Güvenli sayfaya dön</RouterLink>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const backHref = computed(() => {
  try {
    const u = JSON.parse(localStorage.getItem('user') || 'null')
    if (u?.role === 'admin') return '/admin/dashboard'
    if (u?.role === 'advisor') return '/advisor/dashboard'
    if (u?.role === 'user') return '/student/dashboard'
    return '/'
  } catch { return '/' }
})
</script>
