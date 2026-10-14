<template>
  <VcBlade
    :title="$t('INVENTORY.PRODUCTS.TITLE', { name: options?.name ?? '' })"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <div class="inventory-filter">
      <VcSwitch
        v-model="inStockOnly"
        :label="$t('INVENTORY.PRODUCTS.IN_STOCK_ONLY')"
      />
    </div>
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      state-key="INVENTORY_CENTER_PRODUCTS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-package', title: $t('INVENTORY.PRODUCTS.EMPTY') }"
      @row-click="(e: { data: Row }) => openInventory(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="image"
        field="product.imgSrc"
        :title="$t('INVENTORY.PRODUCTS.COLUMNS.IMAGE')"
        type="image"
        width="80px"
      />
      <VcColumn
        id="name"
        :title="$t('INVENTORY.PRODUCTS.COLUMNS.NAME')"
        always-visible
      >
        <template #body="{ data }">{{ data.product?.name ?? $t("INVENTORY.PRODUCTS.NO_PRODUCT") }}</template>
      </VcColumn>
      <VcColumn
        id="code"
        field="product.code"
        :title="$t('INVENTORY.PRODUCTS.COLUMNS.CODE')"
      />
      <VcColumn
        id="inStockQuantity"
        field="inventory.inStockQuantity"
        :title="$t('INVENTORY.PRODUCTS.COLUMNS.IN_STOCK')"
        type="number"
        align="end"
        sortable
        always-visible
      />
      <VcColumn
        id="reservedQuantity"
        field="inventory.reservedQuantity"
        :title="$t('INVENTORY.PRODUCTS.COLUMNS.RESERVED')"
        type="number"
        align="end"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useDataTablePagination, useDataTableSort, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcSwitch } from "@vc-shell/framework/ui";
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useInventoryClient, type ProductEntry } from "../api";
import type { ProductInventoryInfo } from "../../api_client/virtocommerce.inventory";

defineBlade({ url: "/fulfillment-center-products", name: "InventoryFulfillmentCenterProducts", permissions: ["inventory:read"] });

type Row = ProductInventoryInfo & { id?: string };

// param: the fulfillment center id
const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { param, options, openBlade, exposeToChildren } = useBlade<{ name?: string }>();
const { hasAccess } = usePermissions();
const client = useInventoryClient();

const items = ref<Row[]>([]);
const totalCount = ref(0);
const activeItemId = ref<string>();
const inStockOnly = ref(true);
const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "INVENTORY_CENTER_PRODUCTS", initialField: "inStockQuantity", initialDirection: "DESC" });
const pagination = useDataTablePagination({ stateKey: "INVENTORY_CENTER_PRODUCTS", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const r = await (await client()).searchProductInventoryInfos({
    fulfillmentCenterIds: [param.value!],
    withPositiveQuantityOnly: inStockOnly.value,
    sort: sortExpression.value,
    skip: pagination.skip,
    take: PAGE_SIZE,
  });
  items.value = (r.results ?? []).map((x) => ({ ...x, id: x.productId }));
  totalCount.value = r.totalCount ?? 0;
});

load();
watch(inStockOnly, () => (pagination.skip ? pagination.setPage(1) : load()));
watch([sortExpression, () => pagination.skip], () => load());

function openInventory(row: Row) {
  activeItemId.value = row.productId;
  openBlade({
    name: "InventoryDetails",
    param: row.productId,
    options: {
      fulfillmentCenterId: param.value,
      title: row.product?.name ?? t("INVENTORY.PRODUCTS.NO_PRODUCT"),
    },
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

// products picked in InventoryProductPicker get an empty stock record in this center (legacy behavior)
async function addProducts(products: ProductEntry[]) {
  const c = await client();
  const ids = products.map((p) => p.id);
  const existing = await c.searchProductInventoryInfos({ fulfillmentCenterIds: [param.value!], productIds: ids, take: ids.length });
  const known = new Set((existing.results ?? []).map((x) => x.productId));
  const added = ids.filter((id) => !known.has(id));
  if (added.length) {
    await c.upsertProductInventories(added.map((productId) => ({ productId, fulfillmentCenterId: param.value, inStockQuantity: 0 })));
  }
  // the added products have no stock: show them (turning the filter off reloads through its watcher)
  if (inStockOnly.value) inStockOnly.value = false;
  else await load();
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("INVENTORY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("INVENTORY.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "inventory:update",
    // the picker searches the catalog (catalog:products:read falls back to catalog:read)
    isVisible: hasAccess(["catalog:read", "catalog:products:read"]),
    clickHandler: () => openBlade({ name: "InventoryProductPicker" }),
  },
]);

exposeToChildren({ reload: () => load(), addProducts });
</script>

<style lang="scss">
.inventory-filter {
  padding: 12px 20px 4px;
}
</style>
