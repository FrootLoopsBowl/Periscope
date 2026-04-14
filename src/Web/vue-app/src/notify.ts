import { notify } from "@kyvg/vue3-notification";

const DEFAULT_SUCCESS = "Opération réussie"
const DEFAULT_ERROR = "Une erreur est survenue"

export function notifySuccess(text?: string) {
  const payloadText = (text && String(text).trim()) ? text : DEFAULT_SUCCESS
  notify({
    text: payloadText,
    type: "success"
  })
}

export function notifyError(text?: string) {
  const payloadText = (text && String(text).trim()) ? text : DEFAULT_ERROR
  notify({
    text: payloadText,
    type: "error"
  })
}
