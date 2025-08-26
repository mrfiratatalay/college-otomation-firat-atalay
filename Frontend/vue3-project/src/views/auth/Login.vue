<template>
  <div class="login-container">
    <div class="login-box">
      <h2>Giriş Yap</h2>

      <div v-if="showTwoFactorNotice" class="info-message">
        <i class="fas fa-shield-alt"></i> İki Faktörlü Doğrulama gerekiyor. Yönlendiriliyorsunuz...
      </div>

      <!-- Form: useForm yok; submit doğrudan onSubmit -->
      <Form @submit="onSubmit" :initial-values="initialValues">
        <!-- E-POSTA -->
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
              autofocus
              placeholder="ornek@domain.com"
            />
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <!-- ŞİFRE -->
        <Field name="password" rules="required" v-slot="{ field, errorMessage, meta }">
          <div class="form-group">
            <label for="password">Şifre</label>
            <div class="input-with-addon">
              <input
                v-bind="field"
                :type="showPassword ? 'text' : 'password'"
                id="password"
                class="form-input"
                :class="{'is-invalid': !!errorMessage, 'is-valid': meta.valid && meta.touched}"
                autocomplete="current-password"
                enterkeyhint="go"
                placeholder="••••••••"
              />
              <button type="button" class="addon-btn" @click="showPassword = !showPassword" :aria-pressed="showPassword">
                <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'" aria-hidden="true"></i>
                <span class="sr-only">{{ showPassword ? 'Şifreyi gizle' : 'Şifreyi göster' }}</span>
              </button>
            </div>
            <span v-if="errorMessage" class="field-error" aria-live="polite">{{ errorMessage }}</span>
          </div>
        </Field>

        <div class="forgot-password">
          <router-link to="/forgot-password">Şifremi Unuttum</router-link>
        </div>

        <button type="submit" class="btn btn-primary" :disabled="loading">
          <span v-if="loading" class="btn-spinner" aria-hidden="true"></span>
          <span>{{ loading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}</span>
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
import { Field, Form } from 'vee-validate';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';

const router = useRouter();
const toast = useToast();

const loading = ref(false);
const showTwoFactorNotice = ref(false);
const showPassword = ref(false);

// Form başlangıç değerleri (reactive)
const initialValues = ref({
  email: '',
  password: ''
});

// Submit handler: Form values parametre olarak gelir
const onSubmit = async (values) => {
  loading.value = true;
  try {
    const credentials = {
      email: values.email,
      password: values.password
    };

    const response = await authService.login(credentials);

    // 2FA kontrolü
    if (response?.data?.requiresTwoFactor) {
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

    const userType = String(userWithRole.role || '').toLowerCase();
    switch (userType) {
      case 'admin':
        router.push('/admin/dashboard');
        break;
      case 'advisor':
        router.push('/advisor/dashboard');
        break;
      case 'student':
      case 'user':
        router.push('/student/dashboard');
        break;
      default:
        toast.error('Geçersiz kullanıcı tipi!');
    }
  } catch (error) {
    console.error('Giriş hatası:', error);
    const errorMessage = error?.response?.data?.message || 'Geçersiz e-posta veya şifre!';
    toast.error(errorMessage);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  // Zaten giriş yaptıysa rolüne göre yönlendir
  if (authService.isLoggedIn()) {
    const user = authService.getUser();
    if (user && user.role) {
      router.push(`/${String(user.role).toLowerCase()}/dashboard`);
      return;
    }
  }

  // Kayıt sayfasından gelen geçici e-posta varsa doldur
  const tempUser = JSON.parse(localStorage.getItem('tempUser') || 'null');
  if (tempUser?.email) {
    initialValues.value.email = tempUser.email; // initial-values reactive olduğu için formu doldurur
    localStorage.removeItem('tempUser');
  }
});
</script>

<style scoped>
/* Konteyner & Kutu */
.login-container {
  display: flex; justify-content: center; align-items: center;
  min-height: 100vh; background-color: #f5f5f5;
}
.login-box {
  background: #fff; padding: 2rem; border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.08);
  width: 100%; max-width: 400px;
}

/* Başlık */
h2 {
  text-align: center; color: #2c3e50; margin-bottom: 1.75rem;
  font-weight: 600; letter-spacing: 0.2px;
}

/* Form & Etiketler */
.form-group { margin-bottom: 1.25rem; }
label { display: block; margin-bottom: 0.5rem; color: #555; font-size: 0.95rem; }

/* Input */
.form-input, input {
  width: 100%; padding: 0.9rem 0.95rem; border: 1px solid #dcdfe4; border-radius: 10px;
  font-size: 1rem; outline: none; transition: border-color .2s, box-shadow .2s, background-color .2s;
  background-color: #fff;
}
.form-input::placeholder { color: #9aa3af; }
.form-input:focus {
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52,152,219,0.18);
}

/* Hata & Başarı durumu */
.is-invalid { border-color: #e53935 !important; }
.is-invalid:focus { box-shadow: 0 0 0 3px rgba(229,57,53,0.18) !important; }
.is-valid { border-color: #34a853; }

/* Şifre göster/gizle eklentisi */
.input-with-addon { position: relative; }
.addon-btn {
  position: absolute; right: 8px; top: 50%; transform: translateY(-50%);
  border: 0; background: transparent; padding: 6px 8px; cursor: pointer;
  color: #6b7280; border-radius: 8px; transition: background-color .2s, color .2s;
}
.addon-btn:hover { background-color: rgba(0,0,0,0.04); color: #374151; }

/* Linkler */
.forgot-password { text-align: right; margin-bottom: 1rem; }
.forgot-password a { color: #3498db; font-size: 0.9rem; text-decoration: none; }
.forgot-password a:hover { text-decoration: underline; }

.register-link { text-align: center; margin-top: 1rem; }
.register-link p { font-size: 0.9rem; color: #666; }
.register-link a { color: #3498db; text-decoration: none; }
.register-link a:hover { text-decoration: underline; }

/* Bilgi mesajı */
.info-message {
  background-color: #e8f4fb; color: #0b3558; padding: 0.75rem; border-radius: 10px;
  margin-bottom: 1rem; text-align: center; display: flex; align-items: center; justify-content: center; gap: 0.5rem;
  border: 1px solid #cfe7fa;
}

/* Hata metni */
.field-error { color: #d32f2f; font-size: 0.85rem; margin-top: 6px; }

/* Buton & Spinner */
.btn { width: 100%; padding: 0.9rem; border: none; border-radius: 10px; cursor: pointer; font-size: 1rem; }
.btn-primary {
  background-color: #3498db; color: #fff; font-weight: 600; letter-spacing: 0.3px;
  display: inline-flex; align-items: center; justify-content: center; gap: 10px;
  transition: background-color .2s, transform .08s ease-in-out;
}
.btn-primary:hover { background-color: #2980b9; }
.btn:active { transform: translateY(1px); }
.btn:disabled { opacity: 0.75; cursor: not-allowed; }

.btn-spinner {
  width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.6); border-top-color: #fff; border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* Erişilebilirlik yardımcı sınıfı */
.sr-only {
  position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden;
  clip: rect(0,0,0,0); white-space: nowrap; border: 0;
}
</style>
