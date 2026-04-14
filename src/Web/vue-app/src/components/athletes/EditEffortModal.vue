<template>
  <Transition name="fade">
    <div class="popup" v-if="showLocal">
      <span class="popup__bg" @click="close"></span>
      <div class="popup__container" style="background: transparent; border-radius: 0;">
        <div class="bg-white rounded-xl border border-grey overflow-hidden max-w-3xl w-full">
          <div class="flex items-center gap-3 px-6 py-4" style="background: rgba(255,235,238,0.8); border-bottom: 1px solid rgba(255,200,200,0.6);">
            <span class="block w-1.5 h-7 rounded-full" style="background:#fca5a5"></span>
            <h3 class="font-montserrat font-semibold text-red-700 text-base">Modification des données</h3>
            <button type="button" class="ml-auto text-grey-dark" @click="close" aria-label="close">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M10 8.586L15.95 2.636a1 1 0 111.414 1.414L11.414 10l5.95 5.95a1 1 0 01-1.414 1.414L10 11.414l-5.95 5.95a1 1 0 01-1.414-1.414L8.586 10 2.636 4.05A1 1 0 014.05 2.636L10 8.586z" clip-rule="evenodd"/></svg>
            </button>
          </div>
          <div class="p-6">
            <div class="grid grid-cols-1 gap-4">
              <div>
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('athleteForm.date') }}</label>
                <input type="date" v-model="local.trainingDate" :max="today" class="border border-grey rounded-lg px-4 py-2 mt-1 w-full" />
                <p v-if="dateError" class="text-error mt-2">{{ dateError }}</p>
                <div v-else-if="serverError" class="mt-2 p-3 rounded bg-red-50 border border-red-100 text-red-700">{{ serverError }}</div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div class="col-span-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('athleteForm.effort') }}</label>
                  <select v-model.number="local.effort" class="border border-grey rounded-lg px-4 py-2 mt-1 w-full text-center" style="min-width:0">
                    <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
                  </select>
                </div>
                <div class="col-span-1">
                  <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('athleteForm.duration') }}</label>
                  <input type="number" v-model.number="local.durationMinutes" min="1" class="border border-grey rounded-lg px-4 py-2 mt-1 w-full text-center" />
                </div>
              </div>

              <div class="mt-2">
                <label class="text-xs font-montserrat uppercase tracking-widest text-grey-dark">{{ t('athleteForm.pleasure') }}</label>
                <div>
                  <select v-model.number="local.pleasure" class="border border-grey rounded-lg px-4 py-2 mt-1 text-center" style="width:49%">
                    <option :value="null">--</option>
                    <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
                  </select>
                </div>
              </div>

              <div class="mt-2 flex gap-3 w-full" style="box-sizing: border-box">
                <button class="btn h-12" style="width:50%" type="button" :disabled="saving" @click="close">{{ t('global.cancel') }}</button>
                <button class="btn btn--primary h-12" style="width:50%" type="button" :disabled="saving" @click="save">
                  <span v-if="saving" class="inline-flex items-center gap-2">
                    <svg class="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"></path></svg>
                    {{ t('global.sending') }}
                  </span>
                  <span v-else>{{ t('global.save') }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue3-i18n'
import { useAthleteService } from '@/inversify.config'
import axios from 'axios'

const props = defineProps<{
  show: boolean
  athleteId: string
  effort: any
  existingDates?: string[]
}>()

const emit = defineEmits<{
  (e: 'update:show', val: boolean): void
  (e: 'saved'): void
}>()

const { t } = useI18n()
const athleteService = useAthleteService()

const showLocal = ref<boolean>(props.show)
const saving = ref(false)
const dateError = ref<string | null>(null)
const serverError = ref<string | null>(null)
const today = new Date().toISOString().split('T')[0]

const local = ref({ effort: 1, durationMinutes: 1, pleasure: null as number | null, trainingDate: undefined as string | undefined })
const effortId = ref<string | null>(null)

watch(() => props.show, (v) => showLocal.value = v)

function populateFromEffort(e: any) {
  if (!e) return
  local.value.effort = e.effort ?? 1
  local.value.durationMinutes = e.durationMinutes ?? 1
  local.value.pleasure = e.pleasure ?? null
  // prefer createdAt from effort; if absent, keep existing local.trainingDate or default to today
  if (e.createdAt) {
    try {
      local.value.trainingDate = new Date(e.createdAt).toISOString().split('T')[0]
    } catch {
      // leave as-is
    }
  } else if (!local.value.trainingDate) {
    local.value.trainingDate = today
  }
  effortId.value = (e as any).id ?? (e as any).value?.id ?? null
}

watch(() => props.effort, (e) => populateFromEffort(e), { immediate: true })

function close() {
  emit('update:show', false)
}

async function save() {
  if (!props.effort) return
  dateError.value = null
  const originalDate = props.effort?.createdAt ? new Date(props.effort.createdAt).toISOString().split('T')[0] : undefined
  const newDate = local.value.trainingDate
  if (newDate && props.existingDates && props.existingDates.includes(newDate) && newDate !== originalDate) {
    dateError.value = t('pages.admin.dashboard.athletePage.efforts.dateConflict') || 'Il existe déjà des données pour cette date.'
    return
  }

  saving.value = true
  try {
    const idToUse = effortId.value ?? (props.effort && ((props.effort as any).id ?? (props.effort as any).value?.id))
    if (!idToUse) {
      console.error('EditEffortModal: missing effort id', props.effort)
      try { const { notifyError } = await import('@/notify'); notifyError(t('pages.athleteForm.submitError')) } catch { alert(t('pages.athleteForm.submitError')) }
      return
    }

    console.debug('EditEffortModal: saving effort', { athleteId: props.athleteId, effortId: idToUse, payload: local.value })
    let res: any = null
    if (typeof (athleteService as any).updateAthleteEffort === 'function') {
      res = await (athleteService as any).updateAthleteEffort(props.athleteId, idToUse, { effort: local.value.effort, pleasure: local.value.pleasure ?? undefined, durationMinutes: local.value.durationMinutes, trainingDate: local.value.trainingDate ?? null })
    } else {
      // fallback direct HTTP call if DI service doesn't expose method at runtime
      try {
        const payload = { effort: local.value.effort, pleasure: local.value.pleasure ?? undefined, durationMinutes: local.value.durationMinutes, trainingDate: local.value.trainingDate ?? null }
        const url = `${import.meta.env.VITE_API_BASE_URL}/athletes/${props.athleteId}/efforts/${idToUse}`
        const httpRes = await axios.put(url, payload, { headers: { 'Content-Type': 'application/json' } })
        res = httpRes.data ?? { succeeded: httpRes.status === 200 || httpRes.status === 204 }
      } catch (httpErr: any) {
        console.error('EditEffortModal: http fallback failed', httpErr)
        if (httpErr?.response?.data) {
          res = httpErr.response.data
        } else {
          res = { succeeded: false }
        }
      }
    }
    console.debug('EditEffortModal: update response', res)
    if (res && (res as any).succeeded) {
      try { const { notifySuccess } = await import('@/notify'); notifySuccess(t('pages.admin.dashboard.athletePage.efforts.updateSuccess')) } catch {}
      emit('saved')
      emit('update:show', false)
    } else {
      console.error('EditEffortModal: update failed', res)
      // surface backend message if present
      if (res && res.error) serverError.value = res.error
      else if (res && res.errors && Array.isArray(res.errors) && res.errors.length > 0) serverError.value = res.errors.map((e: any) => e.message || e).join('\n')
      else serverError.value = t('pages.admin.dashboard.athletePage.efforts.updateFailed') || 'La mise à jour a échoué.'
      try { const { notifyError } = await import('@/notify'); notifyError(t('pages.athleteForm.submitError')) } catch { alert(t('pages.athleteForm.submitError')) }
    }
  } catch (err) {
    console.error('EditEffortModal: error during save', err)
    serverError.value = t('pages.admin.dashboard.athletePage.efforts.updateFailed') || 'La mise à jour a échoué.'
    try { const { notifyError } = await import('@/notify'); notifyError(t('pages.athleteForm.submitError')) } catch { alert(t('pages.athleteForm.submitError')) }
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}
.modal-dialog { background: white; padding: 1.25rem; border-radius: 0.5rem; width: 100%; max-width: 560px }
.modal-title { font-weight: 700; margin-bottom: 0.5rem }
.modal-body { display:flex; flex-direction:column; gap:0.5rem }
.modal-actions { display:flex; justify-content:flex-end; gap:0.5rem; margin-top:1rem }
.label { font-size: 0.85rem; color: #333; }
.text-error { color: #b91c1c; font-size: 0.85rem; }
</style>
