<template>
  <div class="admin-layout">
    <nav class="sidebar" :class="{ 'sidebar-open': isSidebarOpen }">
      <div class="logo">
        <h2>Topluluk Yönetim</h2>
      </div>
      <ul class="nav-links">
        <li>
          <router-link to="/admin/dashboard" class="nav-link">
            <i class="fas fa-home"></i>
            Dashboard
          </router-link>
        </li>
        <li>
          <router-link to="/admin/communities" class="nav-link">
            <i class="fas fa-users"></i>
            Topluluklar
          </router-link>
        </li>
        <li>
          <router-link to="/admin/events" class="nav-link">
            <i class="fas fa-calendar"></i>
            Etkinlikler
          </router-link>
        </li>
        <li>
          <router-link to="/admin/announcements" class="nav-link">
            <i class="fas fa-bullhorn"></i>
            Duyurular
          </router-link>
        </li>
        <li>
          <router-link to="/admin/members" class="nav-link">
            <i class="fas fa-user-friends"></i>
            Üyeler
          </router-link>
        </li>
        <li>
          <router-link to="/admin/facilities" class="nav-link">
            <i class="fas fa-building"></i>
            Tesisler ve Rezervasyonlar
          </router-link>
        </li>

        <li>
          <router-link to="/admin/statistics" class="nav-link">
            <i class="fas fa-chart-bar"></i>
            İstatistikler
          </router-link>
        </li>
        <li>
          <router-link to="/admin/profile" class="nav-link">
            <i class="fas fa-user"></i>
            Profil
          </router-link>
        </li>
      </ul>
    </nav>
    
    <main class="main-content">
     
      <header class="top-bar">
        <button class="hamburger-btn" @click.stop="toggleSidebar">
          <i class="fas fa-bars"></i>
          
        </button>
        <div></div>
        <div class="user-menu">
          <!-- Bildirim Dropdown Bileşeni -->
          <NotificationDropdown />
          
          <div class="user-info">
            <img :src="userAvatar" alt="Profil" class="user-avatar">
            <div class="user-dropdown">
              <button class="user-dropdown-btn" @click="toggleDropdown">
                {{ userName }}
                <i class="fas fa-chevron-down"></i>
              </button>
              <div class="dropdown-menu" v-if="showDropdown">
                <router-link to="/admin/profile" class="dropdown-item">
                  <i class="fas fa-user"></i>
                  Profil
                </router-link>
                <router-link to="/admin/settings" class="dropdown-item">
                  <i class="fas fa-cog"></i>
                  Ayarlar
                </router-link>
                <router-link to="/two-factor-setup" class="dropdown-item">
                  <i class="fas fa-shield-alt"></i>
                  İki Faktörlü Doğrulama
                </router-link>
                <button @click="handleLogout" class="dropdown-item">
                  <i class="fas fa-sign-out-alt"></i>
                  Çıkış Yap
                </button>
              </div>
            </div>
          </div>
        </div>
      </header>
      
      <div class="content">
        <router-view></router-view>
      </div>
    </main>
    <div v-if="isSidebarOpen" class="overlay" @click="toggleSidebar"></div>
  </div>
</template>

<script>
import NotificationDropdown from '@/components/NotificationDropdown.vue';
import { ref } from 'vue';

export default {
  name: 'AdminLayout',
  components: {
    NotificationDropdown
  },
  setup() {
    const isSidebarOpen = ref(false);
    const mobileBreakpoint = 768; // Define mobile breakpoint width

    const toggleSidebar = () => {
      isSidebarOpen.value = !isSidebarOpen.value;
    };

    return {
      isSidebarOpen,
      toggleSidebar
    };
  },
  data() {
    return {
      userName: 'Admin',
      userAvatar: 'https://via.placeholder.com/40',
      showDropdown: false
    }
  },
  methods: {
    handleLogout() {
      localStorage.removeItem('user')
      this.$router.push('/login')
    },
    toggleDropdown() {
      this.showDropdown = !this.showDropdown
    }
  },
  created() {
    const user = JSON.parse(localStorage.getItem('user'))
    if (user && user.userType === 'admin') {
      this.userName = user.name || 'Admin'
    }
    
    // Dropdown dışında bir yere tıklandığında dropdown'ı kapat
    document.addEventListener('click', (e) => {
      if (!e.target.closest('.user-dropdown')) {
        this.showDropdown = false
      }
    })
  }
}
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  position: relative;
}

