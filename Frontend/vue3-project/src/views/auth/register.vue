<template>
  <div class="register-container">
    <div class="register-box">
      <h2>Kayıt Ol</h2>
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
      <div v-if="success" class="success-message">
        {{ success }}
      </div>
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>Ad:</label>
          <input type="text" v-model="name" required>
        </div>
        <div class="form-group">
          <label>Soyad:</label>
          <input type="text" v-model="surname" required>
        </div>
        <div class="form-group">
          <label>E-posta:</label>
          <input type="email" v-model="email" required>
        </div>
        <div class="form-group">
          <label>Telefon Numarası:</label>
          <input type="tel" v-model="phoneNumber" required>
        </div>
        <div class="form-group">
          <label>Okul Numarası:</label>
          <input type="text" v-model="schoolNumber" required>
        </div>
        <div class="form-group">
          <label>Uyruk:</label>
          <select v-model="nationality" class="form-select" required>
            <option value="">Uyruk seçiniz</option>
            <option v-for="country in countries" :key="country.code" :value="country.code">
              {{ country.name }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Cinsiyet:</label>
          <select v-model="gender" class="form-select" required>
            <option value="">Cinsiyet seçiniz</option>
            <option value="Erkek">Erkek</option>
            <option value="Kadın">Kadın</option>
          </select>
        </div>
        <div class="form-group">
          <label>Engelli Durumu:</label>
          <select v-model="hasDisability" class="form-select" required>
            <option value="Yok">Yok</option>
            <option value="Evet">Evet</option>
          </select>
        </div>
        <div v-if="hasDisability === 'Evet'" class="form-group">
          <label>Açıklama:</label>
          <textarea v-model="disabilityDescription" required></textarea>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Kayıt Yapılıyor...' : 'Kayıt Ol' }}
        </button>
      </form>
      <div class="login-link">
        <p>Zaten hesabınız var mı? <router-link to="/login">Giriş Yap</router-link></p>
      </div>
    </div>
  </div>
</template>

<script>
import { authService } from '@/services';
import { countries } from '@/constants/countries';
export default {
  name: 'Register',
  data() {
    return {
      name: '',
      surname: '',
      email: '',
      phoneNumber: '',
      schoolNumber: '',
      nationality: '',
      gender: '',
      hasDisability: 'Yok',
      disabilityDescription: '',
      countries,
      loading: false,
      error: null,
      success: null,
    }
  },
  methods: {
    async handleRegister() {
      this.loading = true;
      this.error = null;
      this.success = null;

      try {
        const userData = {
          Name: this.name,
          Surname: this.surname,
          Email: this.email,
          PhoneNumber: this.phoneNumber,
          SchoolNumber: this.schoolNumber,
          Nationality: this.nationality,
          Gender: this.gender,
          DisabilityStatus: this.hasDisability === 'Evet' ? this.disabilityDescription : this.hasDisability,
        };

        console.log('Register Payload:', userData); // Log the payload

        const response = await authService.register(userData);

        if (response.data.success) {
          this.success = 'Kayıt başarılı! E-posta adresinize gönderilen şifre ile giriş yapabilirsiniz.';
          localStorage.setItem('tempUser', JSON.stringify({ email: this.email }));

          setTimeout(() => {
            this.$router.push('/login');
          }, 3000);
        }
      } catch (error) {
        console.error('Kayıt hatası:', error);
        if (error.response) {
          this.error = error.response.data.message || 'Kayıt işlemi başarısız oldu!';
        } else {
          this.error = 'Kayıt yapılırken bir hata oluştu!';
        }
      } finally {
        this.loading = false;
      }
    }
  }
}
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
}

.register-box {
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

textarea {
  width: 100%;
  padding: 0.8rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  min-height: 100px;
  resize: vertical;
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

.radio-group {
  display: flex;
  gap: 1.5rem;
  margin-top: 0.5rem;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.radio-label input {
  width: auto;
  margin: 0;
  cursor: pointer;
}
</style>
