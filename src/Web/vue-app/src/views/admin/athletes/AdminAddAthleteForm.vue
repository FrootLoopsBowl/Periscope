<template>
  <div class="content-grid">

    <!-- En-tête -->
    <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
      <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
      <div class="flex items-center justify-between flex-wrap gap-4">
        <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
          {{ t('routes.admin.children.athletes.add.name') }}
        </h1>
        <BackLink />
      </div>
    </div>

    <!-- Formulaire -->
    <div class="bg-white rounded-xl border border-grey overflow-hidden" style="box-shadow: var(--shadow-bold)">
      <div class="flex items-center gap-3 px-6 py-4 bg-green-lighter border-b border-green-light">
        <span class="block w-1.5 h-7 rounded-full bg-green"></span>
        <h2 class="font-montserrat font-semibold text-green-dark text-base">
          {{ t('routes.admin.children.athletes.add.name') }}
        </h2>
      </div>
      <div class="p-6">
        <Loader v-if="preventMultipleSubmit" />
        <AthleteForm @formSubmit="handleSubmit" />
      </div>
    </div>

  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {useRouter} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {ICreateAthleteRequest} from "@/types/requests"
import AthleteForm from "@/components/athletes/AthleteForm.vue"
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

  const athletePageRelativeUrl = t("routes.athletePage.path").replace("/:token", "")

  const succeededOrNotResponse = await athleteService.createAthlete({
    ...athlete,
    athletePageRelativeUrl
  })
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
