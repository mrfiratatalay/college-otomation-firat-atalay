<template>
  <div class="advisor-layout">
    <!-- SIDEBAR -->
    <nav class="sidebar" :class="{ 'sidebar-mobile-open': isSidebarOpen }">
      <div class="sidebar-header">
        <h2>Danışman Paneli</h2>
      </div>
      <ul class="nav-links">
        <li>
          <router-link to="/advisor/dashboard" class="nav-link">
            <i class="fas fa-home"></i>
            Dashboard
          </router-link>
        </li>
        <li>
          <router-link to="/advisor/communities" class="nav-link">
            <i class="fas fa-users"></i>
            Topluluklarım
          </router-link>
        </li>
        <li>
          <router-link to="/advisor/events" class="nav-link">
            <i class="fas fa-calendar-check"></i>
            Etkinlik Onayları
          </router-link>
        </li>
        <li>
          <router-link to="/advisor/announcements" class="nav-link">
            <i class="fas fa-bullhorn"></i>
            Duyurular
          </router-link>
        </li>
        <li>
            <router-link to="/advisor/profile" class="nav-link">
            <i class="fas fa-user"></i>
            Profil
            </router-link>
        </li>
      </ul>
    </nav>

    <!-- OVERLAY (visible only if isSidebarOpen = true) -->
    <div v-if="isSidebarOpen" class="sidebar-overlay" @click="toggleSidebar"></div>

    <!-- MAIN CONTENT -->
    <main class="main-content">
      <!-- TOP BAR -->
      <header class="top-bar">
        <!-- HAMBURGER BUTTON (mobile only) -->
        <button class="burger-btn" @click="toggleSidebar">
          <i class="fas fa-bars"></i>
        </button>
        <div></div>
        <div class="user-menu">
          <div class="user-info">
            <img src="https://via.placeholder.com/40" alt="Profil" class="user-avatar">
            <button class="user-dropdown-btn">
              {{ advisorName }}
              <i class="fas fa-chevron-down"></i>
            </button>
            <button @click="logout" class="logout-btn">
              <i class="fas fa-sign-out-alt"></i>
              Çıkış Yap
            </button>
          </div>
        </div>
      </header>

      <!-- CONTENT AREA -->
      <div class="content">
        <router-view></router-view>
      </div>
    </main>
  </div>
</template>


<script>
export default {
  name: 'AdvisorLayout',
  data() {
    return {
      advisorName: 'Danışman',
      // This controls whether the sidebar is open on mobile
      isSidebarOpen: false
    }
  },
  methods: {
    toggleSidebar() {
      // Toggle the sidebar for mobile screens
      this.isSidebarOpen = !this.isSidebarOpen
    },
    logout() {
      localStorage.removeItem('user')
      this.$router.push('/login')
    }
  },
  created() {
    const user = JSON.parse(localStorage.getItem('user'))
    if (user && user.userType === 'advisor') {
      this.advisorName = user.name || 'Danışman'
    }
  }
}
</script>

<style scoped>
/* --------------------
   LAYOUT CONTAINER
--------------------- */
.advisor-layout {
  display: flex;
  min-height: 100vh;
}

/* --------------------
   SIDEBAR
--------------------- */
.sidebar {
  width: 250px;
  background-color: #2c3e50;
  color: white;
  padding: 1rem;
  transition: left 0.3s ease;
  /* for mobile slide-in */
}

.sidebar-header h2 {
  margin: 0;
}

.nav-links {
  list-style: none;
  padding: 0;
  margin: 1rem 0 0 0;
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

.nav-link:hover,
.nav-link.router-link-active {
  background-color: rgba(255, 255, 255, 0.1);
}

.nav-link i {
  margin-right: 0.75rem;
  width: 20px;
}

/* 
  By default (for larger screens), the sidebar is static.
  For mobile, we will position it offscreen (left: -250px)
  and slide it in with a special class.
*/
@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: -250px;
    /* hide sidebar by default on mobile */
    height: 100%;
    z-index: 999;
    /* on top of the main content */
    overflow-y: auto;
  }

  /* When toggled open on mobile, slide in from the left */
  .sidebar.sidebar-mobile-open {
    left: 0;
  }
}

/* OVERLAY */
.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 998;
  /* just behind the sidebar (999) */
}

/* --------------------
   MAIN CONTENT
--------------------- */
.main-content {
  flex: 1;
  background-color: #f5f6fa;
  display: flex;
  flex-direction: column;
}

/* --------------------
   TOP BAR
--------------------- */
.top-bar {
  background-color: white;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.user-avatar {
  width: 42px;
  height: 42px;
  border-radius: 60%;
  object-fit: cover;
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

/* Hamburger button, hidden on larger screens */
.burger-btn {
  background: none;
  border: none;
  font-size: 1.4rem;
  color: #2c3e50;
  cursor: pointer;
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

/* Hide burger button on bigger screens, show sidebar normally */
@media (min-width: 769px) {
  .burger-btn {
    display: none;
  }
}

/* --------------------
   CONTENT AREA
--------------------- */
.content {
  padding: 1.5rem;
  flex: 1;
  overflow-y: auto;
}
</style>
