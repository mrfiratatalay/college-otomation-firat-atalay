import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import Toast, { POSITION } from "vue-toastification"; // <-- Import with POSITION (or other options if needed)
import "vue-toastification/dist/index.css"; // CSS'i import et
import '@fortawesome/fontawesome-free/css/all.css'
import axios from 'axios'

//Vee Validate
import { defineRule, configure } from 'vee-validate';
import { required, email } from '@vee-validate/rules';
import { localize, setLocale } from '@vee-validate/i18n';
import tr from '@vee-validate/i18n/dist/locale/tr.json';

const app = createApp(App)


// VeeValidate Kurallarını Tanımla
defineRule('required', required);
defineRule('email', email);

// VeeValidate'i Türkçe ve anında kontrol edecek şekilde ayarla
configure({
  generateMessage: localize({ tr }),
  validateOnInput: true,
});
setLocale('tr');


// Define toast options (optional, but good practice)
const toastOptions = {
  position: POSITION.BOTTOM_RIGHT, // Example: Set default position
  timeout: 3000, // Example: Set default timeout (3 seconds)
  // You can add other default options here
};

// Axios'u global olarak tüm bileşenlerde $http olarak kullanabilmek için
app.config.globalProperties.$http = axios

app.use(router)
app.use(Toast, toastOptions); // <-- Pass options object
app.mount('#app')
