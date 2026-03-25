<template>
  <Card :is-authentication="true">
    <Loader v-if="isLoading" />

    <template v-else>
      <div v-if="athlete">
        <div class="athlete-submission">
          <header class="athlete-submission__hero">
            <div>
              <p class="athlete-submission__eyebrow">Suivi quotidien</p>
              <h2 class="athlete-submission__title">{{ t('pages.athleteForm.title').replace('{firstName}', athlete.firstName) }}</h2>
              <p class="athlete-submission__intro">
                Remplis ton niveau d'effort, la durée de ta séance et ton indice de plaisir en moins d'une minute.
              </p>
            </div>

            <div class="athlete-submission__legend" aria-hidden="true">
              <div class="athlete-submission__legend-pill">
                <span>1</span>
                <small>léger</small>
              </div>
              <div class="athlete-submission__legend-divider"></div>
              <div class="athlete-submission__legend-pill">
                <span>10</span>
                <small>maximal</small>
              </div>
            </div>
          </header>

          <form class="form athlete-submission__form" novalidate @submit.prevent="handleSubmit">
            <div class="athlete-submission__grid">
              <section class="athlete-submission__field-card" ref="effortRow">
                <div class="athlete-submission__field-head">
                  <label for="effort" class="athlete-submission__label">{{ t('athleteForm.effort') }} <span class="athlete-submission__required">*</span></label>
                  <button type="button" class="athlete-submission__info-btn" @click="toggleEffortInfo">i</button>
                </div>
                <p class="athlete-submission__hint">Note ton intensité globale sur une échelle de 1 à 10.</p>
                <select id="effort" class="form__input athlete-submission__input" v-model.number="effort" required>
                  <option :value="null" disabled>{{ t('athleteForm.selectPlaceholder') }}</option>
                  <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
                </select>
              </section>

              <section class="athlete-submission__field-card" ref="durationRow">
                <div class="athlete-submission__field-head">
                  <label for="duration" class="athlete-submission__label">{{ t('athleteForm.duration') }} <span class="athlete-submission__required">*</span></label>
                </div>
                <p class="athlete-submission__hint">Entre la durée totale de l'entraînement en minutes.</p>
                <input
                  id="duration"
                  class="form__input athlete-submission__input"
                  type="number"
                  v-model.number="duration"
                  min="1"
                  @input="onDurationInput"
                  required
                />
              </section>

              <section class="athlete-submission__field-card" ref="pleasureRow">
                <div class="athlete-submission__field-head">
                  <label for="pleasure" class="athlete-submission__label">{{ t('athleteForm.pleasure') }} <span class="athlete-submission__required">*</span></label>
                  <button type="button" class="athlete-submission__info-btn" @click="togglePleasureInfo">i</button>
                </div>
                <p class="athlete-submission__hint">Indique comment tu as vécu la séance, du plus négatif au plus positif.</p>
                <select id="pleasure" class="form__input athlete-submission__input" v-model.number="pleasure" required>
                  <option :value="null" disabled>{{ t('athleteForm.selectPlaceholder') }}</option>
                  <option v-for="n in 10" :key="n" :value="n">{{ n }}</option>
                </select>
              </section>
            </div>

            <div class="athlete-submission__actions">
              <p class="athlete-submission__required-note">* Les 3 champs sont obligatoires.</p>
              <button class="btn btn--primary athlete-submission__submit" type="submit" :disabled="submitting">
                <span class="athlete-submission__submit-main">{{ submitting ? t('global.sending') : t('athleteForm.submit') }}</span>
              </button>
            </div>
          </form>

          <div v-if="submitSuccess" class="athlete-submission__message athlete-submission__message--success">{{ t('pages.athleteForm.thanks') }}</div>
          <div v-if="submitError" class="athlete-submission__message athlete-submission__message--error">{{ submitError }}</div>
        </div>

        <div v-if="showEffortInfo" class="athlete-submission__popover-overlay" @click.self="showEffortInfo = false">
          <div class="athlete-submission__popover-dialog" ref="effortPopover">
            <button type="button" class="athlete-submission__popover-close" @click="showEffortInfo = false">x</button>
            <div class="athlete-submission__popover-content">
              <div class="athlete-submission__scale-header">
                <h3 class="athlete-submission__scale-title">Échelle de perception de l'effort</h3>
                <p class="athlete-submission__scale-subtitle">Repère-toi selon l'intensité ressentie et ta capacité à parler pendant l'effort.</p>
              </div>

              <div class="athlete-submission__scale-table" role="table" aria-label="Échelle de perception de l'effort">
                <div class="athlete-submission__scale-row athlete-submission__scale-row--head" role="row">
                  <div role="columnheader">Niveau</div>
                  <div role="columnheader">Je...</div>
                  <div role="columnheader">Je peux...</div>
                </div>

                <div v-for="item in effortScaleRows" :key="item.level" class="athlete-submission__scale-row" role="row">
                  <div class="athlete-submission__scale-level" role="cell">{{ item.level }}</div>
                  <div role="cell">{{ item.feeling }}</div>
                  <div role="cell">{{ item.speaking }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-if="showPleasureInfo" class="athlete-submission__popover-overlay" @click.self="showPleasureInfo = false">
          <div class="athlete-submission__popover-dialog" ref="pleasurePopover">
            <button type="button" class="athlete-submission__popover-close" @click="showPleasureInfo = false">x</button>
            <div class="athlete-submission__popover-content">
              <div class="athlete-submission__scale-header">
                <h3 class="athlete-submission__scale-title">Échelle de perception du plaisir</h3>
                <p class="athlete-submission__scale-subtitle">Choisis le niveau qui représente le mieux ton expérience globale pendant la séance.</p>
              </div>

              <div class="athlete-submission__scale-table" role="table" aria-label="Échelle de perception du plaisir">
                <div class="athlete-submission__scale-row athlete-submission__scale-row--head" role="row">
                  <div role="columnheader">Niveau</div>
                  <div role="columnheader">Description</div>
                </div>

                <div
                  v-for="item in pleasureScaleRows"
                  :key="item.level"
                  class="athlete-submission__scale-row athlete-submission__scale-row--two-cols"
                  role="row"
                >
                  <div class="athlete-submission__scale-level" role="cell">{{ item.level }}</div>
                  <div role="cell">{{ item.text }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <p v-else class="athlete-submission__message athlete-submission__message--error">
        {{ t('pages.athleteForm.notFound') }}
      </p>
    </template>
  </Card>
</template>

<script lang="ts" setup>
import {onMounted, ref, onBeforeUnmount} from "vue"
import {useI18n} from "vue3-i18n"
import {useRoute} from "vue-router"
import {useAthleteService} from "@/inversify.config"
import Card from "@/components/layouts/items/Card.vue"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const route = useRoute()
const athleteService = useAthleteService()

const isLoading = ref(true)
const athlete = ref<{ firstName: string; lastName: string } | null>(null)

const effort = ref<number | null>(null)
const pleasure = ref<number | null>(null)
const duration = ref<number | null>(null)
const submitting = ref(false)
const submitSuccess = ref(false)
const submitError = ref<string | null>(null)
const showEffortInfo = ref(false)
const showPleasureInfo = ref(false)

const effortRow = ref<HTMLElement | null>(null)
const pleasureRow = ref<HTMLElement | null>(null)
const effortPopover = ref<HTMLElement | null>(null)
const pleasurePopover = ref<HTMLElement | null>(null)
const effortScaleRows = [
  {
    level: "1-3",
    feeling: "Je suis assis.",
    speaking: "Je peux tout faire."
  },
  {
    level: "4",
    feeling: "Je trottine.",
    speaking: "Je peux parler sans arrêt."
  },
  {
    level: "5",
    feeling: "Je fais un jogging lent.",
    speaking: "Je peux tenir une conversation."
  },
  {
    level: "6",
    feeling: "Je fais un jogging régulier.",
    speaking: "Je peux dire \"Je vais bien, je vais très bien, je peux communiquer\" sans reprendre mon souffle."
  },
  {
    level: "7",
    feeling: "Je fais un effort intense.",
    speaking: "Je peux répondre par une phrase courte."
  },
  {
    level: "8",
    feeling: "Je fais un effort très intense.",
    speaking: "Je peux dire un mot."
  },
  {
    level: "9",
    feeling: "Je fais un sprint.",
    speaking: "Je ne peux rien dire."
  },
  {
    level: "10",
    feeling: "Je fais un effort maximal en compétition.",
    speaking: "Je ne peux rien dire et si je continue à ce rythme je vais vomir."
  }
] as const
const pleasureScaleRows = [
  { level: "1", text: "Je n'ai pas terminé l'entraînement en raison de mon humeur." },
  { level: "2", text: "J'ai pensé à plusieurs reprises d'arrêter l'entraînement." },
  { level: "3", text: "J'ai pensé une fois à arrêter l'entraînement." },
  { level: "4", text: "J'ai fait l'entraînement par obligation." },
  { level: "5", text: "L'entraînement n'a pas été plaisant." },
  { level: "6", text: "L'entraînement n'a pas été marquant." },
  { level: "7", text: "L'entraînement a été plaisant." },
  { level: "8", text: "J'ai aimé l'entraînement." },
  { level: "9", text: "J'ai adoré l'entraînement." },
  { level: "10", text: "J'aurais voulu continuer l'entraînement, j'étais déçu qu'il se termine." }
] as const

function onDurationInput(e: Event) {
  const value = Number((e.target as HTMLInputElement).value)
  if (Number.isNaN(value)) {
    duration.value = null
    return
  }
  if (value < 1) duration.value = 1
  else duration.value = value
}

function toggleEffortInfo() {
  showEffortInfo.value = !showEffortInfo.value
  if (showEffortInfo.value) showPleasureInfo.value = false
}

function togglePleasureInfo() {
  showPleasureInfo.value = !showPleasureInfo.value
  if (showPleasureInfo.value) showEffortInfo.value = false
}

function onDocumentClick(e: MouseEvent) {
  const target = e.target as Node
  if (showEffortInfo.value) {
    const pop = effortPopover.value
    const row = effortRow.value
    if (pop && row && !pop.contains(target) && !row.contains(target)) showEffortInfo.value = false
  }
  if (showPleasureInfo.value) {
    const pop = pleasurePopover.value
    const row = pleasureRow.value
    if (pop && row && !pop.contains(target) && !row.contains(target)) showPleasureInfo.value = false
  }
}

onMounted(async () => {
  const token = route.params.token as string
  athlete.value = await athleteService.getBySubmissionToken(token)
  isLoading.value = false
  document.addEventListener('click', onDocumentClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick)
})

async function handleSubmit() {
  if (!athlete.value) return
  if (effort.value == null || duration.value == null || pleasure.value == null) {
    submitError.value = t('validation.errorsInForm')
    return
  }

  submitting.value = true
  submitError.value = null
  submitSuccess.value = false
  const token = route.params.token as string
  try {
    const res = await athleteService.submitSubmission(token, effort.value, duration.value, pleasure.value)
    if (res.succeeded) {
      submitSuccess.value = true
    } else {
      submitError.value = (res.errors || []).map((e: any) => e.Message || e.ErrorMessage).join(', ') || t('pages.athleteForm.submitError')
    }
  } catch (e) {
    submitError.value = t('pages.athleteForm.submitError')
  }
  submitting.value = false
}
</script>

<style scoped>
.athlete-submission {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.athlete-submission__hero {
  display: flex;
  justify-content: space-between;
  gap: 1.5rem;
  padding: 1.75rem;
  border-radius: 1.25rem;
  background:
    radial-gradient(circle at top right, var(--color-green-light), transparent 32%),
    linear-gradient(135deg, #fff7f7 0%, #fffdf8 52%, var(--color-green-light) 100%);
  border: 1px solid rgba(110, 62, 62, 0.14);
}

.athlete-submission__eyebrow {
  margin: 0 0 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--color-red-dark);
}

.athlete-submission__title {
  margin: 0;
  color: var(--color-grey-darker);
  font-family: var(--font-montserrat);
  font-size: clamp(1.9rem, 3.2vw, 2.6rem);
  line-height: 1.05;
}

.athlete-submission__intro {
  max-width: 40rem;
  margin: 0.9rem 0 0;
  color: var(--color-grey-dark);
  font-size: 1rem;
  line-height: 1.65;
}

.athlete-submission__legend {
  display: flex;
  align-items: center;
  align-self: flex-start;
  gap: 0.9rem;
  padding: 0.9rem 1rem;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.88);
  border: 1px solid rgba(110, 62, 62, 0.16);
  box-shadow: 0 14px 30px rgba(47, 60, 40, 0.08);
}

.athlete-submission__legend-pill {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 3.25rem;
}

.athlete-submission__legend-pill span {
  font-family: var(--font-montserrat);
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--color-red-dark);
}

.athlete-submission__legend-pill small {
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.62rem;
  color: var(--color-grey-dark);
}

.athlete-submission__legend-divider {
  width: 3rem;
  height: 0.35rem;
  border-radius: 999px;
  background: linear-gradient(90deg, var(--color-red-light) 0%, var(--color-red) 100%);
}

.athlete-submission__form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.athlete-submission__grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

.athlete-submission__field-card {
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
  padding: 1.2rem;
  border-radius: 1.1rem;
  background: linear-gradient(180deg, #ffffff 0%, #faf7f1 100%);
  border: 1px solid rgba(142, 148, 125, 0.24);
  box-shadow: 0 18px 38px rgba(78, 73, 52, 0.08);
}

.athlete-submission__field-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
}

.athlete-submission__label {
  display: block;
  margin: 0;
  color: var(--color-grey-darker);
  font-family: var(--font-montserrat);
  font-size: 0.98rem;
  font-weight: 700;
}

.athlete-submission__required {
  color: var(--color-red);
  font-weight: 700;
}

.athlete-submission__hint {
  margin: 0;
  min-height: 3.2rem;
  color: var(--color-grey-dark);
  font-size: 0.93rem;
  line-height: 1.45;
}

.athlete-submission__input {
  width: 100%;
  min-height: 3.4rem;
  padding: 0.85rem 1rem;
  border-radius: 0.95rem;
  border: 1px solid rgba(110, 62, 62, 0.22);
  background: #fff;
  box-sizing: border-box;
  text-align: left;
  font-size: 1rem;
}

.athlete-submission__input:focus {
  outline: none;
  border-color: var(--color-red);
  box-shadow: 0 0 0 4px rgba(255, 96, 96, 0.16);
}

.athlete-submission__actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.25rem 0;
}

