<template>
  <div class="login-container">
    <div class="login-box">
      <h2>Giriş Yap</h2>

      <div v-if="showTwoFactorNotice" class="info-message">
        <i class="fas fa-shield-alt"></i> İki Faktörlü Doğrulama gerekiyor. Yönlendiriliyorsunuz...
      </div>

      <Form @submit="handleLogin">
        <div class="form-group">
          <label for="email">E-posta:</label>
          <Field name="email" type="email" v-model="email" rules="required|email" class="form-input" id="email" />
          <ErrorMessage name="email" class="field-error" />
        </div>

        <div class="form-group">
          <label for="password">Şifre:</label>
          <Field name="password" type="password" v-model="password" rules="required" class="form-input" id="password" />
          <ErrorMessage name="password" class="field-error" />
        </div>

        <div class="forgot-password">
          <router-link to="/forgot-password">Şifremi Unuttum</router-link>
        </div>

        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}
        </button>
      </Form>

      <div class="register-link">
        <p>Hesabınız yok mu? <router-link to="/register">Kayıt Ol</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { authService } from '@/services';
import { ErrorMessage, Field, Form } from 'vee-validate';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';

// Gerekli hook'ları tanımlıyoruz
const router = useRouter();
const toast = useToast();

// State (Durum) Değişkenleri
const email = ref('');
const password = ref('');
const loading = ref(false);
const showTwoFactorNotice = ref(false);

// Giriş işlemini yöneten fonksiyon
const handleLogin = async () => {
  loading.value = true;

  try {
    const credentials = {
      email: email.value,
      password: password.value
    };

    const response = await authService.login(credentials);

    // 2FA kontrolü
    if (response.data.requiresTwoFactor) {
      loading.value = false;
      authService.saveToken(response.data.tempToken);
      authService.saveUser(response.data.user);
      showTwoFactorNotice.value = true;
      setTimeout(() => {
        router.push({
          path: '/two-factor-verify',
          query: { userId: response.data.user.id }
        });
      }, 3000);
      return;
    }

    // Normal giriş akışı
    authService.saveToken(response.data.token);
    const userWithRole = {
      ...response.data.user,
      role: response.data.user.role || response.data.user.userType
    };
    authService.saveUser(userWithRole);

    const userType = userWithRole.role.toLowerCase();
    switch(userType) {
      case 'admin':
        router.push('/admin/dashboard');
        break;
      case 'advisor':
        router.push('/advisor/dashboard');
        break;
      case 'student':
      case 'user': // 'user' rolü de öğrenci paneline yönlendiriliyor
        router.push('/student/dashboard');
        break;
      default:
        // Bilinmeyen rol tipi için hata bildirimi
        toast.error('Geçersiz kullanıcı tipi!');
    }
  } catch (error) {
    console.error('Giriş hatası:', error);
    const errorMessage = error.response?.data?.message || 'Geçersiz e-posta veya şifre!';
    toast.error(errorMessage);
  } finally {
    loading.value = false;
  }
};

// Component yüklendiğinde çalışacak kod (created() hook'unun yeni hali)
onMounted(() => {
  // Kullanıcı zaten giriş yapmışsa yönlendirme mantığı (router guard'da olmasına rağmen ek bir kontrol)
  if (authService.isLoggedIn()) {
    const user = authService.getUser();
    if (user && user.role) {
      router.push(`/${user.role.toLowerCase()}/dashboard`);
    }
  }

  // Kayıt sayfasından gelinmişse e-postayı otomatik doldur
  const tempUser = JSON.parse(localStorage.getItem('tempUser'));
  if (tempUser) {
    email.value = tempUser.email;
    localStorage.removeItem('tempUser');
  }
});
</script>

<style scoped>
/* Stil kodları register.vue'den gelenlerle neredeyse aynı,
   .field-error gibi eklemelerle güncel tutulmalı */
.login-container { display: flex; justify-content: center; align-items: center; min-height: 100vh; background-color: #f5f5f5; }
.login-box { background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
h2 { text-align: center; color: #2c3e50; margin-bottom: 2rem; }
.form-group { margin-bottom: 1.5rem; }
label { display: block; margin-bottom: 0.5rem; color: #666; }
.form-input, input { width: 100%; padding: 0.8rem; border: 1px solid #ddd; border-radius: 4px; font-size: 1rem; }
.btn { width: 100%; padding: 0.8rem; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem; transition: background-color 0.2s; }
.btn-primary { background-color: #3498db; color: white; }
.btn-primary:hover { background-color: #2980b9; }
.btn:disabled { opacity: 0.7; cursor: not-allowed; }
.register-link { text-align: center; margin-top: 1rem; }
.register-link p { font-size: 0.9rem; color: #666; }
.register-link a { color: #3498db; text-decoration: none; }
.register-link a:hover { text-decoration: underline; }
.info-message { background-color: #d1ecf1; color: #0c5460; padding: 0.75rem; border-radius: 4px; margin-bottom: 1rem; text-align: center; display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
.forgot-password { text-align: right; margin-bottom: 1rem; }
.forgot-password a { color: #666; font-size: 0.9rem; text-decoration: none; }
.forgot-password a:hover { text-decoration: underline; }
.field-error { color: #d32f2f; font-size: 0.875em; margin-top: 6px; }
</style>
