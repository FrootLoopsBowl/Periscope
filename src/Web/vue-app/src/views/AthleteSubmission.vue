<template>
    <Card :is-authentication="true">
        <Loader v-if="isLoading" />

        <template v-else>
            <div v-if="athlete">
                <div class="athlete-submission">

                    <!-- En-tête -->
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

                    <form v-if="!submitSuccess"
                          class="form athlete-submission__form" 
                          novalidate 
                          @submit.prevent="handleSubmit"
                          >

                        <!-- Date de l'entraînement -->
                        <section class="athlete-submission__field-card">
                            <div class="athlete-submission__field-head">
                                <label for="training-date" class="athlete-submission__label">Date de l'entraînement </label>
                            </div>
                            <p class="athlete-submission__hint">Par défaut : aujourd'hui. Modifie si tu entres une séance passée.</p>
                            <input id="training-date" class="form__input athlete-submission__input" type="date" v-model="trainingDate" :max="todayISO" required />
                        </section>

                        <!-- Durée -->
                        <section class="athlete-submission__field-card">
                            <div class="athlete-submission__field-head">
                                <label for="duration" class="athlete-submission__label">
                                    {{ t('athleteForm.duration') }} <span class="athlete-submission__required">*</span>
                                </label>
                            </div>
                            <p class="athlete-submission__hint">Entre la durée totale de l'entraînement en minutes.</p>
                            <input id="duration"
                                   class="form__input athlete-submission__input athlete-submission__input--large"
                                   type="number"
                                   v-model.number="duration"
                                   min="1"
                                   placeholder="Ex : 60"
                                   @input="onDurationInput"
                                   required />
                        </section>

                        <!-- Niveau d'effort -->
                        <section class="athlete-submission__field-card">
                            <div class="athlete-submission__field-head">
                                <label class="athlete-submission__label">
                                    {{ t('athleteForm.effort') }} <span class="athlete-submission__required">*</span>
                                </label>
                            </div>
                            <p class="athlete-submission__hint">Note ton intensité globale sur une échelle de 1 à 10.</p>

                            <!-- Échelle toujours visible AVANT la sélection -->
                            <div class="athlete-submission__scale-block">
                                <p class="athlete-submission__scale-label">Échelle de perception de l'effort</p>
                                <p class="athlete-submission__scale-sub">Repère-toi selon l'intensité ressentie et ta capacité à parler pendant l'effort.</p>
                                <div class="athlete-submission__scale-table" role="table" aria-label="Échelle de perception de l'effort">
                                    <div class="athlete-submission__scale-row athlete-submission__scale-row--head" role="row">
                                        <div role="columnheader">Niv.</div>
                                        <div role="columnheader">Je...</div>
                                        <div role="columnheader">Je peux...</div>
                                    </div>
                                    <div v-for="item in effortScaleRows"
                                         :key="item.level"
                                         class="athlete-submission__scale-row athlete-submission__scale-row--clickable"
                                         :class="{ 'is-highlighted': isEffortHighlighted(item.level) }"
                                         role="button"
                                         tabindex="0"
                                         @click="selectEffort(item.level)">
                                        <div class="athlete-submission__scale-level" role="cell">{{ item.level }}</div>
                                        <div role="cell">{{ item.feeling }}</div>
                                        <div role="cell">{{ item.speaking }}</div>
                                    </div>
                                </div>
                            </div>

                            <!-- Boutons 1 à 10 -->
                            <div class="athlete-submission__scale-buttons" role="group" aria-label="Niveau d'effort">
                                <button v-for="n in 10"
                                        :key="n"
                                        type="button"
                                        class="athlete-submission__scale-btn"
                                        :class="[
                                            { 'is-selected': effort === n }
                                         ]"
                                        :aria-pressed="effort === n"
                                        :aria-label="`Niveau ${n}`"
                                        @click="effort = n"
                                >
                                {{ n }}
                                </button>
                            </div>
                        </section>

                        <!-- Indice de plaisir -->
                        <section class="athlete-submission__field-card">
                            <div class="athlete-submission__field-head">
                                <label class="athlete-submission__label">
                                    {{ t('athleteForm.pleasure') }} <span class="athlete-submission__required">*</span>
                                </label>
                            </div>
                            <p class="athlete-submission__hint">Indique comment tu as vécu la séance, du plus négatif au plus positif.</p>

                            <!-- Échelle toujours visible AVANT la sélection -->
                            <div class="athlete-submission__scale-block">
                                <p class="athlete-submission__scale-label">Échelle de perception du plaisir</p>
                                <p class="athlete-submission__scale-sub">Choisis le niveau qui représente le mieux ton expérience globale pendant la séance.</p>
                                <div class="athlete-submission__scale-table" role="table" aria-label="Échelle de perception du plaisir">
                                    <div class="athlete-submission__scale-row athlete-submission__scale-row--head athlete-submission__scale-row--two-cols" role="row">
                                        <div role="columnheader">Niv.</div>
                                        <div role="columnheader">Description</div>
                                    </div>
                                    <div v-for="item in pleasureScaleRows"
                                         :key="item.level"
                                         class="athlete-submission__scale-row athlete-submission__scale-row--two-cols athlete-submission__scale-row--clickable"
                                         :class="{ 'is-highlighted': pleasure !== null && String(pleasure) === item.level }"
                                         role="button"
                                         tabindex="0"
                                         @click="pleasure = Number(item.level)">
                                        <div class="athlete-submission__scale-level" role="cell">{{ item.level }}</div>
                                        <div role="cell">{{ item.text }}</div>
                                    </div>
                                </div>
                            </div>

                            <!-- Boutons 1 à 10 -->
                            <div class="athlete-submission__scale-buttons" role="group" aria-label="Indice de plaisir">
                                <button v-for="n in 10"
                                        :key="n"
                                        type="button"
                                        class="athlete-submission__scale-btn"
                                        :class="[
                                            { 'is-selected': pleasure === n }
                                        ]"
                                        :aria-pressed="pleasure === n"
                                        :aria-label="`Niveau ${n}`"
                                        @click="pleasure = n"
                                >
                                {{ n }}
                                </button>
                            </div>
                        </section>

                        <!-- Actions -->
                        <div class="athlete-submission__actions">
                            <p class="athlete-submission__required-note">* Les 3 champs sont obligatoires.</p>
                            <button class="btn btn--primary athlete-submission__submit"
                                    type="submit"
                                    :disabled="submitting">
                                <span class="athlete-submission__submit-main">
                                    {{ submitting ? t('global.sending') : t('athleteForm.submit') }}
                                </span>
                            </button>
                        </div>

                    </form>

                    <div v-if="submitSuccess" class="athlete-submission__message athlete-submission__message--success">{{ t('pages.athleteForm.thanks') }}</div>
                    <div v-if="submitError" class="athlete-submission__message athlete-submission__message--error">{{ submitError }}</div>
                </div>
            </div>

            <p v-else class="athlete-submission__message athlete-submission__message--error">
                {{ t('pages.athleteForm.notFound') }}
            </p>
        </template>
    </Card>
