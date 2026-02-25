<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.teams.add.name') }}</h1>
    </div>

    <BackLink />

    <Card>
      <Loader v-if="preventMultipleSubmit" />
      <TeamForm @formSubmit="handleSubmit" />
    </Card>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {useRouter} from "vue-router"
import {useTeamService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {ICreateTeamRequest} from "@/types/requests"
import TeamForm from "@/components/teams/TeamForm.vue"
import Card from "@/components/layouts/items/Card.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {ref} from "vue"

const {t} = useI18n()
const router = useRouter()
const teamService = useTeamService()

const preventMultipleSubmit = ref<boolean>(false)

async function handleSubmit(team: ICreateTeamRequest) {
  if (preventMultipleSubmit.value) return

  preventMultipleSubmit.value = true

  const succeededOrNotResponse = await teamService.createTeam(team)
  if (succeededOrNotResponse.succeeded) {
    preventMultipleSubmit.value = false
    notifySuccess(t('pages.teams.create.validation.successMessage'))
    setTimeout(() => {
      router.push({name: 'admin.children.teams.index'})
    }, 1500)
    return
  }

  const errorMessages = succeededOrNotResponse.getErrorMessages('pages.teams.create.validation')
  if (errorMessages.length == 0)
    notifyError(t('pages.teams.create.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false
}
</script>
