import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import Toast, { POSITION } from "vue-toastification"; // <-- Import with POSITION (or other options if needed)
import "vue-toastification/dist/index.css"; // CSS'i import et
import '@fortawesome/fontawesome-free/css/all.css'
import axios from 'axios'

const app = createApp(App)

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
