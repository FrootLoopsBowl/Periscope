export interface PasswordRequirement {
  key: string;
  label: string;
  valid: boolean;
}

const MinimumPasswordLength = 8;
const MinimumUniqueCharacters = 6;
export function getPasswordRequirements(password: string, translate: (key: string) => string, translationPrefix: string): PasswordRequirement[] {
  return [
    {
      key: "length",
      label: translate(`${translationPrefix}.passwordTooShort`),
      valid: password.length >= MinimumPasswordLength
    },
    {
      key: "uppercase",
      label: translate(`${translationPrefix}.passwordRequiresUpper`),
      valid: /[A-Z]/.test(password)
    },
    {
      key: "lowercase",
      label: translate(`${translationPrefix}.passwordRequiresLower`),
      valid: /[a-z]/.test(password)
    },
    {
      key: "digit",
      label: translate(`${translationPrefix}.passwordRequiresDigit`),
      valid: /\d/.test(password)
    },
    {
      key: "special",
      label: translate(`${translationPrefix}.passwordRequiresNonAlphanumeric`),
      valid: /[^A-Za-z0-9]/.test(password)
    },
    {
      key: "unique",
      label: translate(`${translationPrefix}.passwordRequiresUniqueCharacters`),
      valid: new Set(password).size >= MinimumUniqueCharacters
    }
  ];
}
