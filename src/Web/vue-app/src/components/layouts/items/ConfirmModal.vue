<template>
  <Transition name="fade">
    <div class="popup" v-if="show">
      <span class="popup__bg" @click="$emit('cancel')"></span>
      <div class="popup__container">
        <div class="popup__header">
          <p class="popup__title h2-like">{{ title }}</p>
        </div>
        <div class="popup__content">
          <div class="popup__block">
            <p class="text-grey-medium">{{ message }}</p>
          </div>
          <div class="confirm-modal__actions">
            <button class="btn confirm-modal__btn-cancel" type="button" @click="$emit('cancel')">
              {{ t('global.cancel') }}
            </button>
            <button class="btn btn--red confirm-modal__btn-confirm" type="button" @click="$emit('confirm')">
              {{ t('global.delete') }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue3-i18n'

defineProps<{
  show: boolean
  title: string
  message: string
}>()

defineEmits<{
  confirm: []
  cancel: []
}>()

const { t } = useI18n()
</script>

<style scoped>
.confirm-modal__actions {
  display: flex;
  border-top: 1px solid var(--color-green-light);
}

.confirm-modal__btn-cancel,
.confirm-modal__btn-confirm {
  flex: 1;
  justify-content: center;
  border-radius: 0;
  padding: 16px;
  font-family: var(--font-montserrat);
  font-weight: 700;
}

.confirm-modal__btn-cancel {
  border-right: 1px solid var(--color-green-light);
  background-color: var(--color-white);
  color: var(--color-grey-medium);
  border-top: none;
  border-bottom: none;
  border-left: none;
}

.confirm-modal__btn-cancel:hover {
  background-color: var(--color-grey-lighter);
}

.fade-leave-active,
.fade-enter-active {
  transition: opacity 0.2s cubic-bezier(0.69, 0.33, 0.16, 0.97);
}

.fade-enter-to,
.fade-leave-from {
  opacity: 1;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
