<template>
    <form class="form" novalidate @submit.prevent="handleSubmit">
        <FormRow :withThreeColumns="true">
            <FormInput :ref="addFormInputRef"
                       v-model="athlete.firstName"
                       :label="t('global.firstName')"
                       :rules="[required]"
                       :maxlength="50"
                       name="firstName"
                       type="text"
                       @validated="handleValidation" />
            <FormInput :ref="addFormInputRef"
                       v-model="athlete.lastName"
                       :label="t('global.lastName')"
                       :rules="[required]"
                       :maxlength="50"
                       name="lastName"
                       type="text"
                       @validated="handleValidation" />
            <FormInput :ref="addFormInputRef"
                       v-model="athlete.email"
                       :label="t('global.email')"
                       :rules="[required, mustMatchEmailFormat]"
                       :maxlength="256"
                       name="email"
                       type="text"
                       @validated="handleValidation" />
        </FormRow>
        <FormRow :withThreeColumns="true">
            <FormInput :ref="addFormInputRef"
                       v-model="athlete.dateOfBirth"
                       :label="t('global.dateOfBirth')"
                       :max="today"
                       :rules="[required, mustBeInPast]"
                       name="dateOfBirth"
                       type="date"
                       @validated="handleValidation" />
        </FormRow>
        <button class="form__submit btn btn--fullscreen">{{ t('global.save') }}</button>
    </form>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {notifyError} from "@/notify"
import {Status} from "@/validation"
import {IUpdateAthleteRequest} from "@/types/requests"
import {computed, ref} from "vue"
import {mustBeInPast, mustMatchEmailFormat, required} from "@/validation/rules"
import FormRow from "@/components/forms/FormRow.vue"
import FormInput from "@/components/forms/FormInput.vue"

const props = defineProps<{
  initialAthlete: {
    firstName: string
    lastName: string
    email: string
    dateOfBirth?: string
  }
}>()

const emit = defineEmits<{
  (event: "formSubmit", athlete: IUpdateAthleteRequest): void
}>()

const {t} = useI18n()

const athlete = ref<Partial<IUpdateAthleteRequest>>({
  firstName: props.initialAthlete.firstName,
  lastName: props.initialAthlete.lastName,
  email: props.initialAthlete.email,
  dateOfBirth: props.initialAthlete.dateOfBirth
    ? props.initialAthlete.dateOfBirth.split('T')[0]
    : undefined,
})

const today = computed(() => new Date().toISOString().split('T')[0])

const formInputs = ref<any[]>([])
const inputValidationStatuses: any = {}

function addFormInputRef(ref: typeof FormInput) {
  if (!formInputs.value.includes(ref) && ref)
    formInputs.value.push(ref)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function handleSubmit() {
  formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    return
  }
  emit("formSubmit", athlete.value as IUpdateAthleteRequest)
}
</script>