</template>

<script lang="ts" setup>
    import { onMounted, ref, computed } from "vue"
    import { useI18n } from "vue3-i18n"
    import { useRoute } from "vue-router"
    import { useAthleteService } from "@/inversify.config"
    import Card from "@/components/layouts/items/Card.vue"
    import Loader from "@/components/layouts/items/Loader.vue"

    const { t } = useI18n()
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

    // Date par défaut = aujourd'hui 
    function getTodayISO(): string {
        return new Intl.DateTimeFormat("fr-CA", {
            timeZone: "America/Toronto",
            year: "numeric",
            month: "2-digit",
            day: "2-digit"
        }).format(new Date())
    }

    const todayISO = computed(() => getTodayISO())
    const trainingDate = ref<string>(getTodayISO())

    const effortScaleRows = [
        { level: "1–3", feeling: "Je suis assis.", speaking: "Je peux tout faire." },
        { level: "4", feeling: "Je trottine.", speaking: "Je peux parler sans arrêt." },
        { level: "5", feeling: "Je fais un jogging lent.", speaking: "Je peux tenir une conversation." },
        {
            level: "6",
            feeling: "Je fais un jogging régulier.",
            speaking: "Je peux dire \"Je vais bien\" sans reprendre mon souffle.",
        },
        { level: "7", feeling: "Je fais un effort intense.", speaking: "Je peux répondre par une phrase courte." },
        { level: "8", feeling: "Je fais un effort très intense.", speaking: "Je peux dire un mot." },
        { level: "9", feeling: "Je fais un sprint.", speaking: "Je ne peux rien dire." },
        {
            level: "10",
            feeling: "Je fais un effort maximal en compétition.",
            speaking: "Je ne peux rien dire et si je continue à ce rythme je vais vomir.",
        },
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
        { level: "10", text: "J'aurais voulu continuer l'entraînement, j'étais déçu qu'il se termine." },
    ] as const

    function isEffortHighlighted(levelStr: string): boolean {
        if (effort.value === null) return false
        const e = effort.value
        if (levelStr === "1–3") return e >= 1 && e <= 3
        return String(e) === levelStr
    }

    function onDurationInput(e: Event) {
        const value = Number((e.target as HTMLInputElement).value)
        if (Number.isNaN(value)) {
            duration.value = null
            return
        }
        if (value < 1) duration.value = 1
        else duration.value = value
    }

    onMounted(async () => {
        const token = route.params.token as string
        athlete.value = await athleteService.getBySubmissionToken(token)
        isLoading.value = false
    })

    async function handleSubmit() {
        if (!athlete.value) return
        if (effort.value == null || duration.value == null || pleasure.value == null) {
            submitError.value = t("validation.errorsInForm")
            return
        }

        submitting.value = true
        submitError.value = null
        submitSuccess.value = false
        const token = route.params.token as string
        try {
            const res = await athleteService.submitSubmission(
                token,
                effort.value,
                duration.value,
                pleasure.value,
                trainingDate.value
            )
            if (res.succeeded) {
                submitSuccess.value = true
            } else {
                submitError.value =
                    (res.errors || []).map((e: any) => e.Message || e.ErrorMessage).join(", ") ||
                    t("pages.athleteForm.submitError")
            }
        } catch (e) {
            submitError.value = t("pages.athleteForm.submitError")
        }
        submitting.value = false
    }

    function selectEffort(levelStr: string) {
        if (levelStr === "1–3") {
            effort.value = 2 
        } else {
            effort.value = Number(levelStr)
        }
    }
