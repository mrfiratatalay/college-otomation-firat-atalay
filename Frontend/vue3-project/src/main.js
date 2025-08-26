import '@fortawesome/fontawesome-free/css/all.css'
import axios from 'axios'
import { createApp } from 'vue'
import Toast, { POSITION } from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import App from './App.vue'
import router from './router'

// vee-validate
import { localize, setLocale } from '@vee-validate/i18n'
import tr from '@vee-validate/i18n/dist/locale/tr.json'
import { email, max, min, required } from '@vee-validate/rules'
import { configure, defineRule } from 'vee-validate'

const app = createApp(App)

// Kuralları kaydet (kullandıkların mutlaka burada olmalı)
defineRule('required', required)
defineRule('email', email)
defineRule('min', min)
defineRule('max', max)
// İleride alpha/numeric/regex vs. kullanırsan, burada defineRule ile ekle.

// Mesajlar + yazarken doğrulama
configure({
  generateMessage: localize({ tr }),
  validateOnInput: true,
})
setLocale('tr')

// Toast varsayılanları (opsiyonel)
const toastOptions = {
  position: POSITION.BOTTOM_RIGHT,
  timeout: 3000,
}

// Axios’u global (opsiyonel)
app.config.globalProperties.$http = axios

app.use(router)
app.use(Toast, toastOptions)
app.mount('#app')
