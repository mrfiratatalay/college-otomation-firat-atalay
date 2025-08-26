<template>
  <div class="register-container">
    <div class="register-box">
      <h2>Kayıt Ol</h2>

      <!-- Form: submit yalnızca valid ise tetiklenir, values parametresi gelir -->
      <Form @submit="onSubmit" v-slot="{ values }" class="form-container">
        <!-- Ad -->
        <Field name="ad" rules="required|min:2" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="name">Ad</label>
            <input
              v-bind="field"
              id="name"
              type="text"
              class="form-input"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
              autocomplete="given-name"
              placeholder="Adınız"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Soyad -->
        <Field name="soyad" rules="required|min:2" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="surname">Soyad</label>
            <input
              v-bind="field"
              id="surname"
              type="text"
              class="form-input"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
              autocomplete="family-name"
              placeholder="Soyadınız"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- E-posta -->
        <Field name="email" rules="required|email" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="email">E-posta</label>
            <input
              v-bind="field"
              id="email"
              type="email"
              class="form-input"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
              autocomplete="email"
              placeholder="ornek@domain.com"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Telefon -->
        <Field name="telefon" rules="required|min:10|max:15" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="phoneNumber">Telefon Numarası</label>
            <input
              v-bind="field"
              id="phoneNumber"
              type="tel"
              class="form-input"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
              inputmode="tel"
              autocomplete="tel"
              placeholder="5xx xxx xx xx"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Okul No -->
        <Field name="okulNo" rules="required|min:3" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="schoolNumber">Okul Numarası</label>
            <input
              v-bind="field"
              id="schoolNumber"
              type="text"
              class="form-input"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
              placeholder="Örn: 202012345"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Uyruk -->
        <Field name="uyruk" rules="required" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="nationality">Uyruk</label>
            <select
              v-bind="field"
              id="nationality"
              class="form-select"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
            >
              <option value="" disabled>Uyruk seçiniz</option>
              <option v-for="country in countries" :key="country.code" :value="country.code">
                {{ country.name }}
              </option>
            </select>
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Cinsiyet -->
        <Field name="cinsiyet" rules="required" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="gender">Cinsiyet</label>
            <select
              v-bind="field"
              id="gender"
              class="form-select"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
            >
              <option value="" disabled>Cinsiyet seçiniz</option>
              <option value="Erkek">Erkek</option>
              <option value="Kadın">Kadın</option>
            </select>
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Engelli Durumu -->
        <Field name="engelDurumu" rules="required" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="hasDisability">Engelli Durumu</label>
            <select
              v-bind="field"
              id="hasDisability"
              class="form-select"
              :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
            >
              <option value="Yok">Yok</option>
              <option value="Evet">Evet</option>
            </select>
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- Engellilik Açıklaması (Dinamik zorunluluk) -->
        <div v-if="values.engelDurumu === 'Evet'">
          <Field :rules="values.engelDurumu === 'Evet' ? 'required|min:3' : ''" name="engelAciklama" v-slot="{ field, errorMessage, meta }">
            <div class="form-group">
              <label for="disabilityDescription">Açıklama</label>
              <textarea
                v-bind="field"
                id="disabilityDescription"
                class="form-textarea"
                :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
                placeholder="Durumu kısaca açıklayın"
                rows="4"
              ></textarea>
              <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
            </div>
          </Field>
        </div>

        <button type="submit" class="btn btn-primary" :disabled="loading">
          <span v-if="loading" class="btn-spinner" aria-hidden="true"></span>
          <span>{{ loading ? 'Kayıt Yapılıyor...' : 'Kayıt Ol' }}</span>
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
import { Field, Form } from 'vee-validate';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';

const router = useRouter();
const toast = useToast();
const loading = ref(false);

// Form submit: vee-validate, valid olunca values ile çağırır
const onSubmit = async (values) => {
  loading.value = true;
  try {
    // Backend sözleşmesine map
    const userData = {
      Name: values.ad,
      Surname: values.soyad,
      Email: values.email,
      PhoneNumber: values.telefon,
      SchoolNumber: values.okulNo,
      Nationality: values.uyruk,
      Gender: values.cinsiyet,
      // Projendeki mevcut mantığı koruyorum:
      DisabilityStatus: values.engelDurumu === 'Evet' ? values.engelAciklama : 'Yok',
      // Eğer backend ayrı alan bekliyorsa:
      // DisabilityStatus: values.engelDurumu,
      // DisabilityDescription: values.engelDurumu === 'Evet' ? values.engelAciklama : ''
    };

    // Konsol kontrolü (isteğe bağlı)
    // console.log('Register Payload:', userData);

    const response = await authService.register(userData);

    if (response?.data?.success) {
      toast.success('Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz.');
      localStorage.setItem('tempUser', JSON.stringify({ email: values.email }));
      setTimeout(() => router.push('/login'), 2000);
    } else {
      // success yoksa da güvenli mesaj
      toast.error(response?.data?.message || 'Kayıt işleminde bir sorun oluştu.');
    }
  } catch (err) {
    console.error('Kayıt hatası:', err);
    const errorMessage = err?.response?.data?.message || 'Kayıt başarısız! Lütfen bilgilerinizi kontrol edin.';
    toast.error(errorMessage);
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
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 400px;
}
h2 {
  text-align: center;
  color: #2c3e50;
  margin-bottom: 1.75rem;
  font-weight: 600;
  letter-spacing: 0.2px;
}

.form-group { margin-bottom: 1.25rem; }
label { display: block; margin-bottom: 0.5rem; color: #555; font-size: 0.95rem; }

/* Inputs / Selects / Textarea: temayı bozma, ama odak halkası ekle */
.form-input, .form-select, .form-textarea, input, select, textarea {
  width: 100%;
  padding: 0.9rem 0.95rem;
  border: 1px solid #dcdfe4;
  border-radius: 10px;
  font-size: 1rem;
  outline: none;
  background-color: #fff;
  transition: border-color .2s, box-shadow .2s;
}
.form-input::placeholder, .form-textarea::placeholder { color: #9aa3af; }
.form-input:focus, .form-select:focus, .form-textarea:focus, input:focus, select:focus, textarea:focus {
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52,152,219,0.18);
}

/* Validasyon durumları */
.is-invalid { border-color: #e53935 !important; }
.is-invalid:focus { box-shadow: 0 0 0 3px rgba(229,57,53,0.18) !important; }
.is-valid { border-color: #34a853; }

.field-error {
  color: #d32f2f;
  font-size: 0.875em;
  margin-top: 6px;
}

/* Buton */
.btn {
  width: 100%;
  padding: 0.9rem;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-size: 1rem;
}
.btn-primary {
  background-color: #3498db;
  color: #fff;
  font-weight: 600;
  letter-spacing: 0.3px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  transition: background-color .2s, transform .08s ease-in-out;
}
.btn-primary:hover { background-color: #2980b9; }
.btn:active { transform: translateY(1px); }
.btn:disabled { opacity: 0.75; cursor: not-allowed; }

/* Spinner */
.btn-spinner {
  width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.6); border-top-color: #fff; border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.login-link { text-align: center; margin-top: 1rem; }
.login-link p { font-size: 0.9rem; color: #666; }
.login-link a { color: #3498db; text-decoration: none; }
.login-link a:hover { text-decoration: underline; }
</style>