.athlete-submission__required-note {
  margin: 0;
  color: var(--color-red-dark);
  font-size: 0.9rem;
  font-weight: 600;
}

.athlete-submission__submit {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 12rem;
  min-height: 3.35rem;
  padding: 0.9rem 1.6rem;
  border-radius: 999px;
  box-shadow: 0 16px 28px rgba(110, 62, 62, 0.18);
  font-family: var(--font-montserrat);
  font-weight: 700;
}

.athlete-submission__submit-main {
  display: block;
  width: 100%;
  text-align: center;
  line-height: 1.1;
}

.athlete-submission__message {
  padding: 1rem 1.1rem;
  border-radius: 0.95rem;
  border: 1px solid transparent;
  font-weight: 600;
}

.athlete-submission__message--success {
  color: #225b17;
  background: #edf8e6;
  border-color: #b7d9a3;
}

.athlete-submission__message--error {
  color: #8b1e28;
  background: #fff0f1;
  border-color: #f1c4cb;
}

.athlete-submission__info-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.9rem;
  height: 1.9rem;
  flex: 0 0 1.9rem;
  border: 1px solid rgba(110, 62, 62, 0.2);
  border-radius: 999px;
  background: #fff2f2;
  color: var(--color-red-dark);
  cursor: pointer;
  font-family: var(--font-montserrat);
  font-size: 0.9rem;
  font-weight: 700;
}

