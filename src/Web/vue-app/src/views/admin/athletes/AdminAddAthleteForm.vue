<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.athletes.add.name') }}</h1>
    </div>

    <BackLink />

    <Card>
      <Loader v-if="preventMultipleSubmit" />
      <AthleteForm @formSubmit="handleSubmit" />
    </Card>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {useRouter} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {ICreateAthleteRequest} from "@/types/requests"
import AthleteForm from "@/components/athletes/AthleteForm.vue"
import Card from "@/components/layouts/items/Card.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {ref} from "vue"

const {t} = useI18n()
const router = useRouter()

const athleteService = useAthleteService()

const preventMultipleSubmit = ref<boolean>(false)

async function handleSubmit(athlete: ICreateAthleteRequest) {
  if (preventMultipleSubmit.value) return

  preventMultipleSubmit.value = true

  const succeededOrNotResponse = await athleteService.createAthlete(athlete)
  if (succeededOrNotResponse.succeeded) {
    preventMultipleSubmit.value = false
    notifySuccess(t('pages.athletes.create.validation.successMessage'))
    setTimeout(() => {
      router.push({name: 'admin.children.athletes.index'})
    }, 1500)
    return
  }

  const errorMessages = succeededOrNotResponse.getErrorMessages('pages.athletes.create.validation')
  if (errorMessages.length == 0)
    notifyError(t('pages.athletes.create.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false
}
</script>
