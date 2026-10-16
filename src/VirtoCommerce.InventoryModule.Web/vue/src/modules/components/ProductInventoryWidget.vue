<template>
  <!-- Inventory widget of the Catalog product blade (legacy inventoryWidget): total in stock, opens stock per center -->
  <VcWidget
    v-if="hasAccess('inventory:update')"
    icon="lucide-boxes"
    :title="$t('INVENTORY.WIDGETS.INVENTORY')"
    :value="label"
    :disabled="!product?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useInventoryClient } from "../api";

// the product shared by the Catalog blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const product = computed(() => ctx.value.item as { id?: string; name?: string } | undefined);
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();
const client = useInventoryClient();
const label = ref<string>("…");

async function refresh() {
  if (!product.value?.id) return;
  const records = await (await client()).getProductInventories(product.value.id);
  if (!records.length) {
    label.value = "N/A";
    return;
  }
  // total in stock over all fulfillment centers
  label.value = String(records.reduce((sum, x) => sum + (x.inStockQuantity ?? 0), 0));
}

watch(() => product.value?.id, refresh, { immediate: true });

function open() {
  if (!product.value?.id) return;
  openBlade({
    name: "InventoryProductStock",
    param: product.value.id,
    options: { productName: product.value.name },
    onClose: refresh,
  });
}
</script>
