import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/auth/Login.vue'
import Home from '../views/Home.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/login',
    name: 'Login',
    component: Login
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/auth/register.vue')
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('../views/auth/ForgotPassword.vue')
  },
  {
    path: '/reset-password',
    name: 'ResetPassword',
    component: () => import('../views/auth/ResetPassword.vue')
  },
  {
    path: '/two-factor-setup',
    name: 'TwoFactorSetup',
    component: () => import('../views/auth/TwoFactorSetup.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/two-factor-verify',
    name: 'TwoFactorVerify',
    component: () => import('../views/auth/TwoFactorVerify.vue')
  },
  {
    path: '/admin',
    name: 'AdminLayout',
    component: () => import('../views/admin/Layout.vue'),
    children: [
      {
        path: 'dashboard',
        name: 'AdminDashboard',
        component: () => import('../views/admin/Dashboard.vue')
      },
      {
        path: 'communities',
        name: 'AdminCommunities',
        component: () => import('../views/admin/Communities.vue')
      },
      {
        path: 'events',
        name: 'AdminEvents',
        component: () => import('../views/admin/Events.vue')
      },
      {
        path: 'announcements',
        name: 'AdminAnnouncements',
        component: () => import('../views/admin/Announcements.vue')
      },
      {
        path: '/admin/announcements/detail/:id',
        name: 'AdminAnnouncementDetail',
        component: () => import('../views/admin/AnnouncementDetail.vue')
      },
      {
        path: 'members',
        name: 'AdminMembers',
        component: () => import('../views/admin/Members.vue')
      },
      {
        path: 'statistics',
        name: 'AdminStatistics',
        component: () => import('../views/admin/Statistics.vue')
      },
      {
        path: 'profile',
        name: 'AdminProfile',
        component: () => import('@/views/admin/Profile.vue')
      },
      {
        path: 'community-management/:id',
        name: 'AdminCommunityManagement',
        component: () => import('../views/leader/CommunityManagement.vue')
      },
      {
        path: 'facilities',
        name: 'AdminFacilities',
        component: () => import('../views/admin/Facilities.vue')
      }
    ]
  },

  {
    path: '/advisor',
    name: 'AdvisorLayout',
    component: () => import('../views/advisor/Layout.vue'),
    children: [
      {
        path: 'dashboard',
        name: 'AdvisorDashboard',
        component: () => import('../views/advisor/Dashboard.vue')
      },
      {
        path: 'communities',
        name: 'AdvisorCommunities',
        component: () => import('../views/advisor/Communities.vue')
      },
      {
        path: 'events',
        name: 'AdvisorEvents',
        component: () => import('../views/advisor/Events.vue')
      },
      {
        path: 'announcements',
        name: 'AdvisorAnnouncements',
        component: () => import('../views/advisor/Announcements.vue')
      },
      {
        path: '/advisor/announcements/detail/:id',
        name: 'AdvisorAnnouncementDetail',
        component: () => import('../views/advisor/AnnouncementDetail.vue')
      },
      {
        path: 'community-management/:id',
        name: 'AdvisorCommunityManagement',
        component: () => import('../views/advisor/CommunityManagement.vue')
      },
      {
        path: 'profile',
        name: 'AdvisorProfile',
        component: () => import('../views/advisor/Profile.vue')
      }
    ]
  },

  {
    path: '/student',
    name: 'StudentLayout',
    component: () => import('../views/student/Layout.vue'),
    children: [
      {
        path: 'dashboard',
        name: 'StudentDashboard',
        component: () => import('../views/student/Dashboard.vue')
      },
      {
        path: 'communities',
        name: 'StudentCommunities',
        component: () => import('../views/student/Communities.vue')
      },
      {
        path: 'create-club',
        name: 'StudentCreateClub',
        component: () => import('../views/student/CreateClub.vue')
      },
      {
        path: 'community-management/:id',
        name: 'StudentCommunityManagement',
        component: () => import('../views/leader/CommunityManagement.vue')
      },
      {
        path: 'events',
        name: 'StudentEvents',
        component: () => import('../views/student/Events.vue')
      },
      {
        path: 'events/:id',
        name: 'StudentEventDetail',
        component: () => import('../views/student/AnnouncementDetail.vue')
      },
      {
        path: 'announcements',
        name: 'StudentAnnouncements',
        component: () => import('../views/student/Announcements.vue')
      },
      {
        path: 'announcements/detail/:id',
        name: 'StudentAnnouncementDetail',
        component: () => import('../views/student/AnnouncementDetail.vue')
      },
      {
        path: 'profile',
        name: 'StudentProfile',
        component: () => import('../views/student/Profile.vue')
      }
    ]
  },

  {
    path: '/leader/community/:id/manage',
    name: 'LeaderCommunityManagement',
    component: () => import('../views/leader/CommunityManagement.vue')
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// ROUTER GUARD TEKRAR AKTİF EDİLDİ
router.beforeEach((to, from, next) => {
  console.log(`Routing from ${from.path} to ${to.path}`);
  let user = null;
  try {
    const rawUser = localStorage.getItem('user');
    console.log('Raw user from localStorage:', rawUser);
    if (rawUser && rawUser !== 'undefined' && rawUser !== 'null') {
      user = JSON.parse(rawUser);
      console.log('Parsed user:', user);
    } else {
      console.log('No valid user found in localStorage.');
    }
  } catch (e) {
    console.error('Error parsing user from localStorage:', e);
    user = null;
  }

  // Public routes kontrolü
  const publicRoutes = ['/', '/login', '/register', '/forgot-password', '/reset-password', '/two-factor-verify'];
  if (publicRoutes.includes(to.path)) {
    console.log('Navigating to a public route.');
    if (user) {
      console.log(`User logged in (type: ${user.role}), redirecting to dashboard...`);
      // Kullanıcı tipine göre yönlendirme
      switch (user.role) {
        case 'admin':
          next('/admin/dashboard');
          break;
        case 'advisor':
          next('/advisor/dashboard');
          break;
        case 'user':
          next('/student/dashboard');
          break;
        default:
          console.warn('Unknown user type for redirection:', user.role);
          next('/student/dashboard');
      }
    } else {
      console.log('User not logged in, proceeding to public route.');
      next();
    }
  } else {
    console.log('Navigating to a protected route.');
    if (!user) {
      console.log('User not logged in, redirecting to login.');
      next('/login');
    } else {
      console.log(`User logged in (type: ${user.role}), checking access permissions...`);
      // Route prefix kontrolü
      let allowedPrefix = '';
      if (user.role === 'user') allowedPrefix = '/student';
      else if (user.role === 'admin') allowedPrefix = '/admin';
      else if (user.role === 'advisor') allowedPrefix = '/advisor';
      else allowedPrefix = `/${user.role?.toLowerCase?.() || 'student'}`;
      console.log('Allowed prefix:', allowedPrefix);
      console.log('Target path:', to.path);

      if (!to.path.startsWith(allowedPrefix) && !to.path.startsWith('/two-factor')) {
        console.warn(`Unauthorized access attempt to ${to.path} by user type ${user.role}. Redirecting...`);
        // Yetkisiz erişim durumu - Kullanıcıyı kendi dashboard'una yönlendir
        switch (user.role) {
          case 'admin':
            next('/admin/dashboard');
            break;
          case 'advisor':
            next('/advisor/dashboard');
            break;
          case 'User':
            next('/student/dashboard');
            break;
          default:
            console.warn('Unknown user type for unauthorized redirection:', user.role);
            next('/student/dashboard');
        }
      } else {
        console.log('Access granted, proceeding to route.');
        next();
      }
    }
  }
});
// ROUTER GUARD TEKRAR AKTİF EDİLDİ SONU

export default router