.sidebar {
  width: 250px;
  background-color: #2c3e50;
  color: white;
  padding: 1rem;
  transition: transform 0.3s ease;
}

.logo {
  padding: 1rem;
  text-align: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.logo h2 {
  margin: 0;
  color: white;
  font-size: 1.2rem;
}

.nav-links {
  list-style: none;
  padding: 0;
  margin: 0;
}

.nav-link {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  margin-bottom: 0.5rem;
  transition: background-color 0.3s;
  pointer-events: auto;
  position: relative;
  z-index: 1;
}

.nav-link:hover,
.nav-link.router-link-active {
  background-color: rgba(255, 255, 255, 0.1);
}

.nav-link i {
  margin-right: 0.75rem;
  width: 20px;
}

.main-content {
  flex: 1;
  background-color: #f5f6fa;
  display: flex;
  flex-direction: column;
  transition: margin-left 0.3s ease;
}

.top-bar {
  background-color: white;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.hamburger-btn {
  display: none;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #2c3e50;
  padding: 0 0.5rem;
  order: -1;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-box i {
  position: absolute;
  left: 1rem;
  color: #7f8c8d;
}

.search-box input {
  padding: 0.5rem 1rem 0.5rem 2.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  width: 300px;
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-left: auto;
}

.icon-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  color: #7f8c8d;
  cursor: pointer;
  position: relative;
}

.notification-badge {
  position: absolute;
  top: -5px;
  right: -5px;
  background-color: #e74c3c;
  color: white;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  font-size: 0.7rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}

.user-dropdown {
  position: relative;
}

.user-dropdown-btn {
  background: none;
  border: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: #2c3e50;
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 0.5rem;
  background-color: white;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  min-width: 180px;
  z-index: 100;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  color: #2c3e50;
  text-decoration: none;
  transition: background-color 0.3s;
  cursor: pointer;
  width: 100%;
  text-align: left;
  border: none;
  background: none;
}

.dropdown-item:hover {
  background-color: #f8f9fa;
}

.content {
  padding: 1.5rem;
  flex: 1;
  overflow-y: auto;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background-color: white;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.notification-modal {
  max-width: 400px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.2rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #7f8c8d;
}

.modal-body {
  padding: 1.5rem;
}

.notifications-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.notification-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.notification-item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.notification-icon {
  background-color: #e1f0fa;
  color: #3498db;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

.notification-content {
  flex: 1;
}

.notification-content p {
  margin: 0 0 0.25rem 0;
  color: #2c3e50;
}

.notification-time {
  color: #7f8c8d;
  font-size: 0.8rem;
}

.overlay {
  display: none;
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 999;
  pointer-events: auto;
}

@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    transform: translateX(-100%);
    z-index: 1000;
    box-shadow: 2px 0 5px rgba(0,0,0,0.2);
  }

  .sidebar.sidebar-open {
    transform: translateX(0);
  }

  .main-content {
    margin-left: 0;
  }

  .hamburger-btn {
    display: block;
  }

  .overlay {
    /* display: block; */
  }
  
  .sidebar .logo {
      /* Example: display: none; */
  }
  
  .top-bar {
    padding: 0.5rem 1rem;
  }
  
  .search-box input {
      width: 150px;
  }
}

@media (min-width: 769px) {
  .sidebar {
    transform: translateX(0);
    position: static;
    height: auto;
  }
  .main-content {
     /* Adjust dynamically if sidebar width changes */
    /* Removed fixed margin-left, relying on flex layout */
  }
  .hamburger-btn {
    display: none;
  }
   .overlay {
    display: none;
   }
}
</style> 