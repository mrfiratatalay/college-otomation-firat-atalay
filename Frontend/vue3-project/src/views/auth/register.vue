<template>
  <div class="register-container">
    <div class="register-box">
      <h2>Kayıt Ol</h2>



      <Form @submit="handleRegister" class="form-container">

        <div class="form-group">
          <label for="name">Ad:</label>
          <Field name="ad" type="text" v-model="name" rules="required" class="form-input" id="name" />
          <ErrorMessage name="ad" class="field-error" />
        </div>

        <div class="form-group">
          <label for="surname">Soyad:</label>
          <Field name="soyad" type="text" v-model="surname" rules="required" class="form-input" id="surname" />
          <ErrorMessage name="soyad" class="field-error" />
        </div>

        <div class="form-group">
          <label for="email">E-posta:</label>
          <Field name="email" type="email" v-model="email" rules="required|email" class="form-input" id="email" />
          <ErrorMessage name="email" class="field-error" />
        </div>

        <div class="form-group">
          <label for="phoneNumber">Telefon Numarası:</label>
          <Field name="telefon" type="tel" v-model="phoneNumber" rules="required" class="form-input" id="phoneNumber" />
          <ErrorMessage name="telefon" class="field-error" />
        </div>

        <div class="form-group">
          <label for="schoolNumber">Okul Numarası:</label>
          <Field name="okulNo" type="text" v-model="schoolNumber" rules="required" class="form-input" id="schoolNumber" />
          <ErrorMessage name="okulNo" class="field-error" />
        </div>

        <div class="form-group">
          <label for="nationality">Uyruk:</label>
          <Field name="uyruk" as="select" v-model="nationality" rules="required" class="form-select" id="nationality">
            <option value="" disabled>Uyruk seçiniz</option>
            <option v-for="country in countries" :key="country.code" :value="country.code">
              {{ country.name }}
            </option>
          </Field>
          <ErrorMessage name="uyruk" class="field-error" />
        </div>

        <div class="form-group">
          <label for="gender">Cinsiyet:</label>
          <Field name="cinsiyet" as="select" v-model="gender" rules="required" class="form-select" id="gender">
            <option value="" disabled>Cinsiyet seçiniz</option>
            <option value="Erkek">Erkek</option>
            <option value="Kadın">Kadın</option>
          </Field>
          <ErrorMessage name="cinsiyet" class="field-error" />
        </div>

        <div class="form-group">
          <label for="hasDisability">Engelli Durumu:</label>
          <Field name="engelDurumu" as="select" v-model="hasDisability" rules="required" class="form-select" id="hasDisability">
            <option value="Yok">Yok</option>
            <option value="Evet">Evet</option>
          </Field>
          <ErrorMessage name="engelDurumu" class="field-error" />
        </div>

        <div v-if="hasDisability === 'Evet'" class="form-group">
          <label for="disabilityDescription">Açıklama:</label>
          <Field name="engelAciklama" as="textarea" v-model="disabilityDescription" rules="required" class="form-textarea" id="disabilityDescription" />
          <ErrorMessage name="engelAciklama" class="field-error" />
        </div>

        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Kayıt Yapılıyor...' : 'Kayıt Ol' }}
        </button>
      </Form>

      <div class="login-link">
        <p>Zaten hesabınız var mı? <router-link to="/login">Giriş Yap</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { countries } from '@/constants/countries';
import { authService } from '@/services';
import { ErrorMessage, Field, Form } from 'vee-validate';
import { useToast } from 'vue-toastification'; // <-- BU SATIRI EKLE

import { ref } from 'vue';
import { useRouter } from 'vue-router';
// useRouter hook'u ile router'a erişim
const router = useRouter();
const toast = useToast(); // <-- VE BUNU EKLE

// Options API'deki data() yerine ref() kullanıyoruz
const name = ref('');
const surname = ref('');
const email = ref('');
const phoneNumber = ref('');
const schoolNumber = ref('');
const nationality = ref('');
const gender = ref('');
const hasDisability = ref('Yok');
const disabilityDescription = ref('');
const loading = ref(false);


// methods objesi yerine standart bir fonksiyon tanımı
const handleRegister = async () => {
  loading.value = true;

  try {
    const userData = {
      Name: name.value,
      Surname: surname.value,
      Email: email.value,
      PhoneNumber: phoneNumber.value,
      SchoolNumber: schoolNumber.value,
      Nationality: nationality.value,
      Gender: gender.value,
      DisabilityStatus: hasDisability.value === 'Evet' ? disabilityDescription.value : hasDisability.value,
    };

    console.log('Register Payload:', userData);

    const response = await authService.register(userData);

    if (response.data.success) {
      toast.success('Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz.'); // toast.success kullan
      localStorage.setItem('tempUser', JSON.stringify({ email: email.value }));

      setTimeout(() => {
        router.push('/login');
      }, 3000);
    }
  } catch (err) {
    console.error('Kayıt hatası:', err);
    // YENİ HALİ:
    const errorMessage = err.response?.data?.message || 'Kayıt işlemi başarısız oldu! Lütfen bilgilerinizi kontrol edin.';
    toast.error(errorMessage); // toast.error kullan
  } finally {
    loading.value = false;
  }
};
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
.field-error {
  color: #d32f2f;
  font-size: 0.875em;
  margin-top: 6px;
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
