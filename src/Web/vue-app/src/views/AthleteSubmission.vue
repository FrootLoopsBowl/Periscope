<template>
  <Card :is-authentication="true">
    <Loader v-if="isLoading" />

    <template v-else>
      <div v-if="athlete">
        <h2 class="athlete-submission__title">{{ t('pages.athleteForm.title').replace('{firstName}', athlete.firstName) }}</h2>

        <form class="form athlete-submission__form" novalidate @submit.prevent="handleSubmit">
          <div class="form__row" ref="effortRow">
            <label for="effort">
              {{ t('athleteForm.effort') }}
              <button type="button" class="athlete-submission__info-btn" @click="toggleEffortInfo">ℹ️</button>
            </label>
            <select id="effort" class="form__input" v-model.number="effort" required>
              <option :value="null" disabled>{{ t('athleteForm.selectPlaceholder') }}</option>
              <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>

          <div class="form__row" ref="durationRow">
            <label for="duration">{{ t('athleteForm.duration') }}</label>
            <input id="duration" class="form__input" type="number" v-model.number="duration" min="1" @input="onDurationInput" required />
          </div>

          <div class="form__row" ref="pleasureRow">
            <label for="pleasure">
              {{ t('athleteForm.pleasure') }}
              <button type="button" class="athlete-submission__info-btn" @click="togglePleasureInfo">ℹ️</button>
            </label>
            <select id="pleasure" class="form__input" v-model.number="pleasure" required>
              <option :value="null" disabled>{{ t('athleteForm.selectPlaceholder') }}</option>
              <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>

          <div class="form__actions">
            <button class="btn btn--primary" type="submit" :disabled="submitting">{{ submitting ? t('global.sending') : t('athleteForm.submit') }}</button>
          </div>
        </form>

        <!-- Overlay popovers rendered outside the form to avoid clipping -->
        <div v-if="showEffortInfo" class="athlete-submission__popover-overlay" @click.self="showEffortInfo = false">
          <div class="athlete-submission__popover-dialog" ref="effortPopover">
            <button class="athlete-submission__popover-close" @click="showEffortInfo = false">×</button>
            <div class="athlete-submission__popover-content">{{ t('pages.athleteForm.descriptions.effort') }}</div>
          </div>
        </div>

        <div v-if="showPleasureInfo" class="athlete-submission__popover-overlay" @click.self="showPleasureInfo = false">
          <div class="athlete-submission__popover-dialog" ref="pleasurePopover">
            <button class="athlete-submission__popover-close" @click="showPleasureInfo = false">×</button>
            <div class="athlete-submission__popover-content">{{ t('pages.athleteForm.descriptions.pleasure') }}</div>
          </div>
        </div>

        <p v-if="submitSuccess" class="athlete-submission__message athlete-submission__message--success">{{ t('pages.athleteForm.thanks') }}</p>
        <p v-if="submitError" class="athlete-submission__message athlete-submission__message--error">{{ submitError }}</p>
      </div>
      <p v-else class="athlete-submission__message athlete-submission__message--error">
        {{ t('pages.athleteForm.notFound') }}
      </p>
    </template>
  </Card>
</template>

<script lang="ts" setup>
import {onMounted, ref, onBeforeUnmount} from "vue"
import {useI18n} from "vue3-i18n"
import {useRoute} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import Card from "@/components/layouts/items/Card.vue"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const route = useRoute()
const athleteService = useAthleteService()

const isLoading = ref(true)
const athlete = ref<{ firstName: string; lastName: string } | null>(null)

const effort = ref<number | null>(null)
const pleasure = ref<number | null>(null)
const duration = ref<number | null>(null)
const submitting = ref(false)
const submitSuccess = ref(false)
const submitError = ref<string | null>(null)
const showEffortInfo = ref(false)
const showPleasureInfo = ref(false)

const effortRow = ref<HTMLElement | null>(null)
const pleasureRow = ref<HTMLElement | null>(null)
const effortPopover = ref<HTMLElement | null>(null)
const pleasurePopover = ref<HTMLElement | null>(null)

function onEffortInput(e: Event) {
  const value = Number((e.target as HTMLInputElement).value)
  if (Number.isNaN(value)) {
    effort.value = null
    return
  }
  if (value > 10) effort.value = 10
  else if (value < 1) effort.value = 1
  else effort.value = value
}

