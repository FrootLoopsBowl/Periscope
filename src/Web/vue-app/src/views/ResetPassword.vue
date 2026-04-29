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
               :maxlength="256"
               name="password"
               type="password"
               @validated="handleValidation"/>
    <div class="password-requirements" aria-live="polite">
      <p class="password-requirements__title">{{ t("pages.resetPassword.passwordRequirementsTitle") }}</p>
      <ul class="password-requirements__list">
        <li
          v-for="requirement in passwordRequirements"
          :key="requirement.key"
          class="password-requirements__item"
          :class="{ 'password-requirements__item--valid': requirement.valid }"
        >
          <span class="password-requirements__icon"></span>
          <span>{{ requirement.label }}</span>
        </li>
      </ul>
    </div>
    <FormInput :ref="addFormInputRef"
               v-model="resetPasswordRequest.passwordConfirmation"
               :label="t('global.passwordConfirmation')"
               :rules="[required, validatePasswordConfirmation]"
               :maxlength="256"
               name="passwordConfirmation"
               type="password"
               @validated="handleValidation"/>
    <button class="btn btn--full btn--purple btn--big" @click="sendResetPasswordRequest" :disabled="preventMultipleSubmit">
      {{ t('global.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import {computed, ref} from "vue"
import {useI18n} from "vue3-i18n"
import {required} from "@/validation/rules"
import {getPasswordRequirements} from "@/validation/passwordRequirements";
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

const passwordRequirements = computed(() => {
  return getPasswordRequirements(resetPasswordRequest.value.password ?? "", t, "pages.resetPassword.validation");
});

function validateNewPassword(value?: string): Status {
  const password = value ?? "";
  const invalidRequirement = getPasswordRequirements(password, t, "pages.resetPassword.validation").find(requirement => !requirement.valid);

  if (invalidRequirement) {
    return {
      valid: false,
      message: invalidRequirement.label
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

<style scoped>
.password-requirements {
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
  margin: 0.2rem 0 0.85rem;
  padding: 0.9rem 1rem;
  border: 1px solid rgba(94, 32, 40, 0.12);
  border-radius: 12px;
  background: rgba(94, 32, 40, 0.04);
}

.password-requirements__title {
  margin: 0;
  color: var(--color-grey-dark);
  font-size: 0.88rem;
  font-weight: 800;
}

.password-requirements__list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr));
  gap: 0.45rem 0.75rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.password-requirements__item {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  min-height: 1.75rem;
  color: var(--color-grey-medium);
  font-size: 0.85rem;
  font-weight: 700;
  line-height: 1.3;
}

.password-requirements__item--valid {
  color: var(--color-green-medium);
}

.password-requirements__icon {
  display: inline-grid;
  place-items: center;
  flex: 0 0 1.05rem;
  width: 1.05rem;
  height: 1.05rem;
  border-radius: 50%;
  border: 2px solid rgba(95, 95, 95, 0.32);
  background: transparent;
}

.password-requirements__item--valid .password-requirements__icon {
  border-color: var(--color-green-medium);
  background: radial-gradient(circle, var(--color-green-medium) 42%, transparent 48%);
}
</style>
