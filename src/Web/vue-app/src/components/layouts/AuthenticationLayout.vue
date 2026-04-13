<template>
  <div class="authentication-page">
    <div class="container container--maxwidth-xl">
      <div class="authentication-page__container">
        <LangSwitcher />

        <div class="authentication-page__shell">
          <section class="authentication-page__brand-panel">
            <div class="authentication-page__brand-badge">Periscope</div>
            <img class="authentication-page__logo" :src="NvLogo" alt="Logo" />
            <h1 class="authentication-page__headline">Suivi athlète simple, rapide et centralisé.</h1>
            <p class="authentication-page__lead">
              Connectez-vous pour gérer les accès, consulter les suivis et garder le contrôle sur vos athlètes.
            </p>
          </section>

          <section class="authentication-page__form-panel">
            <div class="authentication-page__content">
              <Notifications :is-in-page-flow="true" />

              <RouterView v-slot="{Component}">
                <template v-if="Component">
                  <Suspense>
                    <component :is="Component"/>
                    <template #fallback>
                      <Loader/>
                    </template>
                  </Suspense>
                </template>
              </RouterView>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Loader from "@/components/layouts/items/Loader.vue";
import Notifications from "@/components/layouts/items/Notifications.vue";
import LangSwitcher from "@/components/layouts/items/LangSwitcher.vue";
import NvLogo from "@/assets/nv_logo.png";
</script>

<style scoped>
.authentication-page {
  min-height: 100vh;
  background:
    radial-gradient(circle at top left, rgba(255, 255, 255, 0.28), transparent 28%),
    linear-gradient(135deg, #5e2028 0%, #7c2d38 48%, #efd8bc 48%, #faf1e6 100%);
}

.authentication-page__container {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 0;
}

.authentication-page__shell {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1.05fr) minmax(320px, 460px);
  width: 100%;
  min-height: calc(100vh - 4rem);
  border-radius: 28px;
  overflow: hidden;
  background: rgba(255, 252, 246, 0.9);
  box-shadow: 0 28px 90px rgba(10, 25, 28, 0.24);
  backdrop-filter: blur(10px);
}

.authentication-page__brand-panel {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 1.25rem;
  padding: 3rem;
  background:
    linear-gradient(180deg, rgba(255, 255, 255, 0.08), rgba(255, 255, 255, 0)),
    linear-gradient(135deg, #5e2028 0%, #7c2d38 100%);
  color: #f7f3e8;
}

.authentication-page__brand-badge {
  display: inline-flex;
  width: fit-content;
  padding: 0.45rem 0.8rem;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.16);
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.authentication-page__logo {
  width: 100%;
  max-width: 300px;
  height: auto;
  object-fit: contain;
  filter: drop-shadow(0 10px 24px rgba(0, 0, 0, 0.18));
}

.authentication-page__headline {
  margin: 0;
  font-size: clamp(2rem, 4vw, 3.4rem);
  line-height: 0.98;
  font-weight: 800;
  letter-spacing: -0.03em;
  color: #fff8ec;
}

.authentication-page__lead {
  margin: 0;
  max-width: 34rem;
  font-size: 1.02rem;
  line-height: 1.7;
  color: rgba(247, 243, 232, 0.86);
}

.authentication-page__form-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background:
    linear-gradient(180deg, rgba(255, 248, 236, 0.98) 0%, rgba(255, 252, 246, 0.98) 100%);
}

.authentication-page__content {
  width: 100%;
  max-width: 28rem;
}

.authentication-page__content :deep(.card) {
  padding: 1.5rem;
  border-radius: 24px;
  border: 1px solid rgba(18, 52, 59, 0.08);
  background: #fffdf8;
  box-shadow: 0 18px 44px rgba(18, 52, 59, 0.1);
}

.authentication-page__content :deep(.card__title) {
  color: #5e2028;
  font-size: 1.5rem;
  font-weight: 800;
}

.authentication-page__content :deep(.form__field label) {
  color: #6e3e3e;
  font-weight: 700;
}

.authentication-page__content :deep(input) {
  border-radius: 14px;
  border: 1px solid rgba(18, 52, 59, 0.14);
  background: #fff;
}

.authentication-page__content :deep(input:focus) {
  border-color: #5e2028;
  box-shadow: 0 0 0 4px rgba(94, 32, 40, 0.12);
}

.authentication-page__content :deep(.btn--purple) {
  min-height: 3.35rem;
  border-radius: 16px;
  background: linear-gradient(135deg, #5e2028 0%, #7c2d38 100%);
  color: #fff8ec;
  box-shadow: 0 14px 28px rgba(94, 32, 40, 0.22);
  font-weight: 800;
  letter-spacing: 0.02em;
  transition: transform 0.18s ease, box-shadow 0.18s ease, filter 0.18s ease;
}

.authentication-page__content :deep(.btn--purple:hover) {
  background: linear-gradient(135deg, #6d2530 0%, #8b3340 100%);
  box-shadow: 0 18px 34px rgba(94, 32, 40, 0.28);
  transform: translateY(-1px);
  filter: saturate(1.05);
}

.authentication-page__content :deep(.btn--purple:active) {
  transform: translateY(0);
  box-shadow: 0 10px 20px rgba(94, 32, 40, 0.18);
}

@media (max-width: 960px) {
  .authentication-page__shell {
    grid-template-columns: 1fr;
    min-height: auto;
  }

  .authentication-page__brand-panel {
    padding: 2rem 1.5rem;
  }

  .authentication-page__form-panel {
    padding: 1.25rem;
  }

  .authentication-page__headline {
    font-size: 2.25rem;
  }
}
</style>