.athlete-submission__popover-content {
  white-space: pre-line;
  color: var(--color-grey-darker);
  line-height: 1.65;
  padding-top: 0.8rem;
}

.athlete-submission__scale-header {
  margin-bottom: 0.7rem;
}

.athlete-submission__scale-title {
  margin: 0;
  font-family: var(--font-montserrat);
  font-size: 1.02rem;
  font-weight: 700;
  color: var(--color-grey-darker);
}

.athlete-submission__scale-subtitle {
  margin: 0.25rem 0 0;
  color: var(--color-grey-dark);
  font-size: 0.86rem;
  line-height: 1.45;
}

.athlete-submission__scale-table {
  display: grid;
  gap: 0;
  border: 1px solid rgba(142, 148, 125, 0.22);
  border-radius: 0.9rem;
  overflow: hidden;
  background: #fff;
}

.athlete-submission__scale-row {
  display: grid;
  grid-template-columns: 5.2rem minmax(0, 1fr) minmax(0, 1.1fr);
  gap: 0.75rem;
  align-items: start;
  padding: 0.7rem 0.85rem;
  border-top: 1px solid rgba(142, 148, 125, 0.16);
  background: #fff;
  font-size: 0.9rem;
  line-height: 1.45;
}

.athlete-submission__scale-row--head {
  border-top: none;
  background: #fff3f3;
  font-family: var(--font-montserrat);
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--color-red-dark);
}

