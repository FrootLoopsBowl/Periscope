<template>
  <Card :is-authentication="true">
    <Loader v-if="isLoading" />

    <template v-else>
      <div v-if="athlete">
        <h2 class="athlete-submission__title">{{ t('pages.athleteForm.title').replace('{firstName}', athlete.firstName) }}</h2>

        <form class="form" novalidate @submit.prevent="handleSubmit">
          <div class="form__row">
            <label for="effort">{{ t('athleteForm.effort') }}</label>
            <input id="effort" type="number" v-model.number="effort" min="1" max="10" required />
          </div>

          <div class="form__row">
            <label for="duration">{{ t('athleteForm.duration') }}</label>
            <input id="duration" type="number" v-model.number="duration" min="1" required />
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