function onDurationInput(e: Event) {
  const value = Number((e.target as HTMLInputElement).value)
  if (Number.isNaN(value)) {
    duration.value = null
    return
  }
  if (value < 1) duration.value = 1
  else duration.value = value
}

function toggleEffortInfo() {
  showEffortInfo.value = !showEffortInfo.value
  if (showEffortInfo.value) showPleasureInfo.value = false
}

function togglePleasureInfo() {
  showPleasureInfo.value = !showPleasureInfo.value
  if (showPleasureInfo.value) showEffortInfo.value = false
}

function onDocumentClick(e: MouseEvent) {
  const target = e.target as Node
  if (showEffortInfo.value) {
    const pop = effortPopover.value
    const row = effortRow.value
    if (pop && row && !pop.contains(target) && !row.contains(target)) showEffortInfo.value = false
  }
  if (showPleasureInfo.value) {
    const pop = pleasurePopover.value
    const row = pleasureRow.value
    if (pop && row && !pop.contains(target) && !row.contains(target)) showPleasureInfo.value = false
  }
}

onMounted(async () => {
  const token = route.params.token as string
  athlete.value = await athleteService.getBySubmissionToken(token)
  isLoading.value = false
  document.addEventListener('click', onDocumentClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick)
})

async function handleSubmit() {
  if (!athlete.value) return
  if (effort.value == null || duration.value == null || pleasure.value == null) {
    submitError.value = t('validation.errorsInForm')
    return
  }

  submitting.value = true
  submitError.value = null
  submitSuccess.value = false
  const token = route.params.token as string
  try {
    const res = await athleteService.submitSubmission(token, effort.value, duration.value, pleasure.value)
    if (res.succeeded) {
      submitSuccess.value = true
    } else {
      submitError.value = (res.errors || []).map((e: any) => e.Message || e.ErrorMessage).join(', ') || t('pages.athleteForm.submitError')
    }
  } catch (e) {
    submitError.value = t('pages.athleteForm.submitError')
  }
  submitting.value = false
}
</script>

<style scoped>
.athlete-submission__form .form__row {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.5rem;
}

.athlete-submission__form .form__row label {
  display: block;
  width: 100%;
  text-align: left;
  font-weight: 600;
}

.athlete-submission__form .form__input {
  width: 100%;
  max-width: 14rem;
  box-sizing: border-box;
  text-align: left;
}

.athlete-submission__message {
  margin-top: 1rem;
}

.athlete-submission__info-btn {
  background: transparent;
  border: none;
  margin-left: 0.5rem;
  cursor: pointer;
  font-size: 1rem;
}

.athlete-submission__popover {
  position: relative;
  margin-top: 0.5rem;
}

.athlete-submission__popover-content {
  /* kept for dialog content fallback */
  white-space: pre-line;
}

.athlete-submission__popover-overlay {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.35);
  z-index: 1000;
  padding: 1rem;
}

.athlete-submission__popover-dialog {
  background: var(--color-white);
  border: 1px solid var(--color-grey);
  border-radius: 0.75rem;
  box-shadow: var(--shadow-bold);
  max-width: 900px;
  width: 100%;
  max-height: 80vh;
  overflow: auto;
  padding: 1rem 1.25rem;
}

.athlete-submission__popover-close {
  position: absolute;
  right: 1rem;
  top: 0.5rem;
  background: transparent;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
}
@media (max-width: 600px) {
  .athlete-submission__popover-content {
    position: static;
    max-width: 100%;
  }
}

@media (max-width: 600px) {
  .athlete-submission__form {
    width: 100%;
  }
  .athlete-submission__form .form__input {
    max-width: none;
    width: 100%;
    font-size: 1rem;
  }
  .athlete-submission__form .form__actions {
    width: 100%;
  }
}

/* iPad breakpoint: improve layout for tablet sizes (incl. iPad Pro) */
@media (min-width: 768px) and (max-width: 1366px) {
  .athlete-submission__form {
    max-width: 720px;
    margin: 0 auto;
  }
  .athlete-submission__form .form__input {
    max-width: 24rem;
    font-size: 1.05rem;
  }
  .athlete-submission__popover-dialog {
    max-width: 820px;
    padding: 1.25rem;
  }
}
</style>
