<template>
  <div class="login-container">
    <div class="login-box">
      <h2>Giriş Yap</h2>
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
      <div v-if="showTwoFactorNotice" class="info-message">
        <i class="fas fa-shield-alt"></i> İki Faktörlü Doğrulama gerekiyor. Yönlendiriliyorsunuz...
      </div>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>E-posta:</label>
          <input type="email" v-model="email" required>
        </div>
        <div class="form-group">
          <label>Şifre:</label>
          <input type="password" v-model="password" required>
        </div>
        <div class="forgot-password">
          <router-link to="/forgot-password">Şifremi Unuttum</router-link>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}
        </button>
      </form>
      <div class="register-link">
        <p>Hesabınız yok mu? <router-link to="/register">Kayıt Ol</router-link></p>
      </div>
    </div>
  </div>
</template>

<script>
import { authService } from '@/services';

export default {
  name: 'Login',
  data() {
    return {
      email: '',
      password: '',
      loading: false,
      error: null,
      showTwoFactorNotice: false
    }
  },
  methods: {
    async handleLogin() {
      this.loading = true;
      this.error = null;

      try {
        const credentials = {
          email: this.email,
          password: this.password
        };

        const response = await authService.login(credentials);
        
        // 2FA kontrolü
        if (response.data.requiresTwoFactor) {
          this.loading = false;
          
          // Geçici token ve kullanıcı bilgilerini sakla
          authService.saveToken(response.data.tempToken);
          authService.saveUser(response.data.user);
          
          // 2FA doğrulama mesajını göster
          this.showTwoFactorNotice = true;
          
          // 3 saniye sonra 2FA doğrulama sayfasına yönlendir
          setTimeout(() => {
            this.$router.push({
              path: '/two-factor-verify',
              query: { userId: response.data.user.id }
            });
          }, 3000);
          
          return;
        }
        
        // Normal giriş akışı
        authService.saveToken(response.data.token);
        // userType'ı role olarak da ekle
        const userWithRole = {
          ...response.data.user,
          role: response.data.user.role || response.data.user.userType // role yoksa userType'ı kullan
        };
        authService.saveUser(userWithRole);
        
        const userType = userWithRole.role.toLowerCase();
        switch(userType) {
          case 'admin':
            this.$router.push('/admin/dashboard');
            break;
          case 'advisor':
            this.$router.push('/advisor/dashboard');
            break;
          case 'leader':
            this.$router.push('/leader/dashboard');
            break;
          case 'student':
            this.$router.push('/student/dashboard');
            break;
          case 'user':
            this.$router.push('/student/dashboard');
            break;
          default:
            this.error = 'Geçersiz kullanıcı tipi!';
            return;
        }
      } catch (error) {
        console.error('Giriş hatası:', error);
        if (error.response) {
          this.error = error.response.data.message || 'Geçersiz e-posta veya şifre!';
        } else {
          this.error = 'Giriş yapılırken bir hata oluştu!';
        }
      } finally {
        this.loading = false;
      }
    }
  },
  created() {
    // Kullanıcı zaten giriş yapmışsa, dashboard'a yönlendir
    if (authService.isLoggedIn()) {
      const user = authService.getUser();
      if (user) {
        const userType = user.role.toLowerCase();
        this.$router.push(`/${userType}/dashboard`);
      }
    }

    // Kayıt sayfasından gelmiş olabilecek bilgileri kontrol et
    const tempUser = JSON.parse(localStorage.getItem('tempUser'));
    if (tempUser) {
      this.email = tempUser.email;
      localStorage.removeItem('tempUser');
    }
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
}

.login-box {
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

.register-link {
  text-align: center;
  margin-top: 1rem;
}

.register-link p {
  font-size: 0.9rem;
  color: #666;
}

.register-link a {
  color: #3498db;
  text-decoration: none;
}

.register-link a:hover {
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

.info-message {
  background-color: #d1ecf1;
  color: #0c5460;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% { opacity: 0.8; }
  50% { opacity: 1; }
  100% { opacity: 0.8; }
}

.forgot-password {
  text-align: right;
  margin-bottom: 1rem;
}

.forgot-password a {
  color: #666;
  font-size: 0.9rem;
  text-decoration: none;
}

.forgot-password a:hover {
  text-decoration: underline;
}
</style>
