<template>
  <EasyDataTable
      :empty-message="t('global.table.noData')"
      :filter-options="filterOptions"
      :headers="headers"
      :hide-footer="isSoloItem"
      :hide-rows-per-page="true"
      :items="items"
      :loading="isLoading"
      :rows-of-page-separator-message="t('global.table.of')"
      :rows-per-page="isSoloItem ? 1 : 10"
      :search-value="searchValue"
      :table-min-height="0"
      alternating
      buttons-pagination
      :body-row-class-name="getBodyRowClassName"
      header-item-class-name="vue3-easy-data-table__header-item"
      theme-color="#5e2028"
      @click-row="handleClickRow"
  >
    <template #item-status="item">
      <div class="tag">
        <p>{{ item.status }}</p>
      </div>
    </template>
    <template #item-lastName="item">
      <span>{{ item.lastName }}</span>
    </template>
    <template #item-actions="item">
      <p v-if="item && item.actions" class="vue3-easy-data-table__actions">
        <router-link
            v-if="item.actions.view"
            v-tippy="t(`global.actions.view`)"
            :to="item.actions.view"
            class="vue3-easy-data-table__action"
            @click.stop
        >
          <IconView class="icon icon--green"/>
        </router-link>
        <router-link
            v-if="item.actions.edit"
            v-tippy="t(`global.actions.update`)"
            :to="item.actions.edit"
            class="vue3-easy-data-table__action"
            @click.stop
        >
          <IconEdit class="icon icon--green"/>
        </router-link>
        <button
            v-if="item.actions.resend && item.id"
            v-tippy="t(`global.actions.resend`)"
            class="vue3-easy-data-table__action"
            type="button"
            @click.stop="handleResend(item)"
        >
          <IconMail class="icon icon--green"/>
        </button>
        <button
            v-if="item.actions.delete && item.id"
            v-tippy="t(`global.actions.delete`)"
            class="vue3-easy-data-table__action"
            type="button"
            @click.stop="handleDelete(item)"
        >
          <IconDelete class="icon icon--green"/>
        </button>
      </p>
    </template>

  </EasyDataTable>
</template>

<script lang="ts" setup>
import type {FilterOption, Header, Item} from "vue3-easy-data-table"
import {useI18n} from "vue3-i18n"
import {useRouter} from "vue-router"
import IconEdit from "@/assets/icons/icon__edit.svg"
import IconDelete from "@/assets/icons/icon__delete.svg"
import IconView from "@/assets/icons/icon__view.svg"
import IconMail from "@/assets/icons/icon__mail.svg"

const {t} = useI18n()
const router = useRouter()

// eslint-disable-next-line
defineProps<{
  headers: Header[],
  items: Item[],
  filterOptions?: FilterOption[],
  isLoading?: boolean,
  searchValue?: string
  isSoloItem?: boolean
}>()

// eslint-disable-next-line
const emit = defineEmits<{
  (event: "delete", item: any): void
  (event: "resend", item: any): void
}>()

function handleDelete(item: any) {
  emit("delete", item)
}

function handleResend(item: any) {
  emit("resend", item)
}

function handleClickRow(item: any) {
  if (!item?.detailLink) return
  router.push(item.detailLink)
}

function getBodyRowClassName(item: any) {
  return item?.detailLink ? "vue3-easy-data-table__row--clickable" : ""
}
</script>