.athlete-submission__scale-level {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 2rem;
  padding: 0.2rem 0.55rem;
  border-radius: 0.7rem;
  background: #fff4f4;
  border: 1px solid rgba(110, 62, 62, 0.12);
  font-family: var(--font-montserrat);
  font-size: 0.82rem;
  font-weight: 700;
  color: var(--color-red-dark);
}

.athlete-submission__scale-row--two-cols {
  grid-template-columns: 5.2rem minmax(0, 1fr);
}

.athlete-submission__popover-overlay {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.35);
  z-index: 1000;
  padding: 1rem;
}

.athlete-submission__popover-dialog {
  position: relative;
  background: var(--color-white);
  border: 1px solid rgba(142, 148, 125, 0.3);
  border-radius: 1rem;
  box-shadow: 0 28px 60px rgba(24, 33, 18, 0.22);
  max-width: 56rem;
  width: 100%;
  max-height: 80vh;
  overflow: auto;
  padding: 1.15rem 1.35rem 1.35rem;
}

.athlete-submission__popover-close {
  position: absolute;
  right: 1rem;
  top: 0.75rem;
  width: 2rem;
  height: 2rem;
  border: none;
  border-radius: 999px;
  background: #f3f4f0;
  color: var(--color-grey-darker);
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
}

