<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <h1 class="admin-account__page-title">{{ t(`routes.account.name`) }}</h1>
    </div>

    <div class="admin-account__sections">
      <details class="admin-account__section" open>
        <summary class="admin-account__section-summary">
          <span class="admin-account__section-title">{{ t('pages.account.loginInfos') }}</span>
          <span class="admin-account__section-arrow" aria-hidden="true"></span>
        </summary>
        <div class="admin-account__section-content">
          <div class="admin-account__info-row">
            <span class="admin-account__info-label">{{ t('global.email') }}</span>
            <span class="admin-account__info-value">{{ userStore.user.email || userStore.username || t('global.undefined') }}</span>
          </div>
        </div>
      </details>

      <details class="admin-account__section" open>
        <summary class="admin-account__section-summary">
          <span class="admin-account__section-title">{{ t('pages.account.changePasswordTitle') }}</span>
          <span class="admin-account__section-arrow" aria-hidden="true"></span>
        </summary>
        <div class="admin-account__section-content">
          <p class="admin-account__note">{{ t('pages.account.passwordDescription') }}</p>

          <div class="admin-account__form-shell">
            <div class="admin-account__form form">
            <FormInput
              :ref="addFormInputRef"
              v-model="form.currentPassword"
              :label="t('pages.account.currentPassword')"
              :rules="[required]"
              name="currentPassword"
              type="password"
              @validated="handleValidation"
            />
            <FormInput
              :ref="addFormInputRef"
              v-model="form.newPassword"
              :label="t('pages.account.newPassword')"
              :rules="[required, validateNewPassword]"
              name="newPassword"
              type="password"
              @validated="handleValidation"
            />
            <FormInput
              :ref="addFormInputRef"
              v-model="form.newPasswordConfirmation"
              :label="t('pages.account.newPasswordConfirmation')"
              :rules="[required, validatePasswordConfirmation]"
              name="newPasswordConfirmation"
              type="password"
              @validated="handleValidation"
            />
            <button class="btn btn--purple btn--big admin-account__submit" :disabled="preventMultipleSubmit" @click="submitChangePassword">
              {{ t('pages.account.changePasswordSubmit') }}
            </button>
            </div>
          </div>
        </div>
      </details>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { useI18n } from "vue3-i18n";
import { useRouter } from "vue-router";
import { useUserService } from "@/inversify.config";
import { notifyError, notifySuccess } from "@/notify";
import { required } from "@/validation/rules";
import { Status } from "@/validation";
import { IChangePasswordRequest } from "@/types/requests";
import FormInput from "@/components/forms/FormInput.vue";
import { useUserStore } from "@/stores/userStore";
import { usePersonStore } from "@/stores/personStore";
import { useMemberStore } from "@/stores/memberStore";
import { useAdministratorStore } from "@/stores/administratorStore";

const { t } = useI18n();
const router = useRouter();
const userService = useUserService();
const userStore = useUserStore();
const personStore = usePersonStore();
const memberStore = useMemberStore();
const administratorStore = useAdministratorStore();

const form = ref<IChangePasswordRequest>({
  currentPassword: "",
  newPassword: "",
  newPasswordConfirmation: ""
});

const formInputs = ref<(typeof FormInput)[]>([]);
const inputValidationStatuses: Record<string, boolean> = {};
const preventMultipleSubmit = ref<boolean>(false);

function validateNewPassword(value?: string): Status {
  if ((value ?? "").toLowerCase() === "qwerty123!".toLowerCase()) {
    return {
      valid: false,
      message: t("pages.account.validation.passwordTooPredictable")
    };
  }

  return { valid: true };
}

function validatePasswordConfirmation(value?: string): Status {
  if ((value ?? "") !== form.value.newPassword) {
    return {
      valid: false,
      message: t("pages.account.validation.passwordAndConfirmationMustMatch")
    };
  }

  return { valid: true };
}

function addFormInputRef(ref: typeof FormInput) {
  if (ref && !formInputs.value.includes(ref))
    formInputs.value.push(ref);
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid;
}

