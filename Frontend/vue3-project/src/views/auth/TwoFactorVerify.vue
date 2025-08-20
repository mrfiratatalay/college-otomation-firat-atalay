<template>
  <div class="two-factor-verify-container">
    <div class="two-factor-verify-box">
      <h2>İki Faktörlü Doğrulama</h2>
      <div v-if="error" class="error-message">
        {{ error }}
      </div>

      <div class="auth-info">
        <i class="fas fa-shield-alt shield-icon"></i>
        <p class="instruction">Lütfen kimlik doğrulama uygulamanızdan 6 haneli kodu girin.</p>
      </div>

      <form @submit.prevent="handleVerify" class="verify-form">
        <div class="form-group">
          <label>Doğrulama Kodu:</label>
          <div class="code-input-container">
            <input type="text" v-model="code" maxlength="6" required pattern="[0-9]*" ref="codeInput" placeholder="******">
            <div class="countdown">{{ countdownText }}</div>
          </div>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Doğrulanıyor...' : 'Doğrula ve Giriş Yap' }}
        </button>
      </form>

      <div class="cancel-link">
        <a href="#" @click.prevent="cancelLogin">İptal Et ve Giriş Sayfasına Dön</a>
      </div>
    </div>
  </div>
</template>

<script>
import { authService } from '@/services';

export default {
  name: 'TwoFactorVerify',
  data() {
    return {
      code: '',
      loading: false,
      error: null,
      userId: null,
      countdown: 30,
      countdownInterval: null
    };
  },
  computed: {
    countdownText() {
      return this.countdown > 0 ? `${this.countdown}s` : '0s';
    }
  },
  methods: {
    async handleVerify() {
      if (!/^\d{6}$/.test(this.code)) {
        this.error = '6 haneli bir kod giriniz.';
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        const response = await authService.validate2FA({
          userId: this.userId,
          code: this.code
        });

        // 2FA doğrulaması başarılı, kullanıcıyı yönlendir
        const user = authService.getUser();
        const userType = user?.userType?.toLowerCase();
        
        if (userType) {
          this.$router.push(`/${userType}/dashboard`);
        } else {
          throw new Error('Kullanıcı tipi bulunamadı');
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Doğrulama kodu geçersiz.';
        this.code = ''; // Kodu temizle
        this.$refs.codeInput.focus(); // Input'a odaklan
      } finally {
        this.loading = false;
      }
    },
    cancelLogin() {
      // Kullanıcı bilgilerini temizle ve login sayfasına dön
      authService.logout();
      this.$router.push('/login');
    },
    startCountdown() {
      this.countdownInterval = setInterval(() => {
        if (this.countdown > 0) {
          this.countdown--;
        } else {
          clearInterval(this.countdownInterval);
        }
      }, 1000);
    }
  },
  created() {
    // URL'den userId'yi al
    const userId = this.$route.query.userId;
    if (!userId) {
      this.error = 'Geçersiz doğrulama isteği';
      setTimeout(() => {
        this.$router.push('/login');
      }, 2000);
      return;
    }
    this.userId = userId;
  },
  mounted() {
    // Component yüklendiğinde input'a odaklan
    this.$refs.codeInput.focus();
    
    // Geri sayımı başlat
    this.startCountdown();
  },
  beforeUnmount() {
    // Component kaldırıldığında interval'i temizle
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }
};
</script>

<style scoped>
.two-factor-verify-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
  padding: 1rem;
}

.two-factor-verify-box {
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
  margin-bottom: 1.5rem;
}

.auth-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 2rem;
}

.shield-icon {
  font-size: 3rem;
  color: #3498db;
  margin-bottom: 1rem;
}

.instruction {
  text-align: center;
  color: #666;
  margin-bottom: 1rem;
}

.verify-form {
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #666;
}

.code-input-container {
  position: relative;
}

input {
  width: 100%;
  padding: 0.8rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1.2rem;
  text-align: center;
  letter-spacing: 0.2rem;
}

.countdown {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  background-color: #f8f9fa;
  color: #666;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
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

.cancel-link {
  text-align: center;
  margin-top: 1rem;
}

.cancel-link a {
  color: #666;
  text-decoration: none;
  font-size: 0.9rem;
}

.cancel-link a:hover {
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
</style>