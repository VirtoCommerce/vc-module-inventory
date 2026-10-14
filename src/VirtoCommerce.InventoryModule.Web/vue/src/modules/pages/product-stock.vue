<template>
  <!-- A product's stock in every fulfillment center (opened from the Inventory widget of the Catalog product blade) -->
  <VcBlade
    :title="$t('INVENTORY.STOCK.TITLE', { name: options?.productName ?? '' })"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="INVENTORY_PRODUCT_STOCK"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-warehouse', title: $t('INVENTORY.STOCK.EMPTY') }"
      @row-click="(e: { data: Row }) => openInventory(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="fulfillmentCenterName"
        :title="$t('INVENTORY.STOCK.COLUMNS.CENTER')"
        sortable
        always-visible
      />
      <VcColumn
        id="inStockQuantity"
        :title="$t('INVENTORY.STOCK.COLUMNS.IN_STOCK')"
        type="number"
        align="end"
        sortable
        always-visible
      />
      <VcColumn
        id="reservedQuantity"
        :title="$t('INVENTORY.STOCK.COLUMNS.RESERVED')"
        type="number"
        align="end"
        :visible="false"
      />
      <VcColumn
        id="available"
        :title="$t('INVENTORY.STOCK.COLUMNS.AVAILABLE')"
        type="number"
        align="end"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useDataTablePagination, useDataTableSort, useFunctions, useTableSearch, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useInventoryClient } from "../api";
import type { InventoryInfo } from "../../api_client/virtocommerce.inventory";

defineBlade({ url: "/product-inventory", name: "InventoryProductStock", permissions: ["inventory:read"] });

type Row = InventoryInfo & { available?: number };

// param: the product id
const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { param, options, openBlade, exposeToChildren } = useBlade<{ productName?: string }>();
const { debounce } = useFunctions();
const client = useInventoryClient();

const items = ref<Row[]>([]);
const totalCount = ref(0);
const activeItemId = ref<string>();
const { searchValue } = useTableSearch({ stateKey: "INVENTORY_PRODUCT_STOCK" });
const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "INVENTORY_PRODUCT_STOCK", initialField: "inStockQuantity", initialDirection: "DESC" });
const pagination = useDataTablePagination({ stateKey: "INVENTORY_PRODUCT_STOCK", pageSize: PAGE_SIZE, totalCount });

// one row per fulfillment center, also the ones without a stock record for the product
const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchProductInventories({
    productId: param.value,
    searchPhrase: searchValue.value || undefined,
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });
  items.value = (r.results ?? []).map((x) => ({
    ...x,
    id: x.fulfillmentCenterId,
    available: (x.inStockQuantity ?? 0) - (x.reservedQuantity ?? 0),
  }));
  totalCount.value = r.totalCount ?? 0;
});

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

function openInventory(row: Row) {
  activeItemId.value = row.fulfillmentCenterId;
  openBlade({
    name: "InventoryDetails",
    param: param.value,
    options: { fulfillmentCenterId: row.fulfillmentCenterId, title: row.fulfillmentCenterName },
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("INVENTORY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);

exposeToChildren({ reload: () => load() });
</script>
