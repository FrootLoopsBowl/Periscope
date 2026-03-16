<template>
  <Card :is-authentication="true">
    <Loader v-if="isLoading" />

    <template v-else>
      <div v-if="athlete">
        <h2 class="athlete-submission__title">{{ t('pages.athleteForm.title').replace('{firstName}', athlete.firstName) }}</h2>

        <form class="form athlete-submission__form" novalidate @submit.prevent="handleSubmit">
          <div class="form__row">
            <label for="effort">{{ t('athleteForm.effort') }}</label>
            <input id="effort" class="form__input" type="number" v-model.number="effort" min="1" max="10" @input="onEffortInput" required />
          </div>

          <div class="form__row">
            <label for="duration">{{ t('athleteForm.duration') }}</label>
            <input id="duration" class="form__input" type="number" v-model.number="duration" min="1" @input="onDurationInput" required />
          </div>

          <div class="form__actions">
            <button class="btn btn--primary" type="submit" :disabled="submitting">{{ submitting ? t('global.sending') : t('athleteForm.submit') }}</button>
          </div>
        </form>

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
import {onMounted, ref} from "vue"
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
const duration = ref<number | null>(null)
const submitting = ref(false)
const submitSuccess = ref(false)
const submitError = ref<string | null>(null)

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

onMounted(async () => {
  const token = route.params.token as string
  athlete.value = await athleteService.getBySubmissionToken(token)
  isLoading.value = false
})

async function handleSubmit() {
  if (!athlete.value) return
  if (effort.value == null || duration.value == null) {
    submitError.value = t('validation.errorsInForm')
    return
  }

  submitting.value = true
  submitError.value = null
  submitSuccess.value = false
  const token = route.params.token as string
  try {
    const res = await athleteService.submitSubmission(token, effort.value, duration.value)
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
</style>