</script>

<style scoped>
     /* =============================================
    BASE
    ============================================= */
     .athlete-submission {
         display: flex;
         flex-direction: column;
         gap: 1.25rem;
     }

     /* =============================================
    HERO (en-tête)
    ============================================= */
     .athlete-submission__hero {
         display: flex;
         justify-content: space-between;
         align-items: flex-start;
         gap: 1.5rem;
         padding: 1.75rem;
         border-radius: 1.25rem;
         background: radial-gradient(circle at top right, var(--color-green-light), transparent 32%), linear-gradient(135deg, #fff7f7 0%, #fffdf8 52%, var(--color-green-light) 100%);
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
         font-size: clamp(1.7rem, 3vw, 2.4rem);
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
         padding: 0.75rem 1rem;
         border-radius: 999px;
         background: rgba(255, 255, 255, 0.88);
         border: 1px solid rgba(110, 62, 62, 0.16);
         box-shadow: 0 6px 18px rgba(47, 60, 40, 0.08);
         white-space: nowrap;
     }

     .athlete-submission__legend-pill {
         display: flex;
         flex-direction: column;
         align-items: center;
         min-width: 3rem;
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
             font-size: 0.6rem;
             color: var(--color-grey-dark);
         }

     .athlete-submission__legend-divider {
         width: 2.5rem;
         height: 0.3rem;
         border-radius: 999px;
         background: linear-gradient(90deg, var(--color-red-light) 0%, var(--color-red) 100%);
     }

     /* =============================================
    FORM
    ============================================= */
     .athlete-submission__form {
         display: flex;
         flex-direction: column;
         gap: 1rem;
     }

     .athlete-submission__field-card {
         display: flex;
         flex-direction: column;
         gap: 0.85rem;
         padding: 1.25rem;
         border-radius: 1.1rem;
         background: linear-gradient(180deg, #ffffff 0%, #faf7f1 100%);
         border: 1px solid rgba(142, 148, 125, 0.24);
         box-shadow: 0 4px 18px rgba(78, 73, 52, 0.07);
         transition: border-color 0.2s, box-shadow 0.2s;
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
         color: var(--color-grey-dark);
         font-size: 0.9rem;
         line-height: 1.5;
     }

     /* Input durée */
     .athlete-submission__input {
         width: 100%;
         min-height: 3.2rem;
         padding: 0.8rem 1rem;
         border-radius: 0.95rem;
         border: 1px solid rgba(110, 62, 62, 0.22);
         background: #fff;
         box-sizing: border-box;
         font-size: 1rem;
         cursor: pointer;
     }

     .athlete-submission__input--large {
         font-size: 1.2rem;
         font-weight: 600;
     }

     .athlete-submission__input:focus {
         outline: none;
         border-color: var(--color-red);
         box-shadow: 0 0 0 4px rgba(255, 96, 96, 0.14);
     }

     /* =============================================
    BOUTONS D'ÉCHELLE 1-10
    ============================================= */
     .athlete-submission__scale-buttons {
         display: grid;
         grid-template-columns: repeat(5, 1fr);
         gap: 0.45rem;
     }

     .athlete-submission__scale-btn {
         padding: 0.75rem 0;
         border-radius: 0.75rem;
         border: 2px solid rgba(142, 148, 125, 0.22);
         background: #fff;
         color: var(--color-grey-darker);
         font-weight: 600;
         font-size: 1.05rem;
         cursor: pointer;
         transition: all 0.15s;
         -webkit-tap-highlight-color: transparent;
         min-height: 3rem; /* accessibilité tactile */
     }

         .athlete-submission__scale-btn:hover {
             transform: translateY(-1px);
         }

     .athlete-submission__scale-btn.is-selected{
         background: var(--color-red-dark);
         border-color: var(--color-red-dark);
         color: #fff;
         box-shadow: 0 4px 12px rgba(110, 62, 62, 0.35)
     }

     /* =============================================
    TABLEAU D'ÉCHELLE (toujours visible)
    ============================================= */
     .athlete-submission__scale-block {
         margin-top: 0.25rem;
     }

     .athlete-submission__scale-label {
         margin: 0 0 0.2rem;
         font-size: 0.72rem;
         font-weight: 700;
         text-transform: uppercase;
         letter-spacing: 0.08em;
         color: var(--color-red-dark);
     }

     .athlete-submission__scale-sub {
         margin: 0 0 0.5rem;
         font-size: 0.84rem;
         color: var(--color-grey-dark);
         line-height: 1.4;
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
         grid-template-columns: 5rem minmax(0, 1fr) minmax(0, 1.1fr);
         gap: 0.5rem;
         align-items: start;
         padding: 0.6rem 0.8rem;
         border-top: 1px solid rgba(142, 148, 125, 0.14);
         font-size: 0.875rem;
         line-height: 1.45;
         transition: background 0.15s;
     }

         .athlete-submission__scale-row.is-highlighted {
             background: #fff9f9;
             border-left: 3px solid var(--color-red);
         }

     .athlete-submission__scale-row--head {
         border-top: none;
         background: #fff3f3;
         font-family: var(--font-montserrat);
         font-size: 0.68rem;
         font-weight: 700;
         letter-spacing: 0.07em;
         text-transform: uppercase;
         color: var(--color-red-dark);
     }

     .athlete-submission__scale-row--two-cols {
         grid-template-columns: 5rem minmax(0, 1fr);
     }

     .athlete-submission__scale-level {
         display: inline-flex;
         align-items: center;
         justify-content: center;
         padding: 0.15rem 0.45rem;
         border-radius: 0.6rem;
         background: #fff4f4;
         border: 1px solid rgba(110, 62, 62, 0.12);
         font-family: var(--font-montserrat);
         font-size: 0.8rem;
         font-weight: 700;
         color: var(--color-red-dark);
         white-space: nowrap;
     }

    .athlete-submission__scale-row--clickable {
        cursor: pointer;
        transition: all 0.15s ease;
    }

    .athlete-submission__scale-row--clickable:hover {
        background: #fff5f5;
    }

    .athlete-submission__scale-row--clickable:active {
        transform: scale(0.98);
    }

     /* =============================================
    ACTIONS (bas du formulaire)
    ============================================= */
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
         font-size: 0.88rem;
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
         box-shadow: 0 12px 24px rgba(110, 62, 62, 0.2);
         font-family: var(--font-montserrat);
         font-weight: 700;
     }

     .athlete-submission__submit-main {
         display: block;
         width: 100%;
         text-align: center;
         line-height: 1.1;
     }

     /* =============================================
    MESSAGES
    ============================================= */
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

     /* =============================================
    RESPONSIVE TABLETTE
    ============================================= */
     @media (max-width: 768px) {
         .athlete-submission__hero {
             flex-direction: column;
             padding: 1.35rem;
         }

         .athlete-submission__legend {
             align-self: stretch;
             justify-content: center;
         }

         .athlete-submission__actions {
             flex-direction: column;
             align-items: stretch;
         }

         .athlete-submission__submit {
             width: 100%;
             min-height: 3.5rem;
         }

         .athlete-submission__required-note {
             text-align: center;
         }
     }

     /* =============================================
    RESPONSIVE MOBILE
    ============================================= */
     @media (max-width: 600px) {
         .athlete-submission {
             gap: 0.85rem;
         }

         .athlete-submission__hero,
         .athlete-submission__field-card {
             padding: 1rem;
         }

         .athlete-submission__scale-buttons {
             grid-template-columns: repeat(5, 1fr);
             gap: 0.35rem;
         }

         .athlete-submission__scale-btn {
             font-size: 1rem;
             padding: 0.7rem 0;
             min-height: 3rem;
         }

         /* Tableau d'échelle en 1 colonne sur petit mobile */
         .athlete-submission__scale-row {
             grid-template-columns: 1fr;
             gap: 0.3rem;
             padding: 0.65rem 0.75rem;
         }

         .athlete-submission__scale-row--two-cols {
             grid-template-columns: 1fr;
         }

         .athlete-submission__scale-row--head {
             display: none;
         }

         .athlete-submission__scale-level {
             justify-self: start;
         }
     }
</style>