<template>
    <div class="content-grid">
        <div class="flex flex-col gap-3 pb-6 border-b-2 border-green-light">
            <span class="text-xs font-montserrat uppercase tracking-widest text-green-dark">Administration</span>
            <div class="flex items-center justify-between flex-wrap gap-4">
                <h1 class="text-4xl font-montserrat font-semibold text-grey-darker">
                    {{ t('pages.athletes.edit.title') }}
                </h1>
                <BackLink />
            </div>
        </div>

        <Loader v-if="isLoading" />

        <AthleteEditForm v-if="!isLoading && athlete"
                         :initial-athlete="athlete"
                         @form-submit="handleSubmit" />
    </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {useRouter} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {IUpdateAthleteRequest} from "@/types/requests"
import AthleteEditForm from "@/components/athletes/AthleteEditForm.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {onMounted, ref} from "vue"
import {Athlete} from "@/types/entities"

const {t} = useI18n()
const router = useRouter()

const props = defineProps<{ id: string }>()

const athleteService = useAthleteService()

const isLoading = ref(true)
const preventMultipleSubmit = ref<boolean>(false)
const athlete = ref<Athlete | null>(null)

onMounted(async () => {
  athlete.value = await athleteService.getById(props.id)
  isLoading.value = false
})

async function handleSubmit(request: IUpdateAthleteRequest) {
  if (preventMultipleSubmit.value) return

  preventMultipleSubmit.value = true

  const succeededOrNotResponse = await athleteService.updateAthlete(props.id, request)
  if (succeededOrNotResponse.succeeded) {
    preventMultipleSubmit.value = false
    notifySuccess(t('pages.athletes.edit.validation.successMessage'))
    setTimeout(() => {
      router.push({name: 'admin.children.athletes.index'})
    }, 1500)
    return
  }

  const errorMessages = succeededOrNotResponse.getErrorMessages('pages.athletes.edit.validation')
  if (errorMessages.length == 0)
    notifyError(t('pages.athletes.edit.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false
}
</script>