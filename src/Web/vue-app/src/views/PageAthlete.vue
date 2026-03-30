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
        <!-- When athlete is found, display the submission form immediately -->
        <section v-if="athlete" class="athlete-page__panel">
          <AthleteSubmission />
        </section>

        <section v-else class="athlete-page__panel athlete-page__panel--error">
          <div class="athlete-page__panel-head">
            <span class="athlete-page__accent"></span>
            <div>
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
import AthleteSubmission from "@/views/AthleteSubmission.vue"

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
  background:
    radial-gradient(circle at top left, rgba(255, 96, 96, 0.1), transparent 24%),
    linear-gradient(180deg, #fbf3ee 0%, var(--color-page-bg) 100%);
}

.athlete-page__sidebar {
  width: 260px;
  min-width: 260px;
  background: linear-gradient(180deg, var(--color-green-dark) 0%, var(--color-green) 100%);
  display: flex;
  flex-direction: column;
  padding: 2rem 1.5rem;
  box-shadow: inset -1px 0 0 rgba(255, 255, 255, 0.08);
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
  gap: 1.5rem;
}

.athlete-page__panel {
  background: var(--color-white);
  border: 1px solid rgba(110, 62, 62, 0.14);
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: 0 18px 42px rgba(110, 62, 62, 0.12);
}

.athlete-page__panel--error {
  max-width: 46rem;
}

.athlete-page__panel-head {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
  padding: 1.5rem;
  background: linear-gradient(180deg, #fff3f3 0%, var(--color-white) 100%);
}

.athlete-page__accent {
  display: block;
  width: 0.4rem;
  min-width: 0.4rem;
  height: 3rem;
  border-radius: 999px;
  background: linear-gradient(180deg, var(--color-red) 0%, var(--color-red-dark) 100%);
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
