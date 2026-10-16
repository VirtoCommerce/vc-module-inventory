<!-- Low stock, in the design's Inventory Management slot (DashboardTables.jsx): the inventory records
     with the least stock on hand. The inventory API has no threshold filter, so it sorts by quantity. -->
<template>
  <DashboardWidgetCard
    :header="$t('INVENTORY.DASHBOARD.LOW_STOCK.TITLE')"
    icon="lucide-clipboard-check"
    :loading="loading"
  >
    <template #actions>
      <button
        type="button"
        class="dash-link-button"
        @click="openList"
      >
        {{ $t("INVENTORY.DASHBOARD.VIEW_ALL") }}
      </button>
    </template>
    <template #content>
      <div
        v-if="rows.length"
        class="dash-table low-stock"
      >
        <table>
          <thead>
            <tr>
              <th>{{ $t("INVENTORY.DASHBOARD.LOW_STOCK.NAME") }}</th>
              <th class="low-stock__sku">{{ $t("INVENTORY.DASHBOARD.LOW_STOCK.SKU") }}</th>
              <th class="low-stock__location">{{ $t("INVENTORY.DASHBOARD.LOW_STOCK.LOCATION") }}</th>
              <th class="low-stock__stock">{{ $t("INVENTORY.DASHBOARD.LOW_STOCK.STOCK") }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in rows"
              :key="row.key"
            >
              <td :title="row.name">
                <span class="low-stock__name dash-table__strong">{{ row.name }}</span>
                <!-- on a narrow card SKU and location move under the name -->
                <span class="low-stock__details dash-table__muted">{{ row.details }}</span>
              </td>
              <td
                class="low-stock__sku dash-table__muted"
                :title="row.sku"
              >
                {{ row.sku }}
              </td>
              <td
                class="low-stock__location dash-table__muted"
                :title="row.location"
              >
                {{ row.location }}
              </td>
              <td>
                <div
                  v-if="row.stock <= 0"
                  class="dash-inline"
                >
                  <VcStatus variant="danger">{{ $t("INVENTORY.DASHBOARD.LOW_STOCK.OUT") }}</VcStatus>
                </div>
                <div
                  v-else
                  class="low-stock__bar"
                  :class="{ 'low-stock__bar--low': row.low }"
                >
                  <span class="low-stock__track"><span :style="{ width: `${row.fill}%` }" /></span>
                  {{ $t("INVENTORY.DASHBOARD.LOW_STOCK.LEFT", { count: row.stock }) }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div
        v-else-if="!loading"
        class="dash-empty"
      >
        <VcIcon
          icon="lucide-boxes"
          size="xl"
        />
        {{ $t("INVENTORY.DASHBOARD.LOW_STOCK.EMPTY") }}
      </div>
    </template>
  </DashboardWidgetCard>
</template>

<script lang="ts" setup>
import { DashboardWidgetCard, useAsync, useBlade } from "@vc-shell/framework";
import { VcIcon, VcStatus } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import type { ProductInventoryInfo } from "../../api_client/virtocommerce.inventory";
import { useInventoryClient } from "../api";

const client = useInventoryClient();
const { openBlade } = useBlade();
const records = ref<ProductInventoryInfo[]>([]);

// the app's error handler shows a failure
const { loading, action: load } = useAsync(
  async () => {
    // the products come resolved on the server, so the widget needs inventory:read only (not catalog read)
    const result = await (await client()).searchProductInventoryInfos({ sort: "inStockQuantity", take: 6 });
    records.value = result.results ?? [];
  },
  { notify: false },
);
onMounted(load);

// the bar fills against twice the reorder point (or 200 units, the design's scale)
const rows = computed(() =>
  records.value.map(({ productId, product, inventory: x = {} }) => {
    const stock = x.inStockQuantity ?? 0;
    const scale = Math.max(200, (x.reorderMinQuantity ?? 0) * 2);
    return {
      key: `${productId}:${x.fulfillmentCenterId}`,
      name: product?.name ?? productId ?? "",
      sku: product?.code ?? "",
      location: x.fulfillmentCenterName ?? "",
      details: [product?.code, x.fulfillmentCenterName].filter(Boolean).join(" · "),
      stock,
      low: stock <= (x.reorderMinQuantity ?? 0),
      fill: Math.max(8, Math.min(100, (stock / scale) * 100)),
    };
  }),
);

// the Inventory workspace (the fulfillment centers), as the menu opens it
const openList = () => openBlade({ name: "InventoryFulfillmentCenterList", isWorkspace: true });
</script>

<style lang="scss" scoped>
// One line per cell (the full text in its title), so six rows fit the card; on a narrow card SKU and location move
// under the name. The table, empty state and link button are the Admin Shell's dashboard classes (dash-*).
.low-stock {
  container-type: inline-size;

  table {
    table-layout: fixed;
  }

  td {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }


  &__sku {
    width: 130px;
  }

  &__location {
    width: 150px;
  }

  &__stock {
    width: 170px;
  }

  &__name,
  &__details {
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__details {
    display: none;
    font-size: 12px;
    line-height: 16px;
    margin-top: 1px;
  }

  // stock bar (DashboardTables.jsx StockBar): dark track, amber fill, red when at/below the reorder point
  &__bar {
    align-items: center;
    color: var(--color-neutral-700);
    display: flex;
    gap: 12px;
    white-space: nowrap;
  }

  &__track {
    background: #2b3a52;
    border-radius: 999px;
    flex: 0 1 134px;
    height: 8px;
    min-width: 32px;
    overflow: hidden;

    [data-theme="dark"] & {
      background: var(--color-neutral-300);
    }

    span {
      background: #f0ad4e;
      border-radius: 999px;
      display: block;
      height: 100%;
    }
  }

  &__bar--low &__track span {
    background: #ff6a6a;
  }

  @container (max-width: 560px) {
    &__sku,
    &__location {
      display: none;
    }

    &__details {
      display: block;
    }

    &__stock {
      width: 160px;
    }

    td {
      padding-bottom: 8px;
      padding-top: 8px;
    }
  }
}
</style>