async function submitChangePassword() {
  if (preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  formInputs.value.forEach((input: typeof FormInput) => input.validateInput());
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t("validation.errorsInForm"));
    preventMultipleSubmit.value = false;
    return;
  }

  try {
    const response = await userService.changePassword(form.value);
    if (!response.succeeded) {
      const errorMessage = response.getErrorMessages("pages.account.validation", "validation.errorOccured")[0];
      notifyError(errorMessage);
      preventMultipleSubmit.value = false;
      return;
    }

    notifySuccess(t("pages.account.validation.success"));

    setTimeout(async () => {
      userStore.reset();
      personStore.reset();
      memberStore.reset();
      administratorStore.reset();
      await router.push(t("routes.login.path"));
    }, 2000);
  } catch {
    notifyError(t("pages.account.validation.errorOccured"));
  } finally {
    preventMultipleSubmit.value = false;
  }
}
</script>

<style scoped>
.admin-account__page-title {
  display: inline-flex;
  align-items: center;
  padding-bottom: 0.5rem;
  border-bottom: 3px solid var(--color-green);
  font-size: 1.4rem;
  line-height: 1.2;
  font-weight: 700;
  color: var(--color-grey-dark);
}

.admin-account__sections {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.admin-account__section {
  border-radius: 4px;
  background: var(--color-white);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-small);
  overflow: hidden;
}

.admin-account__section[open] .admin-account__section-arrow {
  transform: rotate(180deg);
}

.admin-account__section-summary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.25rem;
  cursor: pointer;
  list-style: none;
}

.admin-account__section-summary::-webkit-details-marker {
  display: none;
}

.admin-account__section-title {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--color-grey-dark);
}

.admin-account__section-arrow {
  width: 0.95rem;
  height: 0.95rem;
  flex-shrink: 0;
  transition: transform 0.2s var(--ease-snappy);
}

.admin-account__section-arrow::before {
  content: "";
  position: absolute;
  inset: 0;
  border-right: 2px solid var(--color-green);
  border-bottom: 2px solid var(--color-green);
  transform: rotate(45deg) translate(-10%, -10%);
}

.admin-account__section-content {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  padding: 0 1.25rem 1.25rem;
  min-width: 0;
}

.admin-account__info-row {
  display: grid;
  gap: 0.35rem;
}

.admin-account__info-label {
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-grey-medium);
}

.admin-account__info-value {
  color: var(--color-grey-dark);
  font-size: 1rem;
  font-weight: 700;
  overflow-wrap: anywhere;
}

.admin-account__note {
  margin: 0;
  padding: 0.85rem 1rem;
  border-radius: 4px;
  background: var(--color-green-lighter);
  color: var(--color-green-dark);
  border: 1px solid rgba(94, 32, 40, 0.16);
  line-height: 1.5;
}

.admin-account__form-shell {
  padding: 0;
  border-radius: 0;
  background: transparent;
  border: none;
}

.admin-account__form {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  width: 100%;
  min-width: 0;
  max-width: 34rem;
}

.admin-account__form :deep(.form__field) {
  margin-bottom: 0;
  width: 100%;
  min-width: 0;
}

.admin-account__form :deep(input) {
  width: 100%;
  max-width: 100%;
  min-height: 2.45rem;
  padding: 0.5rem 0.8rem;
  border: 1px solid rgba(92, 92, 92, 0.28);
  border-radius: 0.5rem;
  background: var(--color-white);
  color: var(--color-grey-dark);
  box-shadow: none;
}

.admin-account__form :deep(input:focus) {
  outline: none;
  border-color: var(--color-green);
  background: var(--color-white);
  box-shadow: 0 0 0 3px rgba(94, 32, 40, 0.08);
}

.admin-account__form :deep(label) {
  color: var(--color-grey-medium);
  font-weight: 600;
  margin-bottom: 0.35rem;
}

.admin-account__form :deep(.form__error-message) {
  color: var(--color-red-dark);
}

.admin-account__form :deep(.error input) {
  border-color: var(--color-red);
  background: rgba(255, 96, 96, 0.08);
}

.admin-account__submit {
  align-self: flex-start;
  min-width: 11.5rem;
  margin-top: 0.25rem;
  padding: 0.7rem 1.05rem;
  border-radius: 4px;
  font-size: 0.95rem;
  line-height: 1;
}

@media (max-width: 960px) {
  .admin-account__section-summary,
  .admin-account__section-content {
    padding-left: 1rem;
    padding-right: 1rem;
  }

  .admin-account__submit {
    width: 100%;
  }
}
</style>
