<template>
  <form class="form" novalidate @submit.prevent="handleSubmit">
    <FormRow>
      <FormInput :ref="addFormInputRef"
                 v-model="team.name"
                 :label="t('global.name')"
                 :rules="[required]"
                 name="name"
                 type="text"
                 @validated="handleValidation"/>
    </FormRow>
    <button class="form__submit btn btn--fullscreen">{{ t('global.save') }}</button>
  </form>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {notifyError} from "@/notify"
import {Status} from "@/validation"
import {ICreateTeamRequest} from "@/types/requests"
import {ref} from "vue"
import {required} from "@/validation/rules"
import FormRow from "@/components/forms/FormRow.vue"
import FormInput from "@/components/forms/FormInput.vue"

// eslint-disable-next-line
const emit = defineEmits<{
  (event: "formSubmit", team: ICreateTeamRequest): void
}>()

const {t} = useI18n()

const team = ref<Partial<ICreateTeamRequest>>({})

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
  emit("formSubmit", team.value as ICreateTeamRequest)
}
</script>
