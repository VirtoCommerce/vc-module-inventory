<template>
  <!-- One product's stock in one fulfillment center -->
  <VcBlade
    :title="options?.title ?? ''"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="40%"
  >
    <VcContainer>
      <VcForm class="inventory-form tw-space-y-4">
        <VcCard :header="$t('INVENTORY.DETAILS.STOCK')">
          <div class="tw-p-4 tw-space-y-4">
            <VcInput
              v-model="item.inStockQuantity"
              type="number"
              :label="$t('INVENTORY.DETAILS.IN_STOCK')"
              required
            />
            <VcInput
              v-model="item.reservedQuantity"
              type="number"
              :label="$t('INVENTORY.DETAILS.RESERVED')"
            />
            <VcInput
              v-model="item.reorderMinQuantity"
              type="number"
              :label="$t('INVENTORY.DETAILS.REORDER_MIN')"
            />
          </div>
        </VcCard>
        <VcCard :header="$t('INVENTORY.DETAILS.PREORDER')">
          <div class="tw-p-4 tw-space-y-4">
            <VcSwitch
              v-model="item.allowPreorder"
              :label="$t('INVENTORY.DETAILS.ALLOW_PREORDER')"
            />
            <VcInput
              v-model="item.preorderQuantity"
              type="number"
              :label="$t('INVENTORY.DETAILS.PREORDER_QTY')"
              :disabled="!item.allowPreorder"
            />
            <VcDatePicker
              v-model="item.preorderAvailabilityDate"
              type="datetime-local"
              :label="$t('INVENTORY.DETAILS.PREORDER_DATE')"
              :disabled="!item.allowPreorder"
              clearable
            />
          </div>
        </VcCard>
        <VcCard :header="$t('INVENTORY.DETAILS.BACKORDER')">
          <div class="tw-p-4 tw-space-y-4">
            <VcSwitch
              v-model="item.allowBackorder"
              :label="$t('INVENTORY.DETAILS.ALLOW_BACKORDER')"
            />
            <VcInput
              v-model="item.backorderQuantity"
              type="number"
              :label="$t('INVENTORY.DETAILS.BACKORDER_QTY')"
              :disabled="!item.allowBackorder"
            />
            <VcDatePicker
              v-model="item.backorderAvailabilityDate"
              type="datetime-local"
              :label="$t('INVENTORY.DETAILS.BACKORDER_DATE')"
              :disabled="!item.allowBackorder"
              clearable
            />
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCard, VcContainer, VcDatePicker, VcForm, VcInput, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useInventoryClient } from "../api";
import type { InventoryInfo } from "../../api_client/virtocommerce.inventory";

defineBlade({ url: "/inventory-record", name: "InventoryDetails", permissions: ["inventory:read"] });

// param: the product id; options: the fulfillment center and the blade title
const { t } = useI18n({ useScope: "global" });
const { param, options, callParent } = useBlade<{ fulfillmentCenterId: string; title?: string }>();
const client = useInventoryClient();

const item = ref<InventoryInfo>({});
const NUMBERS = ["inStockQuantity", "reservedQuantity", "reorderMinQuantity", "preorderQuantity", "backorderQuantity"] as const;

const form = useBladeForm({
  data: item,
  canSaveOverride: computed(() => item.value.inStockQuantity !== undefined && (item.value.inStockQuantity as unknown) !== ""),
  closeConfirmMessage: computed(() => t("INVENTORY.ALERTS.CLOSE_CONFIRMATION")),
});

const { loading, action: load } = useAsync(async () => {
  const fulfillmentCenterId = options.value!.fulfillmentCenterId;
  const r = await (await client()).searchProductInventoryInfos({ fulfillmentCenterIds: [fulfillmentCenterId], productIds: [param.value!], take: 1 });
  // a center without a record for the product gets one on the first save
  item.value = r.results?.[0]?.inventory ?? { productId: param.value, fulfillmentCenterId, inStockQuantity: 0 };
  form.setBaseline();
});

const { loading: saving, action: save } = useAsync(async () => {
  NUMBERS.forEach((k) => {
    const v = item.value[k] as unknown;
    item.value[k] = v === "" || v === undefined || v === null ? 0 : Number(v);
  });
  await (await client()).updateProductInventory(param.value!, item.value);
  await load();
  callParent("reload");
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("INVENTORY.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "inventory:update",
    disabled: computed(() => !form.canSave.value || saving.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("INVENTORY.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
]);

onMounted(load);
</script>
