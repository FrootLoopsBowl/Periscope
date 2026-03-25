<template>
  <div class="subnav" :class="{ 'subnav--expand': isExpanded }">
    <button class="subnav__title" @click="toggleExpansion">
      {{ t('routes.admin.children.teams.name') }}
      <IconChevron class="icon icon--white" :class="{'icon--rotate-180' : !isExpanded}"/>
    </button>

    <Transition name="expand">
      <ul class="subnav__content" ref="content" v-show="isExpanded">
        <li>
          <RouterLink :to="{ name: 'admin.children.teams.add' }" class="subnav__navlink subnav__navlink--add">
            + {{ t('routes.admin.children.teams.add.name') }}
          </RouterLink>
        </li>
        <li v-for="team in teamStore.teams" :key="team.id">
          <RouterLink
            :to="{ name: 'admin.children.teams.detail', params: { id: team.id } }"
            class="subnav__navlink"
          >
            {{ team.name }}
          </RouterLink>
        </li>
        <li v-if="teamStore.teams.length === 0 && !isLoading">
          <span class="subnav__navlink opacity-60 cursor-default">{{ t('pages.admin.dashboard.filters.noTeams') }}</span>
        </li>
      </ul>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import {onMounted, ref} from "vue"
import {useI18n} from "vue3-i18n"
import IconChevron from "@/assets/icons/icon__chevron.svg"
import {useTeamService} from "@/inversify.config"
import {useTeamStore} from "@/stores/teamStore"

const {t} = useI18n()
const teamService = useTeamService()
const teamStore = useTeamStore()

const isLoading = ref(true)
const isExpanded = ref(true)
const content = ref<HTMLElement>()
const height = ref<string>()

onMounted(async () => {
  const teams = await teamService.getAllNonPaginated()
  teamStore.setTeams(teams)
  isLoading.value = false
  height.value = `${content.value ? content.value.scrollHeight : 0}px`
  toggleExpansion()
})

function toggleExpansion() {
  isExpanded.value = !isExpanded.value
}
</script>

<style scoped>
.expand-leave-active,
.expand-enter-active {
  transition: max-height 0.2s cubic-bezier(0.69, 0.33, 0.16, 0.97);
  overflow: hidden;
}

.expand-enter-to,
.expand-leave-from {
  max-height: v-bind(height);
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
}
</style>