@media (max-width: 1024px) {
  .athlete-submission__grid {
    grid-template-columns: 1fr;
  }

  .athlete-submission__hint {
    min-height: 0;
  }
}

@media (max-width: 768px) {
  .athlete-submission__hero {
    flex-direction: column;
    padding: 1.35rem;
  }

  .athlete-submission__legend {
    align-self: stretch;
    justify-content: center;
  }

  .athlete-submission__form {
    width: 100%;
  }

  .athlete-submission__actions {
    flex-direction: column;
    align-items: stretch;
  }

  .athlete-submission__submit {
    width: 100%;
  }

  .athlete-submission__required-note {
    text-align: center;
  }
}

@media (max-width: 600px) {
  .athlete-submission {
    gap: 1rem;
  }

  .athlete-submission__hero,
  .athlete-submission__field-card {
    padding: 1rem;
  }

  .athlete-submission__popover-dialog {
    padding: 1rem 1rem 1.15rem;
  }

  .athlete-submission__scale-row {
    grid-template-columns: 1fr;
    gap: 0.45rem;
    padding: 0.75rem;
  }

  .athlete-submission__scale-row--head {
    display: none;
  }

  .athlete-submission__scale-level {
    justify-self: start;
  }

  .athlete-submission__scale-row--two-cols {
    grid-template-columns: 1fr;
  }
}
</style>
