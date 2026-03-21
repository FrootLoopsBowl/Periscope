<template>
  <Card :is-authentication="true">
    <Loader v-if="isLoading" />
    <p v-else-if="athlete" class="athlete-submission__message athlete-submission__message--success">
      {{ t('pages.athleteForm.success').replace('{firstName}', athlete.firstName) }}
    </p>
    <button v-if="athlete" class="btn btn--purple" @click="viewStats">
      Voir les statistiques
    </button>
    <p v-else class="athlete-submission__message athlete-submission__message--error">
      {{ t('pages.athleteForm.notFound') }}
    </p>
  </Card>
</template>

<script lang="ts" setup>
import {onMounted, ref} from "vue"
import {useI18n} from "vue3-i18n"
import {useRoute, useRouter} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import Card from "@/components/layouts/items/Card.vue"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const route = useRoute()
const router = useRouter()
const athleteService = useAthleteService()

const isLoading = ref(true)
const athlete = ref<{ firstName: string; lastName: string } | null>(null)

onMounted(async () => {
  const token = route.params.token as string
  athlete.value = await athleteService.getBySubmissionToken(token)
  isLoading.value = false
})

const viewStats = () => {
  router.push({ name: 'athleteStats', params: { id: athlete.value?.id } });
};
</script>
