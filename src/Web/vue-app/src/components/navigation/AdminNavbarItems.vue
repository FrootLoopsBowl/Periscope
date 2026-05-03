<template>
  <ul class="navbar__nav">
    <li>
      <RouterLink :to="t('routes.account.path')" class="navbar__navlink">
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
    <li>
      <RouterLink :to="{ name: 'admin.children.dashboard' }" class="navbar__navlink">
        {{ t('routes.admin.children.dashboard.name') }}
      </RouterLink>
    </li>
    <li>
      <RouterLink :to="{ name: 'admin.children.athletes.index' }" class="navbar__navlink">
        {{ t('routes.admin.children.athletes.name') }}
      </RouterLink>
    </li>
    <li>
      <TeamNavList />
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";
import TeamNavList from "./TeamNavList.vue";

const {t} = useI18n()
</script>
