<template>
  <section class="nf" role="region" aria-labelledby="nf-title">
    <div class="nf__overlay"></div>

    <div class="nf__content">
      <!-- Sol: REBİS sol kart hissi -->
      <div class="nf__left">
        <div class="nf__brandcard">
          <div class="nf__brandcard__logo">
            <img
              :src="RteuLogo"
              alt="Recep Tayyip Erdoğan Üniversitesi Logosu"
              class="brand-logo"
              loading="eager"
            />
          </div>
          <div class="nf__brandcard__caption">
            BİLGİ VE<br />DEĞER<br />ÜRETEN ÜNİVERSİTE
          </div>
          <p class="nf__signature">Tasarım: Fırat Atalay</p>
        </div>
      </div>

      <!-- Sağ: Mesaj ve aksiyon -->
      <div class="nf__right">
        <div class="nf__badge" aria-hidden="true">404</div>
        <h1 id="nf-title" class="nf__title">Sayfa Bulunamadı</h1>
        <p class="nf__desc">
          <code class="nf__path">{{ $route.fullPath }}</code>
          adresi taşınmış veya hiç var olmamış olabilir.
        </p>

        <div class="nf__actions">
          <RouterLink :to="backHref" class="btn btn--primary">{{ backText }}</RouterLink>
          <RouterLink :to="homeHref" class="btn btn--ghost">Ana sayfa</RouterLink>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import RteuLogo from '@/assets/rteu-logo.jpg'; // svg ise .svg kullan
import { computed } from 'vue';

const backHref = computed(() => {
  try {
    const u = JSON.parse(localStorage.getItem('user') || 'null')
    if (u?.role === 'admin')   return '/admin/dashboard'
    if (u?.role === 'advisor') return '/advisor/dashboard'
    if (u?.role === 'user')    return '/student/dashboard'
    return '/login'
  } catch { return '/login' }
})

const backText = computed(() => {
  try {
    const u = JSON.parse(localStorage.getItem('user') || 'null')
    return u?.role ? 'Panelime Dön' : 'Girişe Dön'
  } catch { return 'Girişe Dön' }
})

const homeHref = '/'
</script>

<style scoped>
/* Kurumsal tonlar */
.nf {
  --brand-blue: #1e4398;  /* RTEÜ mavi */
  --brand-green: #177d3a; /* RTEÜ yeşil (opsiyonel kullanılır) */
  --brand: var(--brand-blue);
  --accent: #ffc107;      /* REBİS sarı buton */
  --text: #ffffff;

  position: relative;
  min-height: min(100vh, 920px);
  color: var(--text);

  /* İstersen arka plana kampüs görseli ekleyebilirsin */
  /* background-image: url('/assets/campus.jpg'); */
  background:
    linear-gradient(90deg, rgba(10, 28, 40, .86), rgba(10, 28, 40, .56)),
    radial-gradient(1200px 800px at -20% -20%, rgba(30, 67, 152, .35), transparent 60%),
    #0f2021;
}

.nf__overlay { position: absolute; inset: 0; pointer-events: none; }

.nf__content {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: clamp(16px, 3vw, 48px);
  align-items: center;
  min-height: inherit;
  padding: clamp(16px, 3vw, 40px);
}
@media (max-width: 960px) {
  .nf__content { grid-template-columns: 1fr; padding-block: 64px; }
}

/* Sol kart */
.nf__left { display: grid; place-items: center; }
.nf__brandcard {
  width: min(420px, 92%);
  border: 3px solid #ffffff;
  border-radius: 8px;
  padding: 24px 20px;
  background: rgba(255,255,255,0.04);
  backdrop-filter: blur(2px);
}
.nf__brandcard__logo {
  width: min(200px, 70%);
  margin: 8px auto 16px;
}
.brand-logo {
  display: block;
  width: 100%;
  height: auto;
  object-fit: contain;
  filter: drop-shadow(0 1px 0 #ffffff40);
}
.nf__brandcard__caption {
  font-weight: 700;
  line-height: 1.08;
  letter-spacing: .02em;
}
.nf__signature {
  opacity: .6;
  font-size: .85rem;
  margin-top: 10px;
}

/* Sağ blok */
.nf__right { text-align: left; }
.nf__badge {
  display: inline-grid;
  place-items: center;
  width: clamp(88px, 12vw, 140px);
  height: clamp(88px, 12vw, 140px);
  border-radius: 50%;
  border: 10px solid var(--brand);
  outline: 3px solid #ffffff2e;
  font-weight: 800;
  font-size: clamp(28px, 5vw, 48px);
  letter-spacing: .06em;
  margin-bottom: 18px;
}
.nf__title {
  font-size: clamp(28px, 5vw, 46px);
  margin: 0 0 10px;
  font-weight: 800;
}
.nf__desc { margin: 0 0 28px; color: #e9f1f1; opacity: .92; }
.nf__path {
  background: #00000033;
  padding: 2px 8px;
  border-radius: 6px;
  border: 1px solid #ffffff1a;
  color: #fff;
  word-break: break-all;
}

/* Butonlar */
.nf__actions { display: flex; gap: 12px; flex-wrap: wrap; }
.btn {
  appearance: none; border: 2px solid transparent; cursor: pointer;
  padding: 10px 18px; border-radius: 8px; font-weight: 800; text-decoration: none;
  color: var(--text); display: inline-flex; align-items: center; justify-content: center;
}
.btn--primary { background: var(--accent); color: #1a1a1a; border-color: var(--accent); }
.btn--primary:hover { filter: brightness(0.95); }
.btn--ghost { background: transparent; border-color: #ffffff88; }
.btn--ghost:hover { background: #ffffff14; }
</style>
