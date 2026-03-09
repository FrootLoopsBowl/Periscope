<template>
  <div class="athlete-page">
    <aside class="athlete-page__sidebar">
      <div class="athlete-page__brand">
        <img class="athlete-page__logo" src="@/assets/logo.png" alt="Periscope" />
      </div>
    </aside>
    <main class="athlete-page__content">
      <Loader v-if="isLoading" />
      <div v-else class="athlete-page__shell">
        <section v-if="athlete" class="athlete-page__panel">
          <div class="athlete-page__panel-head">
            <span class="athlete-page__accent"></span>
            <div>
              <h2 class="athlete-page__panel-title">{{ athlete.firstName }} {{ athlete.lastName }}</h2>
              <p class="athlete-page__panel-text">Votre espace athlète est maintenant accessible depuis ce lien.</p>
            </div>
          </div>
        </section>

        <section v-else class="athlete-page__panel athlete-page__panel--error">
          <div class="athlete-page__panel-head">
            <span class="athlete-page__accent"></span>
            <div>
              <h2 class="athlete-page__panel-title">{{ t('routes.athletePage.name') }}</h2>
              <p class="athlete-page__error">{{ t('pages.athletePage.notFound') }}</p>
            </div>
          </div>
        </section>
      </div>
    </main>
  </div>
</template>

<script lang="ts" setup>
import {onMounted, ref} from "vue"
import {useRoute} from "vue-router"
import {useI18n} from "vue3-i18n"
import {useAthleteService} from "@/inversify.config"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const route = useRoute()
const athleteService = useAthleteService()

const isLoading = ref(true)
const athlete = ref<{ firstName: string; lastName: string } | null>(null)

onMounted(async () => {
  const token = route.params.token as string
  athlete.value = await athleteService.getBySubmissionToken(token)
  isLoading.value = false
})
</script>

<style scoped>
.athlete-page {
  display: flex;
  min-height: 100vh;
  background: var(--color-page-bg);
}

.athlete-page__sidebar {
  width: 260px;
  min-width: 260px;
  background: var(--color-green);
  display: flex;
  flex-direction: column;
  padding: 2rem 1.5rem;
}

.athlete-page__brand {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.athlete-page__logo {
  width: 100%;
  max-width: 180px;
  height: auto;
}

.athlete-page__content {
  flex: 1;
  display: flex;
  padding: 2rem;
}

.athlete-page__shell {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.athlete-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid var(--color-green-light);
}

.athlete-page__eyebrow {
  margin-bottom: 0.5rem;
  color: var(--color-green-dark);
  font-family: var(--font-montserrat);
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.athlete-page__title {
  color: var(--color-grey-darker);
  font-family: var(--font-montserrat);
  font-size: 2rem;
  line-height: 1.1;
  font-weight: 700;
}

.athlete-page__panel {
  background: var(--color-white);
  border: 1px solid var(--color-grey);
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: var(--shadow-bold);
}

.athlete-page__panel--error {
  max-width: 46rem;
}

.athlete-page__panel-head {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
  padding: 1.5rem;
  background: linear-gradient(180deg, var(--color-green-lighter) 0%, var(--color-white) 100%);
}

.athlete-page__accent {
  display: block;
  width: 0.4rem;
  min-width: 0.4rem;
  height: 3rem;
  border-radius: 999px;
  background: var(--color-green);
}

.athlete-page__panel-title {
  margin-bottom: 0.75rem;
  color: var(--color-green-dark);
  font-family: var(--font-montserrat);
  font-size: 1.125rem;
  line-height: 1.2;
  font-weight: 700;
}

.athlete-page__panel-text {
  color: var(--color-grey-dark);
  font-size: 1rem;
  line-height: 1.5;
}

.athlete-page__error {
  color: var(--color-red-dark);
  font-weight: 600;
}

@media (max-width: 1024px) {
  .athlete-page {
    flex-direction: column;
  }

  .athlete-page__sidebar {
    width: 100%;
    min-width: 0;
    padding: 1.25rem;
  }

  .athlete-page__brand {
    justify-content: flex-start;
  }

  .athlete-page__logo {
    max-width: 150px;
  }
}
</style>
