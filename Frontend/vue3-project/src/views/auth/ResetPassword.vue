<template>
  <div class="reset-password-container">
    <div class="reset-password-box">
      <h2>Yeni Şifre Oluştur</h2>
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
      <div v-if="success" class="success-message">
        {{ success }}
      </div>
      <form @submit.prevent="handleResetPassword" v-if="!success">
        <div class="form-group">
          <label>Yeni Şifre:</label>
          <input type="password" v-model="password" required>
        </div>
        <div class="form-group">
          <label>Şifre Tekrar:</label>
          <input type="password" v-model="confirmPassword" required>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'İşleniyor...' : 'Şifreyi Güncelle' }}
        </button>
      </form>
      <div class="login-link">
        <p v-if="success">
          <router-link to="/login">Giriş Yap</router-link>
        </p>
        <p v-else>
          Giriş sayfasına dön? <router-link to="/login">Giriş Yap</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import { authService } from '@/services';

export default {
  name: 'ResetPassword',
  data() {
    return {
      password: '',
      confirmPassword: '',
      loading: false,
      error: null,
      success: null
    }
  },
  methods: {
    async handleResetPassword() {
      if (this.password !== this.confirmPassword) {
        this.error = 'Şifreler eşleşmiyor!';
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        const email = this.$route.query.email;
        const token = this.$route.query.token;

        if (!email || !token) {
          this.error = 'Geçersiz şifre sıfırlama linki!';
          return;
        }

        await authService.resetPassword({
          email,
          token,
          newPassword: this.password
        });

        this.success = 'Şifreniz başarıyla güncellendi. Şimdi giriş yapabilirsiniz.';
      } catch (error) {
        this.error = error.response?.data?.message || 'Bir hata oluştu. Lütfen tekrar deneyin.';
      } finally {
        this.loading = false;
      }
    }
  },
  created() {
    // URL'den email ve token parametrelerini kontrol et
    const { email, token } = this.$route.query;
    if (!email || !token) {
      this.error = 'Geçersiz şifre sıfırlama linki!';
    }
  }
}
</script>

<style scoped>
.reset-password-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
}

.reset-password-box {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

h2 {
  text-align: center;
  color: #2c3e50;
  margin-bottom: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #666;
}

input {
  width: 100%;
  padding: 0.8rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.btn {
  width: 100%;
  padding: 0.8rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  transition: background-color 0.2s;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.login-link {
  text-align: center;
  margin-top: 1rem;
}

.login-link p {
  font-size: 0.9rem;
  color: #666;
}

.login-link a {
  color: #3498db;
  text-decoration: none;
}

.login-link a:hover {
  text-decoration: underline;
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
}

.success-message {
  background-color: #d4edda;
  color: #155724;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
}
</style>