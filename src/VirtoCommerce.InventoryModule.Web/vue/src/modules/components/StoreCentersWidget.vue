<template>
  <!-- Fulfillment centers widget of the Store blade (legacy storeFulfillmentWidget) -->
  <VcWidget
    icon="lucide-warehouse"
    :title="$t('INVENTORY.WIDGETS.FULFILLMENT_CENTERS')"
    :value="count"
    :disabled="!store"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed } from "vue";

type Store = { mainFulfillmentCenterId?: string; additionalFulfillmentCenterIds?: string[] };

// the store shared by the Store blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const store = computed(() => ctx.value.item as Store | undefined);
const count = computed(
  () => new Set([store.value?.mainFulfillmentCenterId, ...(store.value?.additionalFulfillmentCenterIds ?? [])].filter(Boolean)).size,
);
const { openBlade } = useBlade();

function open() {
  if (store.value) openBlade({ name: "InventoryStoreFulfillmentCenters", options: { store: store.value } });
}
</script>
