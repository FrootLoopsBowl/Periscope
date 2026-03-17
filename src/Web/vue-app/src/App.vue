<template>
  <RouterView v-if="isPublicPath" />
  <AuthenticationLayout v-else-if="!userStore.user.email || isAuthenticationPath"/>
  <DashboardLayout v-else/>
</template>

<script lang="ts" setup>
import {computed, onMounted} from "vue";
import {useRoute, useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import AuthenticationLayout from "@/components/layouts/AuthenticationLayout.vue";
import DashboardLayout from "@/components/layouts/DashboardLayout.vue";
import {useUserService} from "@/inversify.config";

const router = useRouter();
const route = useRoute();
const userStore = useUserStore();
const userService = useUserService();

const authenticationRoutes = ['login', 'twoFactor', 'forgotPassword', 'resetPassword']

const isPublicPath = computed(() => {
  return route.meta.publicLayout === true
})

let isAuthenticationPath = computed(() => {
  return authenticationRoutes.includes(route.name as string)
});

onMounted(async () => {
  if (!userStore.user.email)
    userStore.setUser(await userService.getCurrentUser())
});

</script>


