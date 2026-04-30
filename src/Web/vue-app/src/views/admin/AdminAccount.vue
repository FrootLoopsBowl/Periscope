<template>
  <div class="account-page content-grid">
    <div class="account-page__hero">
      <h1 class="account-page__title">{{ t("routes.account.name") }}</h1>
    </div>

    <div class="account-page__grid">
      <section class="account-panel account-panel--profile">
        <div class="account-panel__header">
          <h2 class="account-panel__title">{{ t("pages.account.loginInfos") }}</h2>
          <p class="account-panel__subtitle">{{ t("pages.account.loginInfosDescription") }}</p>
        </div>

        <div class="account-info-list">
          <div class="account-info-list__item">
            <span class="account-info-list__label">{{ t("global.fullName") }}</span>
            <span class="account-info-list__value">{{ displayName }}</span>
          </div>
          <div class="account-info-list__item">
            <span class="account-info-list__label">{{ t("global.email") }}</span>
            <span class="account-info-list__value">{{ userStore.user.email || userStore.username || t("global.undefined") }}</span>
          </div>
        </div>
      </section>

      <section class="account-panel account-panel--security">
        <div class="account-panel__header">
          <h2 class="account-panel__title">{{ t("pages.account.changePasswordTitle") }}</h2>
          <p class="account-panel__subtitle">{{ t("pages.account.passwordDescription") }}</p>
        </div>

        <div class="account-security-callout">
          <strong>{{ t("pages.account.passwordCalloutTitle") }}</strong>
          <p>{{ t("pages.account.passwordCalloutText") }}</p>
        </div>

        <div class="account-form form">
          <FormInput
            :ref="addFormInputRef"
            v-model="form.currentPassword"
            :label="t('pages.account.currentPassword')"
            :rules="[required]"
            :maxlength="256"
            name="currentPassword"
            type="password"
            @validated="handleValidation"
          />
          <FormInput
            :ref="addFormInputRef"
            v-model="form.newPassword"
            :label="t('pages.account.newPassword')"
            :rules="[required, validateNewPassword]"
            :maxlength="256"
            name="newPassword"
            type="password"
            @validated="handleValidation"
          />
          <div class="password-requirements" aria-live="polite">
            <p class="password-requirements__title">{{ t("pages.account.passwordRequirementsTitle") }}</p>
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
          <FormInput
            :ref="addFormInputRef"
            v-model="form.newPasswordConfirmation"
            :label="t('pages.account.newPasswordConfirmation')"
            :rules="[required, validatePasswordConfirmation]"
            :maxlength="256"
            name="newPasswordConfirmation"
            type="password"
            @validated="handleValidation"
          />
          <button class="btn btn--purple btn--big account-form__submit" :disabled="preventMultipleSubmit" @click="submitChangePassword">
            {{ t("pages.account.changePasswordSubmit") }}
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from "vue";
import type { ComponentPublicInstance } from "vue";
import { useI18n } from "vue3-i18n";
import { useRouter } from "vue-router";
import { useUserService } from "@/inversify.config";
import { notifyError, notifySuccess } from "@/notify";
import { required } from "@/validation/rules";
import { Status } from "@/validation";
import { getPasswordRequirements } from "@/validation/passwordRequirements";
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

type FormInputInstance = ComponentPublicInstance & { validateInput: () => void };

const formInputs = ref<FormInputInstance[]>([]);
const inputValidationStatuses: Record<string, boolean> = {};
const preventMultipleSubmit = ref<boolean>(false);

const displayName = computed(() => personStore.person.fullName || `${personStore.person.firstName ?? ""} ${personStore.person.lastName ?? ""}`.trim() || userStore.user.email || userStore.username || t("global.undefined"));

const passwordRequirements = computed(() => {
  return getPasswordRequirements(form.value.newPassword ?? "", t, "pages.account.validation");
});

function validateNewPassword(value?: string): Status {
  const password = value ?? "";
  const invalidRequirement = getPasswordRequirements(password, t, "pages.account.validation").find(requirement => !requirement.valid);

  if (invalidRequirement) {
    return {
      valid: false,
      message: invalidRequirement.label
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

function addFormInputRef(inputRef: Element | ComponentPublicInstance | null) {
  if (!inputRef) return;

  const formInput = inputRef as FormInputInstance;
  if (!formInputs.value.includes(formInput))
    formInputs.value.push(formInput);
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid;
}

async function submitChangePassword() {
  if (preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  formInputs.value.forEach((input: FormInputInstance) => input.validateInput());
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
.account-page {
  gap: 1.5rem;
}

.account-page__hero {
  display: block;
}

.account-page__title {
  margin: 0;
  color: var(--color-black);
  font-weight: 700;
}

.account-panel {
  border-radius: 20px;
  border: 1px solid rgba(94, 32, 40, 0.12);
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 18px 38px rgba(37, 24, 26, 0.06);
}

.account-page__grid {
  display: grid;
  grid-template-columns: minmax(18rem, 24rem) minmax(0, 1fr);
  gap: 1rem;
}

.account-panel {
  padding: 1.25rem;
}

.account-panel__header {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 1rem;
}

.account-panel__title {
  margin: 0;
}

.account-panel__subtitle {
  margin: 0;
  color: var(--color-grey-medium);
  line-height: 1.6;
}

.account-info-list {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.account-info-list__item {
  display: grid;
  gap: 0.25rem;
  padding: 0.9rem 1rem;
  border-radius: 14px;
  background: linear-gradient(180deg, #fffdf9 0%, #f8f2f3 100%);
  border: 1px solid rgba(94, 32, 40, 0.08);
}

.account-info-list__label {
  color: var(--color-grey-medium);
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.account-info-list__value {
  color: var(--color-grey-dark);
  font-size: 0.98rem;
  font-weight: 700;
  overflow-wrap: anywhere;
}

.account-security-callout {
  margin-bottom: 1rem;
  padding: 1rem 1.05rem;
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(94, 32, 40, 0.08) 0%, rgba(255, 255, 255, 0.95) 100%);
  border: 1px solid rgba(94, 32, 40, 0.12);
}

.account-security-callout strong {
  display: block;
  margin-bottom: 0.35rem;
  color: var(--color-green-medium);
}

.account-security-callout p {
  margin: 0;
  color: var(--color-grey-medium);
  line-height: 1.6;
}

.account-form {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  max-width: 34rem;
}

.account-form :deep(.form__field) {
  margin-bottom: 0;
}

.password-requirements {
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
  margin: -0.2rem 0 0.2rem;
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

.account-form__submit {
  align-self: flex-start;
  min-width: 12rem;
}

@media (max-width: 960px) {
  .account-page__hero,
  .account-page__grid {
    grid-template-columns: 1fr;
  }

  .account-form__submit {
    width: 100%;
  }
}
</style>
