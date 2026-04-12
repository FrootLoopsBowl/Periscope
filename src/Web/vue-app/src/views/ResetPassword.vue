<template>
  <BackLink :path="{name: 'login'}"/>

  <Card :title="t('routes.resetPassword.name')" 
        class="form" 
        :is-authentication="true">
    <Loader v-if="preventMultipleSubmit" />
    <FormTooltip>
      <p v-html="t('pages.resetPassword.tooltip')"></p>
    </FormTooltip>
    <FormInput :ref="addFormInputRef"
               v-model="resetPasswordRequest.password"
               :label="t('global.password')"
               :rules="[required, validateNewPassword]"
               name="password"
               type="password"
               @validated="handleValidation"/>
    <FormInput :ref="addFormInputRef"
               v-model="resetPasswordRequest.passwordConfirmation"
               :label="t('global.passwordConfirmation')"
               :rules="[required, validatePasswordConfirmation]"
               name="passwordConfirmation"
               type="password"
               @validated="handleValidation"/>
    <button class="btn btn--full btn--purple btn--big" @click="sendResetPasswordRequest" :disabled="preventMultipleSubmit">
      {{ t('global.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import {ref} from "vue"
import {useI18n} from "vue3-i18n"
import {required} from "@/validation/rules"
import {useAuthenticationService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Status} from "@/validation";
import {IResetPasswordRequest} from "@/types/requests";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import {Guid} from "@/types";
import {useRouter} from "vue-router";
import Loader from "@/components/layouts/items/Loader.vue";
import BackLink from "@/components/layouts/items/BackLink.vue";
import FormTooltip from "@/components/layouts/items/Tooltip.vue";

// eslint-disable-next-line no-undef
const props = defineProps<{
  userId: Guid
  token: string
}>()

const {t} = useI18n()
const router = useRouter()
const authenticationService = useAuthenticationService()

const resetPasswordRequest = ref<IResetPasswordRequest>({
  userId: props.userId,
  token: props.token,
  password: '',
  passwordConfirmation: ''
})

const formInputs = ref<(typeof FormInput)[]>([])
const inputValidationStatuses: any = {}

const preventMultipleSubmit = ref<boolean>(false);

function validateNewPassword(value?: string): Status {
  if ((value ?? "").toLowerCase() === "qwerty123!") {
    return {
      valid: false,
      message: t("pages.resetPassword.validation.passwordTooPredictable")
    };
  }

  return { valid: true };
}

function validatePasswordConfirmation(value?: string): Status {
  if ((value ?? "") !== resetPasswordRequest.value.password) {
    return {
      valid: false,
      message: t("pages.resetPassword.validation.passwordAndConfirmationMustMatch")
    };
  }

  return { valid: true };
}

function addFormInputRef(ref: typeof FormInput) {
  if (!formInputs.value.includes(ref))
    formInputs.value.push(ref)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function sendResetPasswordRequest() {
  if(preventMultipleSubmit.value) return;

  if (!resetPasswordRequest.value.userId || !resetPasswordRequest.value.token) {
    notifyError(t('pages.resetPassword.validation.invalidLink'))
    return;
  }

  preventMultipleSubmit.value = true;
  
  formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    preventMultipleSubmit.value = false;
    return;
  }

  let resetPasswordResponse = await authenticationService.resetPassword(resetPasswordRequest.value)
  if (resetPasswordResponse.succeeded) {
    preventMultipleSubmit.value = false;
    notifySuccess(t('pages.resetPassword.validation.success'))
    setTimeout(() => {
      router.push(t("routes.login.path"))
    }, 1500);
    return;
  }

  let errorMessages = resetPasswordResponse.getErrorMessages('pages.resetPassword.validation');
  if (errorMessages.length == 0)
    notifyError(t('pages.resetPassword.validation.errorOccured'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false;
}
</script>
