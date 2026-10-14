<template>
  <VcBlade
    :title="$t('INVENTORY.CENTERS.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="INVENTORY_CENTERS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-warehouse', title: $t('INVENTORY.CENTERS.EMPTY') }"
      @row-click="(e: { data: FulfillmentCenter }) => openDetails(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('INVENTORY.CENTERS.COLUMNS.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="city"
        field="address.city"
        :title="$t('INVENTORY.CENTERS.COLUMNS.CITY')"
      />
      <VcColumn
        id="country"
        field="address.countryName"
        :title="$t('INVENTORY.CENTERS.COLUMNS.COUNTRY')"
      />
      <VcColumn
        id="outerId"
        :title="$t('INVENTORY.CENTERS.COLUMNS.OUTER_ID')"
        :visible="false"
      />
      <VcColumn
        id="modifiedDate"
        :title="$t('INVENTORY.CENTERS.COLUMNS.MODIFIED')"
        type="date"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useInventoryClient } from "../api";
import type { FulfillmentCenter } from "../../api_client/virtocommerce.inventory";

defineBlade({
  url: "/inventory",
  name: "InventoryFulfillmentCenterList",
  isWorkspace: true,
  permissions: ["inventory:access"],
  menuItem: { title: "INVENTORY.MENU.TITLE", icon: "lucide-boxes", priority: 30 },
});

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren } = useBlade();
const { debounce } = useFunctions();
const { showConfirmation } = usePopup();
const client = useInventoryClient();

const items = ref<FulfillmentCenter[]>([]);
const totalCount = ref(0);
const selection = ref<FulfillmentCenter[]>([]);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "INVENTORY_CENTERS", initialField: "name", initialDirection: "ASC" });
const { searchValue } = useTableSearch({ stateKey: "INVENTORY_CENTERS" });
const pagination = useDataTablePagination({ stateKey: "INVENTORY_CENTERS", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchFulfillmentCenters({
    searchPhrase: searchValue.value || undefined,
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });
  items.value = r.results ?? [];
  totalCount.value = r.totalCount ?? 0;
});

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

function openDetails(item?: FulfillmentCenter) {
  activeItemId.value = item?.id;
  openBlade({
    name: "InventoryFulfillmentCenterDetails",
    param: item?.id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("INVENTORY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("INVENTORY.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "inventory:fulfillment:edit",
    clickHandler: () => openDetails(),
  },
  {
    id: "delete",
    title: t("INVENTORY.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "inventory:fulfillment:delete",
    disabled: computed(() => !selection.value.length),
    async clickHandler() {
      if (!(await showConfirmation(t("INVENTORY.ALERTS.DELETE_CENTERS", { count: selection.value.length })))) return;
      await (await client()).deleteFulfillmentCenters(selection.value.map((x) => x.id!));
      selection.value = [];
      await load();
    },
  },
]);

exposeToChildren({ reload: () => load() });
</script>
