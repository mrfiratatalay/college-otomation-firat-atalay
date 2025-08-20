import axios from 'axios';
import router from '@/router';
import authService from './authService';

// API'nin temel URL'sini oluşturma
const api = axios.create({
  baseURL: 'https://localhost:7274/api', // API server URL with correct path (matches Swagger)
  headers: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  },
  timeout: 10000, // Adding timeout for better error handling
  withCredentials: false // CORS sorunlarını önlemek için
});

// FormData içeren istekler için interceptor ekleyin
api.interceptors.request.use(config => {
  if (config.data instanceof FormData) {
    // Remove the default Content-Type header when sending FormData
    delete config.headers['Content-Type'];
  }
  return config;
});

// İstek intercept edici - token eklemek için
api.interceptors.request.use(
  config => {
    console.log('API Request:', config.method.toUpperCase(), config.baseURL + config.url, config.data || '');

    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }

    return config;
  },
  error => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Yanıt intercept edici - token süresi dolma vb. durumlar için
api.interceptors.response.use(
  response => {
    // Başarılı yanıtı log'la
    console.log('API Response:', response.status, 'from', response.config.url.replace(response.config.baseURL, ''), response.data);
    return response;
  },
  error => {
    // Hata durumları
    if (error.response) {
      // Serverdan cevap gelmiş ama status 2xx değil
      console.error('API Error Response:', error.response.status, 'from', error.config.url.replace(error.config.baseURL, ''),
        typeof error.response.data === 'string'
          ? `"${error.response.data}"`
          : error.response.data
      );

      // Token süresi dolmuşsa veya geçersizse
      if (error.response.status === 401) {
        // Login endpoint için özel davranış - token ile ilgili işlem yapmayız
        if (error.config.url === '/auth/login') {
          // Login hatası component içinde ele alınacak
          console.warn('Giriş başarısız: E-posta veya şifre hatalı.');
          return Promise.reject(error);
        }
        
        // Diğer 401 hataları için token süresi dolmuş olarak değerlendir
        console.warn('Oturum süresi dolmuş veya geçersiz token. Çıkış yapılıyor...');
        authService.logout();
        router.push('/login');
        return Promise.reject(new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.'));
      }

      // 403 - Yetkisiz erişim
      if (error.response.status === 403) {
        console.warn('Yetkisiz erişim.');
        router.push('/forbidden');
        return Promise.reject(new Error('Bu sayfaya erişim yetkiniz bulunmuyor.'));
      }

      // 404 - Sayfa bulunamadı
      if (error.response.status === 404) {
        console.warn('İstenen kaynak bulunamadı.');
        // Özel bir işlem yapılmayacak, hatayı ileten component'ta halledilecek
      }

      // 400 - Kötü istek (doğrulama hataları vb.)
      if (error.response.status === 400) {
        console.warn('Geçersiz istek:', error.response.data);
        // Hatalar component seviyesinde işlenecek
      }

      // 500 - Sunucu hatası
      if (error.response.status >= 500) {
        console.error('Sunucu hatası:', error.response.status, error.response.data);
        // Kritik hataları global olarak gösterebiliriz
      }
    } else if (error.request) {
      // İstek yapıldı ama cevap alınamadı (bağlantı hatası)
      console.error('Sunucu yanıt vermiyor. Bağlantı hatası:', error.request);
    } else {
      // İstek oluşturulurken bir şeyler yanlış gitti
      console.error('İstek hatası:', error.message);
    }

    return Promise.reject(error);
  }
);

const handleDocumentUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    newExpense.value.documentFile = file;
    newExpense.value.documentName = file.name;
  }
};

export default api;