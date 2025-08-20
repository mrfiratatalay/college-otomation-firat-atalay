<template>
  <div class="home-container">
    <header class="hero">
      <div class="hero-content">
        <h1>Topluluk Yönetim Sistemi</h1>
        <p>Öğrenci topluluklarınızı yönetmek ve etkinliklerinize katılmak için en iyi platform</p>
        <div class="hero-buttons">
          <router-link to="/login" class="btn btn-primary">Giriş Yap</router-link>
          <a @click="goToRegister" class="btn btn-secondary">Kayıt Ol</a>
        </div>
      </div>
    </header>

    <section class="features">
      <h2>Özellikler</h2>
      <div class="features-grid">
        <div class="feature-card">
          <i class="fas fa-users"></i>
          <h3>Topluluk Yönetimi</h3>
          <p>Toplulukları kolayca oluşturun, yönetin ve yeni üyelere ulaşın.</p>
        </div>
        <div class="feature-card">
          <i class="fas fa-calendar-alt"></i>
          <h3>Etkinlik Organizasyonu</h3>
          <p>Etkinliklerinizi planlayın, yönetin ve katılımcılarla iletişim kurun.</p>
        </div>
        <div class="feature-card">
          <i class="fas fa-bullhorn"></i>
          <h3>Duyuru Yönetimi</h3>
          <p>Duyurularınızı tüm üyelere anında ulaştırın ve geri bildirim alın.</p>
        </div>
        <div class="feature-card">
          <i class="fas fa-chart-line"></i>
          <h3>İstatistikler ve Raporlar</h3>
          <p>Topluluk ve etkinlik istatistiklerinizi analiz edin, başarınızı ölçün.</p>
        </div>
      </div>
    </section>

    <section class="how-it-works">
      <h2>Nasıl Çalışır?</h2>
      <div class="steps">
        <div class="step">
          <div class="step-number">1</div>
          <h3>Hesap Oluşturun</h3>
          <p>Hızlı ve kolay bir şekilde hesap oluşturun ve sisteme giriş yapın.</p>
        </div>
        <div class="step">
          <div class="step-number">2</div>
          <h3>Topluluklara Katılın</h3>
          <p>İlgi alanlarınıza uygun topluluklara katılın veya kendi topluluğunuzu kurun.</p>
        </div>
        <div class="step">
          <div class="step-number">3</div>
          <h3>Etkinliklere Katılın</h3>
          <p>Toplulukların düzenlediği etkinliklere katılın ve sosyal ağınızı genişletin.</p>
        </div>
      </div>
    </section>

    <footer class="footer">
      <div class="footer-content">
        <p>&copy; 2024 Topluluk Yönetim Sistemi. Tüm hakları saklıdır.</p>
        <div class="footer-links">
          <a href="#" class="footer-link">Hakkımızda</a>
          <a href="#" class="footer-link">Gizlilik Politikası</a>
          <a href="#" class="footer-link">İletişim</a>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
export default {
  name: 'Home',
  methods: {
    goToRegister() {
      this.$router.push('/register')
    }
  },
  created() {
    // Eğer kullanıcı giriş yapmışsa, otomatik olarak dashboard'a yönlendir
    const user = JSON.parse(localStorage.getItem('user'))
    if (user) {
      if (user.role === 'admin') {
        this.$router.push('/admin/dashboard')
      } else {
        this.$router.push('/student/dashboard')
      }
    }
  }
}
</script>

<style scoped>
.home-container {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.hero {
  background: linear-gradient(rgba(0, 0, 0, 0.7), rgba(0, 0, 0, 0.7)), url('https://via.placeholder.com/1920x1080');
  background-size: cover;
  background-position: center;
  color: white;
  padding: 100px 20px;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero h1 {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.hero p {
  font-size: 1.2rem;
  margin-bottom: 2rem;
}

.hero-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn {
  padding: 0.8rem 2rem;
  border-radius: 4px;
  font-size: 1rem;
  text-decoration: none;
  transition: all 0.3s ease;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.btn-secondary {
  background-color: transparent;
  color: white;
  border: 1px solid white;
  cursor: pointer;
}

.btn-secondary:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.features, .how-it-works {
  padding: 80px 20px;
  text-align: center;
}

.features h2, .how-it-works h2 {
  margin-bottom: 3rem;
  font-size: 2rem;
  color: #2c3e50;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.feature-card {
  background-color: white;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-10px);
}

.feature-card i {
  font-size: 3rem;
  color: #3498db;
  margin-bottom: 1rem;
}

.feature-card h3 {
  margin-bottom: 1rem;
  color: #2c3e50;
}

.steps {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.step {
  flex: 1;
  min-width: 250px;
  max-width: 350px;
  text-align: center;
}

.step-number {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 50px;
  height: 50px;
  background-color: #3498db;
  color: white;
  border-radius: 50%;
  font-size: 1.5rem;
  font-weight: bold;
  margin: 0 auto 1rem;
}

.footer {
  background-color: #2c3e50;
  color: white;
  padding: 30px 20px;
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.footer-links {
  display: flex;
  gap: 1.5rem;
}

.footer-link {
  color: white;
  text-decoration: none;
}

.footer-link:hover {
  text-decoration: underline;
}

@media (max-width: 768px) {
  .hero h1 {
    font-size: 2rem;
  }
  
  .hero-buttons {
    flex-direction: column;
    align-items: center;
  }
  
  .footer-content {
    flex-direction: column;
    text-align: center;
  }
}
</style> 