# Routing Revamp — **Öncesi / Sonrası** (Markdown)
---
## Fırat Atalay
---
## 1) Nested Route (child path başında `/`)

**Öncesi:** `/admin/announcements/detail/:id` ve `/advisor/...` child yolları `/` ile yazıldığı için **layout kopuyor**, menü/header kayboluyordu.
**Sonrası:** Baştaki `/` kaldırıldı → alt sayfalar artık **panel layout’u içinde** açılıyor.

```diff
- { path: '/admin/announcements/detail/:id', name: 'AdminAnnouncementDetail', component: ... }
+ { path: 'announcements/detail/:id', name: 'AdminAnnouncementDetail', props: true, component: ... }

- { path: '/advisor/announcements/detail/:id', name: 'AdvisorAnnouncementDetail', component: ... }
+ { path: 'announcements/detail/:id', name: 'AdvisorAnnouncementDetail', props: true, component: ... }
```

---

## 2) Kök Panel Yönlendirmeleri

**Öncesi:** `/admin`, `/advisor`, `/student` adresleri **boş ekran**.
**Sonrası:** Her panelde kök yol **dashboard’a redirect** ediyor.

```js
{ path: '', redirect: { name: 'AdminDashboard' } }
{ path: '', redirect: { name: 'AdvisorDashboard' } }
{ path: '', redirect: { name: 'StudentDashboard' } }
```

---

## 3) Router Guard Rol Tutarlılığı

**Öncesi:** `User`/`user` karışıklığı nedeniyle yönlendirmeler **yanılabiliyordu**.
**Sonrası:** Tümü **`user`** olarak normalize edildi → **stabil** yönlendirme.

```diff
- case 'User': next('/student/dashboard'); break;
+ case 'user': next('/student/dashboard'); break;
```

---

## 4) Dinamik Parametrelerin Aktarımı

**Öncesi:** `:id` gibi parametreler component içinde **dağınık** (`$route.params`) kullanılıyordu.
**Sonrası:** Detay rotalarına **`props: true`** eklendi → **temiz ve standart** prop akışı.

```js
{ path: 'announcements/detail/:id', name: 'AdminAnnouncementDetail', props: true, component: ... }
{ path: 'community-management/:id', name: 'AdminCommunityManagement', props: true, component: ... }
```

---

## 5) 404 Deneyimi (Global + Panel İçi)

**Öncesi:** Yanlış URL’de **boş/şaşırtıcı** ekran.
**Sonrası:**

* **Global 404:** routes sonunda `/:pathMatch(.*)*` → `NotFound.vue`
* **Panel içi 404:** Admin/Advisor/Student `children` sonunda `:pathMatch(.*)*` → **layout korunarak** 404
* **Guard istisnası:** 404 rotaları guard’dan **her zaman geçer**

```js
// children sonu (ör. admin)
{ path: ':pathMatch(.*)*', name: 'AdminNotFound', component: () => import('../views/NotFound.vue') }

// global en son
{ path: '/:pathMatch(.*)*', name: 'NotFound', component: () => import('../views/NotFound.vue') }

// guard başı
if (to.matched.some(r => r.name === 'NotFound' || r.name?.endsWith('NotFound'))) {
  return next()
}
```

---

## 6) Kurumsal 404 Arayüzü

**Öncesi:** Basit/temsili 404.
**Sonrası:** **RTEÜ temalı** `NotFound.vue` (logo + slogan, koyu arka plan, rol’e göre “**Panelime Dön**” / “**Girişe Dön**”).

> Logo: `src/assets/rteu-logo.png`
> Dosya: `src/views/NotFound.vue`

---

## 7) Scroll Davranışı

**Öncesi:** Route değişiminde sayfanın konumu korunabiliyordu.
**Sonrası:** **`scrollBehavior`** eklendi → her sayfa geçişinde **üste dön**.

```js
const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() { return { top: 0 } }
})
```


---

## 8) Hata Dayanıklılığı

**Öncesi:** Bozuk `localStorage` JSON’u guard’ı **kırabiliyordu**.
**Sonrası:** `try/catch` ile güvenli parse; kullanıcı yoksa **public/login** akışı sorunsuz.

---

### **Kısa Özet**

* **Layout kopmaları bitti**, kök URL’ler anlamlı yönleniyor, guard **tutarlı**.
* **Global + panel içi 404** ile kullanıcı **hiç boşta kalmıyor**.
* 404 ekranı **kurumsal** kimliğe uygun ve **rol farkında** geri dönüşler sunuyor.
