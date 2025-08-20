<template>
  <div class="student-layout">
    <nav class="sidebar" :class="{ 'sidebar-open': isSidebarOpen }">
      <div class="sidebar-header">
        <h2>Öğrenci Paneli</h2>
      </div>
      <ul class="nav-links">
        <li>
          <router-link to="/student/dashboard" class="nav-link">
            <i class="fas fa-home"></i>
            Dashboard
          </router-link>
        </li>
        <li>
          <router-link to="/student/communities" class="nav-link">
            <i class="fas fa-users"></i>
            Topluluklar
          </router-link>
        </li>
        <li>
          <router-link to="/student/create-club" class="nav-link">
            <i class="fas fa-plus-circle"></i>
            Topluluk Oluştur
          </router-link>
        </li>
        <li>
          <router-link to="/student/events" class="nav-link">
            <i class="fas fa-calendar"></i>
            Etkinlikler
          </router-link>
        </li>
        <li>
          <router-link to="/student/announcements" class="nav-link">
            <i class="fas fa-bullhorn"></i>
            Duyurular
          </router-link>
        </li>
        <li>
          <router-link to="/student/profile" class="nav-link">
            <i class="fas fa-user"></i>
            Profil
          </router-link>
        </li>
      </ul>
    </nav>
    
    <main class="main-content">
      <header class="top-bar">
        <button class="hamburger-btn" @click="toggleSidebar">
          <i class="fas fa-bars"></i>
        </button>
        <div class="user-info">
          <span>{{ studentName }}</span>
          <button @click="logout" class="logout-btn">
            <i class="fas fa-sign-out-alt"></i>
            Çıkış Yap
          </button>
        </div>
      </header>
      
      <div class="content">
        <router-view></router-view>
      </div>
    </main>
    <div v-if="isSidebarOpen" class="overlay" :style="{ display: isSidebarOpen ? 'block' : 'none' }" @click="toggleSidebar"></div>
  </div>
</template>

<script>
import { ref } from 'vue';

export default {
  name: 'StudentLayout',
  setup() {
    const isSidebarOpen = ref(false);

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
      studentName: 'Öğrenci'
    }
  },
  methods: {
    logout() {
      localStorage.removeItem('user')
      this.$router.push('/login')
    }
  },
  created() {
    const user = JSON.parse(localStorage.getItem('user'))
    if (user && user.userType === 'student') {
      this.studentName = user.name || 'Öğrenci'
    }
  }
}
</script>

<style scoped>
.student-layout {
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

.sidebar-header {
  padding: 1rem 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  margin-bottom: 1rem;
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
}

.nav-link:hover {
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
}

.hamburger-btn {
  display: none;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #2c3e50;
  padding: 0 0.5rem;
  margin-right: 1rem;
  order: -1;
}

.user-info {
  display: flex;
  align-items: center;
  margin-left: auto;
  gap: 1rem;
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background-color: #e74c3c;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.logout-btn:hover {
  background-color: #c0392b;
}

.content {
  padding: 2rem;
  flex: 1;
  overflow-y: auto;
}

.overlay {
  /* display: none; */
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 999;
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
    overflow-y: auto;
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
    /* Ensure this block remains empty or remove it */
  }

  .top-bar {
    padding: 0.5rem 1rem;
  }
  
  .user-info {
  }

  .content {
      padding: 1rem;
  }
}

@media (min-width: 769px) {
  .sidebar {
    transform: translateX(0);
    position: static;
    height: auto;
  }
  .main-content {
  }
  .hamburger-btn {
    display: none;
  }
   .overlay {
    /* display: none; */
   }
}
</style> 